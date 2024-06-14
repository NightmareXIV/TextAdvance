using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.GeneratedSheets;
using Lumina.Excel.GeneratedSheets2;
using System.Diagnostics;
using TextAdvance.Executors;
using Level = Lumina.Excel.GeneratedSheets.Level;
using QuestLinkMarker = FFXIVClientStructs.FFXIV.Client.UI.Agent.QuestLinkMarker;

namespace TextAdvance.Gui;

internal static unsafe class TabDebug
{
    internal static void Draw()
    {
        if(ImGui.CollapsingHeader("Quest markers"))
        {
            var markers = AgentHUD.Instance()->MapMarkers.Span;
            for (int i = 0; i < markers.Length; i++)
            {
                var marker = markers[i];
                if(ThreadLoadImageHandler.TryGetIconTextureWrap(marker.IconId, false, out var tex))
                {
                    ImGui.Image(tex.ImGuiHandle, tex.Size);
                }
                ImGuiEx.Text($"{marker.IconId} / {marker.X} / {marker.Y} / {marker.Z}");
            }
        }
        if(ImGui.Button("copy target descriptor"))
        {
            if (Svc.Targets.Target != null) Copy(new ObjectDescriptor(Svc.Targets.Target, true).AsCtorString());
        }
        if(ImGui.CollapsingHeader("Auto interact"))
        {
            ImGuiEx.Text($"Target: {ExecAutoInteract.WasInteracted(Svc.Targets.Target)}");
            ImGuiEx.Text($"Auto interacted objects: {ExecAutoInteract.InteractedObjects.Print("\n")}");
        }
        if (ImGui.CollapsingHeader("Quests"))
        {
            ImGuiEx.Text($"{Utils.GetQuestArray().Print("\n")}");
        }
        if (ImGui.CollapsingHeader("Map"))
        {
            ImGuiEx.Text($"Flight addr: {P.Memory.FlightAddr:X16} / {(P.Memory.FlightAddr - Process.GetCurrentProcess().MainModule.BaseAddress):X}");
            ImGuiEx.Text($"CanFly: {P.Memory.IsFlightProhibited(P.Memory.FlightAddr)}");
            var questLinkSpan = new ReadOnlySpan<QuestLinkMarker>(AgentMap.Instance()->MiniMapQuestLinkContainer.Markers, AgentMap.Instance()->MiniMapQuestLinkContainer.MarkerCount);

            foreach(var q in questLinkSpan)
            {
                ImGuiEx.Text($"{q.TooltipText.ToString()}");
                if (Svc.Data.GetExcelSheet<Level>().GetRow(q.LevelId) is not { X: var x, Y: var y, Z: var z }) continue;
                ImGuiEx.Text($"   {x}, {y} {z}");
            }
        }
        if(TryGetAddonByName<AtkUnitBase>("JournalResult", out var addon) && IsAddonReady(addon))
        {
            var canvas = addon->UldManager.NodeList[7];
            var r = new ReaderJournalResult(addon);
            ImGuiEx.Text($"Rewards: \n{r.OptionalRewards.Select(x => $"ID:{x.ItemID} / Icon:{x.IconID} / Amount:{x.Amount} / Name:{x.Name} ").Print("\n")}");
            for (int i = 0; i < 5; i++)
            {
                if (ImGui.Button($"{i}"))
                {
                    P.Memory.PickRewardItemUnsafe((nint)canvas->GetComponent(), i);
                }
            }
        }
    }
}
