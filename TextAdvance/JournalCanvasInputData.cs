namespace TextAdvance
{
    [StructLayout(LayoutKind.Explicit, Size = 100)]
    internal unsafe struct JournalCanvasInputData
    {
        [FieldOffset(0)] internal int Unk0;
        [FieldOffset(4)] internal byte Unk4;
        [FieldOffset(6)] internal byte Unk6;
    }
}
