using Dalamud.Interface;
using Dalamud.Interface.Internal.Notifications;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TextAdvance.Gui
{
    internal class ConfigGui : Window, IDisposable
    {
        TextAdvance p;
        WindowSystem ws = new();
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
            ImGuiEx.EzTabBar("TextAdvanceTab",
                ("General config", TabConfig.Draw, null, true),
                ("Auto-enable", TabChars.Draw, null, true),
                ("Per area config", TabTerritory.Draw, null, true),
                ("Contribute", Donation.DonationTabDraw, ImGuiColors.DalamudYellow, true)
                );
        }

        public override void PreDraw()
        {
            base.PreDraw();
        }

        public override void OnClose()
        {
            Svc.PluginInterface.SavePluginConfig(p.config);
            Svc.PluginInterface.UiBuilder.AddNotification("Configuration saved", "TextAdvance", NotificationType.Success);
            base.OnClose();
        }

        public void Dispose()
        {
            Svc.PluginInterface.UiBuilder.Draw -= ws.Draw;
        }
    }
}
