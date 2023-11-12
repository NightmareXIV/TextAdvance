﻿using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using ECommons.DalamudServices.Legacy;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextAdvance
{
    internal unsafe class Memory : IDisposable
    {
        delegate nint AtkComponentJournalCanvas_ReceiveEventDelegate(nint a1, ushort a2, int a3, JournalCanvasInputData* a4, void* a5);
        [Signature("48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 48 89 7C 24 ?? 41 56 48 83 EC 50 48 8B F1 0F B7 C2", DetourName =nameof(AtkComponentJournalCanvas_ReceiveEventDetour))]
        Hook<AtkComponentJournalCanvas_ReceiveEventDelegate> AtkComponentJournalCanvas_ReceiveEventHook;

        internal Memory()
        {
            SignatureHelper.Initialise(this);
            //AtkComponentJournalCanvas_ReceiveEventHook?.Enable();
        }

        public void Dispose()
        {
            AtkComponentJournalCanvas_ReceiveEventHook?.Dispose();
        }

        internal void PickRewardItemUnsafe(nint canvas, int index)
        {
            var emptyBytes = stackalloc byte[50];
            var data = stackalloc JournalCanvasInputData[1];
            AtkComponentJournalCanvas_ReceiveEventDetour(canvas, 9, 5 + index, data, emptyBytes);
        }

        nint AtkComponentJournalCanvas_ReceiveEventDetour(nint a1, ushort a2, int a3, JournalCanvasInputData* a4, void* a5)
        {
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