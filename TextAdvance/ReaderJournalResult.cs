using ECommons.UIHelpers;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextAdvance
{
    internal unsafe class ReaderJournalResult(AtkUnitBase* UnitBase, int BeginOffset = 0) : AtkReader(UnitBase, BeginOffset)
    {
        internal List<OptionalReward> OptionalRewards => Loop<OptionalReward>(84, 1, 5);

        internal unsafe class OptionalReward(nint UnitBasePtr, int BeginOffset = 0) : AtkReader(UnitBasePtr, BeginOffset)
        {
            internal uint ItemID => (ReadUInt(0) ?? 0) % 1000000;
            internal bool IsHQ => (ReadUInt(0) ?? 0) > 1000000;
            internal uint IconID => ReadUInt(6) ?? 0;
            internal uint Amount => ReadUInt(11) ?? 0;
            internal string Name => ReadSeString(16)?.ExtractText();
        }
    }
}
