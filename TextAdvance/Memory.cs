using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Hooking;
using Dalamud.Memory;
using Dalamud.Utility.Signatures;
using ECommons.DalamudServices.Legacy;
using ECommons.EzHookManager;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Common.Lua;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextAdvance.Executors;

namespace TextAdvance
{
    internal unsafe class Memory
    {
        delegate nint AtkComponentJournalCanvas_ReceiveEventDelegate(nint a1, ushort a2, int a3, JournalCanvasInputData* a4, void* a5);
        EzHook<AtkComponentJournalCanvas_ReceiveEventDelegate> AtkComponentJournalCanvas_ReceiveEventHook;

        internal delegate nint IsFlightProhibitedDelegate(nint a1);
        internal IsFlightProhibitedDelegate IsFlightProhibited = EzDelegate.Get<IsFlightProhibitedDelegate>("40 53 48 83 EC 20 48 8B 1D ?? ?? ?? ?? 48 85 DB 0F 84 ?? ?? ?? ?? 80 3D");
        internal nint FlightAddr = Svc.SigScanner.TryScanText("48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 84 C0 75 11", out var result)?result:default;

        internal delegate nint AddonTalk_ReceiveEventDelegate(nint a1, ushort a2, nint a3, nint a4, nint a5);
        [EzHook("40 53 48 83 EC 40 0F B7 C2", false)]
        EzHook<AddonTalk_ReceiveEventDelegate> AddonTalk_ReceiveEventHook;

        /*[EzHook("48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 50 48 8B F1 48 8D 4C 24 ?? E8 ?? ?? ?? ?? 48 8B 4C 24 ??", false)]
        internal EzHook<EnqueueSnipeTaskDelegate> SnipeHook = null!;
        internal delegate ulong EnqueueSnipeTaskDelegate(EventSceneModuleImplBase* scene, lua_State* state);*/
        internal Memory()
        {
            try
            {
                AtkComponentJournalCanvas_ReceiveEventHook = new("48 89 5C 24 ?? 48 89 74 24 ?? 4C 89 4C 24 ?? 55", AtkComponentJournalCanvas_ReceiveEventDetour);
                AtkComponentJournalCanvas_ReceiveEventHook.Enable();
                EzSignatureHelper.Initialize(this);
            }
            catch(Exception e)
            {
                e.Log();
            }
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

        /*ulong SnipeDetour(EventSceneModuleImplBase* scene, lua_State* state)
        {
            PluginLog.Information($"{nameof(SnipeDetour)}: {state->top->tt} {state->top->value.n}");
            var ret = SnipeHook.Original.Invoke(scene, state);
            try
            {
                if (ExecAutoSnipe.IsEnabled)
                {
                    var val = state->top;
                    val->tt = 3;
                    val->value.n = 1;
                    state->top += 1;
                    PluginLog.Debug($"{nameof(SnipeDetour)}: {state->top->tt} {state->top->value.n} {state->top->value}");
                    return 1;
                }
                else
                    return ret;
            }
            catch
            {
                return ret;
            }
        }*/
    }
}
