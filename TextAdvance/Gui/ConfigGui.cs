using Dalamud.Interface.Internal.Notifications;
using ECommons.Configuration;

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
        KoFiButton.DrawRight();
        ImGuiEx.EzTabBar("TextAdvanceTab",
            ("General config", TabConfig.Draw, null, true),
            ("Target indicators", TabSplatoon.Draw, null, true),
            ("Auto-enable", TabChars.Draw, null, true),
            ("Per area config", TabTerritory.Draw, null, true),
            ("Navigation", TabNavmesh.Draw, null, true),
            ("Debug", TabDebug.Draw, ImGuiColors.DalamudGrey3, true)
            );
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
