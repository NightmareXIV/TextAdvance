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
using static TextAdvance.ImGuiEx;

namespace TextAdvance
{
    internal class ConfigGui : Window
    {
        TextAdvance p;
        public ConfigGui(TextAdvance plugin) : base("TextAdvance config")
        {
            this.p = plugin;
            this.SizeConstraints = new WindowSizeConstraints()
            {
                MinimumSize = new Vector2(400, 200),
                MaximumSize = new Vector2(99999, 99999),
            };
        }

        public override void Draw()
        {
            ImGui.Checkbox("Enable plugin (non-persistend)", ref p.Enabled);
            ImGui.SetNextItemWidth(200f);
            ImGuiEnumCombo("Button to hold to temporarily disable plugin when active", ref p.config.TempDisableButton);
            ImGuiEnumCombo("Button to hold to temporarily enable plugin when inactive", ref p.config.TempEnableButton);
            ImGui.Text("Auto-enable plugin when you log in with characters:");
            string dele = null;
            foreach(var s in p.config.AutoEnableNames)
            {
                ImGui.Text(s);
                ImGui.SameLine();
                if (ImGui.SmallButton("Delete"))
                {
                    dele = s;
                }
            }
            if(ImGui.Button("Add current character") && Svc.ClientState.LocalPlayer != null)
            {
                p.config.AutoEnableNames.Add(Svc.ClientState.LocalPlayer.Name.ToString() + "@" + Svc.ClientState.LocalPlayer.HomeWorld.GameData.Name);
            }
            if(dele != null)
            {
                p.config.AutoEnableNames.Remove(dele);
                dele = null;
            }
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
    }
}
