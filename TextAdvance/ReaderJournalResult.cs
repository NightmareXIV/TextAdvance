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
        internal List<OptionalReward> OptionalRewards => this.Loop<OptionalReward>(82, 1, 5);

        internal unsafe class OptionalReward(nint UnitBasePtr, int BeginOffset = 0) : AtkReader(UnitBasePtr, BeginOffset)
        {
            internal uint ItemID => (this.ReadUInt(0) ?? 0) % 1000000;
            internal bool IsHQ => (this.ReadUInt(0) ?? 0) > 1000000;
            internal uint IconID => this.ReadUInt(6) ?? 0;
            internal uint Amount => this.ReadUInt(11) ?? 0;
            internal string Name => this.ReadSeString(16)?.ExtractText();
        }
    }
}
