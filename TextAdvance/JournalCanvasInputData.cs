using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextAdvance
{
    [StructLayout(LayoutKind.Explicit)]
    internal unsafe struct JournalCanvasInputData
    {
        [FieldOffset(0)] internal int Unk0;
        [FieldOffset(4)] internal byte Unk4;
        [FieldOffset(6)] internal byte Unk6;
    }
}
