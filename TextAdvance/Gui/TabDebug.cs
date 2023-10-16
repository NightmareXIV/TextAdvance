using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextAdvance.Gui
{
    internal static unsafe class TabDebug
    {
        internal static void Draw()
        {
            var agent = AgentCutscene.Instance();
            ImGuiEx.Text($"Active: {agent->AgentInterface.IsAgentActive()}");
            ImGuiEx.Text($"Skipped: {agent->IsCutsceneSkipped}");
            ImGuiEx.Text($"unk_44: {agent->unk_44}");
            ImGuiEx.Text($"unk_56: {*agent->unk_56}");
        }
    }
}
