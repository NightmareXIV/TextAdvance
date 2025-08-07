using ECommons.EzHookManager;

namespace TextAdvance;

public unsafe class Memory
{
    private delegate nint AtkComponentJournalCanvas_ReceiveEventDelegate(nint a1, ushort a2, int a3, JournalCanvasInputData* a4, void* a5);
    [EzHook("48 89 5C 24 ?? 48 89 74 24 ?? 4C 89 4C 24 ?? 55")]
    private EzHook<AtkComponentJournalCanvas_ReceiveEventDelegate> AtkComponentJournalCanvas_ReceiveEventHook;

    private Memory()
    {
        EzSignatureHelper.Initialize(this);
    }

    internal void PickRewardItemUnsafe(nint canvas, int index)
    {
        if (canvas < 1024)
        {
            DuoLog.Error($"TextAdvance anti-crash: canvas={canvas:X16} invalid data detected. Please report to developer.");
            return;
        }
        var emptyBytes = stackalloc byte[50];
        var data = stackalloc JournalCanvasInputData[1];
        this.AtkComponentJournalCanvas_ReceiveEventDetour(canvas, 9, 7 + index, data, emptyBytes);
    }

    private nint AtkComponentJournalCanvas_ReceiveEventDetour(nint a1, ushort a2, int a3, JournalCanvasInputData* a4, void* a5)
    {
        PluginLog.Information($"AtkComponentJournalCanvas_ReceiveEventDetour: Canvas ptr: {a1:X16}");
        var ret = this.AtkComponentJournalCanvas_ReceiveEventHook.Original(a1, a2, a3, a4, a5);
        try
        {
            var d = (JournalCanvasInputData*)a4;
            PluginLog.Debug($"AtkComponentJournalCanvas_ReceiveEventDetour: {(nint)a1:X16}, {a2}, {a3}, {(nint)a4:X16} ({d->Unk0}, {d->Unk4}, {d->Unk6}), {(nint)a5:X16}");
        }
        catch (Exception e)
        {
            e.Log();
        }
        return ret;
    }
}
