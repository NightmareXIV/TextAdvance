using ECommons.UIHelpers;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace TextAdvance.Readers;
public unsafe class ReaderJournalAccept(AtkUnitBase* UnitBase, int BeginOffset = 0) : AtkReader(UnitBase, BeginOffset)
{
    public uint QuestId => this.ReadUInt(266) ?? 0;
}
