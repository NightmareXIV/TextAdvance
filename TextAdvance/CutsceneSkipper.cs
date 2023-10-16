using Dalamud;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using ECommons.Configuration;
using ECommons.DalamudServices.Legacy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TextAdvance
{
    internal unsafe class CutsceneSkipper : IDisposable
    {
        internal delegate void CutsceneHandleInputDelegate(nint a1);
        [Signature("40 53 48 83 EC 20 80 79 29 00 48 8B D9 0F 85", DetourName = nameof(CutsceneHandleInputDetour))]
        internal Hook<CutsceneHandleInputDelegate> CutsceneHandleInputHook;

        internal static readonly string ConditionSig = "75 11 BA ?? ?? ?? ?? 48 8B CF E8 ?? ?? ?? ?? 84 C0 74 52";
        internal static int ConditionOriginalValuesLen => ConditionSig.Split(" ").Length;
        internal nint ConditionAddr;

        internal CutsceneSkipper()
        {
            SignatureHelper.Initialise(this);
            ConditionAddr = Svc.SigScanner.ScanText(ConditionSig);
            PluginLog.Information($"Found cutscene skip condition address at {ConditionAddr}");
            CutsceneHandleInputHook?.Enable();
        }

        public void Dispose()
        {
            CutsceneHandleInputHook?.Dispose();
        }

        internal void CutsceneHandleInputDetour(nint a1)
        {
            var called = false;
            try
            {
                if (!P.Locked && (!P.IsDisableButtonHeld() || !P.IsEnabled()) && P.IsEnabled() && P.config.GetEnableCutsceneEsc())
                {
                    var skippable = *(nint*)(a1 + 56) != 0;
                    if (skippable)
                    {
                        SafeMemory.WriteBytes(ConditionAddr, [0xEB]);
                        CutsceneHandleInputHook.Original(a1);
                        called = true;
                        SafeMemory.WriteBytes(ConditionAddr, [0x75]);
                    }
                }
            }
            catch(Exception e)
            {
                e.Log();
            }
            if(!called)
            {
                CutsceneHandleInputHook.Original(a1);
            }
        }
    }
}
