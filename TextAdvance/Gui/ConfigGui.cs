using Dalamud.Interface.Internal.Notifications;
using ECommons.Configuration;
using NightmareUI;

namespace TextAdvance.Gui;

internal class ConfigGui : Window, IDisposable
{
    TextAdvance p;
    internal WindowSystem ws = new();
    public ConfigGui(TextAdvance plugin) : base("TextAdvance config")
    {
        p = plugin;
        SizeConstraints = new WindowSizeConstraints()
        {
            MinimumSize = new Vector2(400, 200),
            MaximumSize = new Vector2(99999, 99999),
        };
        ws.AddWindow(this);
        Svc.PluginInterface.UiBuilder.Draw += ws.Draw;
    }

    public override void Draw()
    {
        if (ImGui.BeginChild("Child", new(ImGui.GetContentRegionAvail().X, ImGui.GetContentRegionAvail().Y - ImGui.GetFrameHeightWithSpacing())))
        {
            ImGuiEx.EzTabBar("TextAdvanceTab",
                ("General config", TabConfig.Draw, null, true),
                ("Target indicators", TabSplatoon.Draw, null, true),
                ("Auto-enable", TabChars.Draw, null, true),
                ("Per area config", TabTerritory.Draw, null, true),
                ("Debug", TabDebug.Draw, ImGuiColors.DalamudGrey3, true)
                );
        }
        ImGui.EndChild();
        FundingBanner.Draw(ref P.config.DisplayFunding);
    }

    public override void PreDraw()
    {
        base.PreDraw();
    }

    public override void OnClose()
    {
        EzConfig.Save();
        Svc.PluginInterface.UiBuilder.AddNotification("Configuration saved", "TextAdvance", NotificationType.Success);
        base.OnClose();
    }

    public void Dispose()
    {
        Svc.PluginInterface.UiBuilder.Draw -= ws.Draw;
    }
}
