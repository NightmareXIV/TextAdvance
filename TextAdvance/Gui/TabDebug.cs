using Dalamud.Game.Gui.Toast;
using Dalamud.Game.Network.Structures;
using Dalamud.Memory;
using ECommons.Automation;
using ECommons.Automation.LegacyTaskManager;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.Sheets;
using System.Diagnostics;
using TextAdvance.Executors;
using Level = Lumina.Excel.Sheets.Level;
using QuestLinkMarker = FFXIVClientStructs.FFXIV.Client.UI.Agent.QuestLinkMarker;

namespace TextAdvance.Gui;

internal static unsafe class TabDebug
{
    private static TaskManager TestTaskManager;
    internal static void Draw()
    {
        if (ImGui.CollapsingHeader("IPC test"))
        {
            ImGuiEx.Text($"""
                IsEnabled {S.IPCTester.IsEnabled()}
                GetEnableQuestAccept {S.IPCTester.GetEnableQuestAccept()}
                GetEnableQuestComplete {S.IPCTester.GetEnableQuestComplete()}
                GetEnableRewardPick {S.IPCTester.GetEnableRewardPick()}
                GetEnableCutsceneEsc {S.IPCTester.GetEnableCutsceneEsc()}
                GetEnableCutsceneSkipConfirm {S.IPCTester.GetEnableCutsceneSkipConfirm()}
                GetEnableRequestHandin {S.IPCTester.GetEnableRequestHandin()}
                GetEnableRequestFill {S.IPCTester.GetEnableRequestFill()}
                GetEnableTalkSkip {S.IPCTester.GetEnableTalkSkip()}
                GetEnableAutoInteract {S.IPCTester.GetEnableAutoInteract()}
                IsPaused {S.IPCTester.IsPaused()}
                """);
        }
        if (ImGui.CollapsingHeader("External control test"))
        {
            var opts = Ref<ExternalTerritoryConfig>.Get("", () => new());
            ImGuiEx.Checkbox("EnableAutoInteract", ref opts.EnableAutoInteract);
            ImGuiEx.Checkbox("EnableCutsceneEsc", ref opts.EnableCutsceneEsc);
            ImGuiEx.Checkbox("EnableCutsceneSkipConfirm", ref opts.EnableCutsceneSkipConfirm);
            ImGuiEx.Checkbox("EnableQuestAccept", ref opts.EnableQuestAccept);
            ImGuiEx.Checkbox("EnableQuestComplete", ref opts.EnableQuestComplete);
            ImGuiEx.Checkbox("EnableRequestFill", ref opts.EnableRequestFill);
            ImGuiEx.Checkbox("EnableRequestHandin", ref opts.EnableRequestHandin);
            ImGuiEx.Checkbox("EnableRewardPick", ref opts.EnableRewardPick);
            ImGuiEx.Checkbox("EnableTalkSkip", ref opts.EnableTalkSkip);
            ImGuiEx.Text($"Is in external control: {S.IPCTester.IsInExternalControl()}");
            if (ImGui.Button("Enable external control (Plugin1)")) DuoLog.Information(S.IPCTester.EnableExternalControl("Plugin1", opts).ToString());
            if (ImGui.Button("Enable external control (Plugin2)")) DuoLog.Information(S.IPCTester.EnableExternalControl("Plugin2", opts).ToString());
            if (ImGui.Button("Disable external control (Plugin1)")) DuoLog.Information(S.IPCTester.DisableExternalControl("Plugin1").ToString());
            if (ImGui.Button("Disable external control (Plugin2)")) DuoLog.Information(S.IPCTester.DisableExternalControl("Plugin2").ToString());
        }
        if (ImGui.CollapsingHeader("Cutscene"))
        {

        }
        if (ImGui.CollapsingHeader("Request"))
        {
            if (TryGetAddonByName<AddonRequest>("Request", out var request) && IsAddonReady((AtkUnitBase*)request))
            {
                ImGuiEx.Text($"{request->EntryCount}");
            }
        }
        if (ImGui.Button("Install hook")) Callback.InstallHook();
        if (ImGui.Button("UnInstall hook")) Callback.UninstallHook();
        if (ImGui.CollapsingHeader("Antistuck"))
        {
            ImGuiEx.Text($"""
                Last position: {S.MoveManager.LastPosition}
                Last update: {S.MoveManager.LastPositionUpdate} ({Environment.TickCount64 - S.MoveManager.LastPositionUpdate} ms ago)
                IsRunning: {P.NavmeshManager.IsRunning()}
                Animation locked: {Player.IsAnimationLocked} / {Player.AnimationLock}
                """);
        }
        if (ImGui.CollapsingHeader("Quest markers"))
        {
            var markers = AgentHUD.Instance()->MapMarkers.AsSpan();
            for (var i = 0; i < markers.Length; i++)
            {
                var marker = markers[i];
                if (ThreadLoadImageHandler.TryGetIconTextureWrap(marker.IconId, false, out var tex))
                {
                    ImGui.Image(tex.ImGuiHandle, tex.Size);
                }
                ImGuiEx.Text($"{marker.IconId} / {marker.X} / {marker.Y} / {marker.Z} / {Vector3.Distance(Player.Position, new(marker.X, marker.Y, marker.Z))}");
                ImGui.Separator();
            }
        }
        if (ImGui.Button("copy target descriptor"))
        {
            if (Svc.Targets.Target != null) Copy(new ObjectDescriptor(Svc.Targets.Target, true).AsCtorString());
        }
        if (ImGui.CollapsingHeader("Auto interact"))
        {
            ImGuiEx.Text($"Target: {ExecAutoInteract.WasInteracted(Svc.Targets.Target)}");
            ImGuiEx.Text($"Auto interacted objects: {ExecAutoInteract.InteractedObjects.Print("\n")}");
        }
        if (ImGui.CollapsingHeader("Quests"))
        {
            ImGuiEx.Text($"{Utils.GetQuestArray().Print("\n")}");
        }
        if (ImGui.CollapsingHeader("Reward pick"))
        {
            if (TryGetAddonByName<AtkUnitBase>("JournalResult", out var addon) && IsAddonReady(addon))
            {
                var canvas = addon->UldManager.NodeList[7];
                var r = new ReaderJournalResult(addon);
                ImGuiEx.Text($"Rewards: \n{r.OptionalRewards.Select(x => $"ID:{x.ItemID} / Icon:{x.IconID} / Amount:{x.Amount} / Name:{x.Name} ").Print("\n")}");
                for (var i = 0; i < 5; i++)
                {
                    if (ImGui.Button($"{i}"))
                    {
                        P.Memory.PickRewardItemUnsafe((nint)canvas->GetComponent(), i);
                    }
                }
                if (ImGui.Button("Stress test"))
                {
                    TestTaskManager ??= new();
                    for (var i = 0; i < 1000; i++)
                    {
                        var x = i % 5;
                        TestTaskManager.Enqueue(() => P.Memory.PickRewardItemUnsafe((nint)canvas->GetComponent(), x));
                    }
                }
                if (TestTaskManager != null)
                {
                    ImGuiEx.Text($"Task {TestTaskManager.MaxTasks - TestTaskManager.NumQueuedTasks}/{TestTaskManager.MaxTasks}");
                }
                if (ImGui.Button("Stop stress test"))
                {
                    TestTaskManager.Abort();
                }
            }
        }
    }
}
