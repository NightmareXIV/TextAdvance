using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextAdvance
{
    [StructLayout(LayoutKind.Explicit)]
    internal unsafe struct AgentCutscene
    {
        internal static AgentCutscene* Instance() => (AgentCutscene*)AgentModule.Instance()->GetAgentByInternalId(AgentId.Cutscene);

        [FieldOffset(0)] internal AgentInterface AgentInterface;
        [FieldOffset(41)] internal int IsCutsceneSkipped;
        [FieldOffset(44)] internal nint unk_44;
        [FieldOffset(56)] internal nint* unk_56;
    }
}
