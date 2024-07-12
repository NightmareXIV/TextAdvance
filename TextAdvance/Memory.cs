using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Hooking;
using Dalamud.Memory;
using Dalamud.Utility.Signatures;
using ECommons.DalamudServices.Legacy;
using ECommons.EzHookManager;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextAdvance
{
    internal unsafe class Memory
    {
        delegate nint AtkComponentJournalCanvas_ReceiveEventDelegate(nint a1, ushort a2, int a3, JournalCanvasInputData* a4, void* a5);
        EzHook<AtkComponentJournalCanvas_ReceiveEventDelegate> AtkComponentJournalCanvas_ReceiveEventHook;

        internal delegate nint IsFlightProhibitedDelegate(nint a1);
        //internal IsFlightProhibitedDelegate IsFlightProhibited = EzDelegate.Get<IsFlightProhibitedDelegate>("48 89 5C 24 ?? 57 48 83 EC 20 48 8B 1D ?? ?? ?? ?? 48 8B F9 48 85 DB 0F 84 ?? ?? ?? ?? 80 3D");
        //internal nint FlightAddr = Svc.SigScanner.GetStaticAddressFromSig("48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 84 C0 75 11");

        internal delegate nint AddonTalk_ReceiveEventDelegate(nint a1, ushort a2, nint a3, nint a4, nint a5);
        [EzHook("40 53 48 83 EC 40 0F B7 C2", false)]
        EzHook<AddonTalk_ReceiveEventDelegate> AddonTalk_ReceiveEventHook;

        internal nint AddonTalk_ReceiveEventDetour(nint a1, ushort a2, nint a3, nint a4, nint a5)
        {
            try
            {
                var memory = MemoryHelper.ReadRaw(a5, 40);
                PluginLog.Information($"TalkEvent: {a1:X16}, {a2}, {a3:X16}, {a4:X16}, {a5:X16}");
                var e = (AtkEvent*)a4;
                PluginLog.Information($"""

                    L {(nint)e->Listener:X16}
                    F {e->Flags}
                    E {(nint)e->NextEvent:X16}
                    N {(nint)e->Node:X16}
                    P {e->Param}
                    T {(nint)e->Target:X16}
                    {(nint)(&AtkStage.Instance()->AtkEventTarget):X16}
                    """);
            }
            catch(Exception e)
            {
                e.Log();
            }
            return AddonTalk_ReceiveEventHook.Original(a1, a2, a3, a4, a5);
        }

        internal Memory()
        {
            AtkComponentJournalCanvas_ReceiveEventHook = new("48 89 5C 24 ?? 48 89 74 24 ?? 48 89 7C 24 ?? 4C 89 4C 24 ?? 55", AtkComponentJournalCanvas_ReceiveEventDetour);
            EzSignatureHelper.Initialize(this);
        }

        internal void PickRewardItemUnsafe(nint canvas, int index)
        {
            if(canvas < 1024)
            {
                DuoLog.Error($"TextAdvance anti-crash: canvas={canvas:X16} invalid data detected. Please report to developer.");
                return;
            }
            var emptyBytes = stackalloc byte[50];
            var data = stackalloc JournalCanvasInputData[1];
            AtkComponentJournalCanvas_ReceiveEventDetour(canvas, 9, 7 + index, data, emptyBytes);
        }

        nint AtkComponentJournalCanvas_ReceiveEventDetour(nint a1, ushort a2, int a3, JournalCanvasInputData* a4, void* a5)
        {
            PluginLog.Information($"AtkComponentJournalCanvas_ReceiveEventDetour: Canvas ptr: {a1:X16}");
            var ret = AtkComponentJournalCanvas_ReceiveEventHook.Original(a1, a2, a3, a4, a5);
            try
            {
                var d = (JournalCanvasInputData*)a4;
                PluginLog.Debug($"AtkComponentJournalCanvas_ReceiveEventDetour: {(nint)a1:X16}, {a2}, {a3}, {(nint)a4:X16} ({d->Unk0}, {d->Unk4}, {d->Unk6}), {(nint)a5:X16}");
            }
            catch(Exception e)
            {
                e.Log();
            }
            return ret;
        }
    }
}
