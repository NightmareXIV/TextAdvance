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
    internal class ConfigGui : Window, IDisposable
    {
        TextAdvance p;
        WindowSystem ws = new();
        public ConfigGui(TextAdvance plugin) : base("TextAdvance config")
        {
            this.p = plugin;
            this.SizeConstraints = new WindowSizeConstraints()
            {
                MinimumSize = new Vector2(400, 200),
                MaximumSize = new Vector2(99999, 99999),
            };
            ws.AddWindow(this);
            Svc.PluginInterface.UiBuilder.Draw += ws.Draw;
        }

        public override void Draw()
        {
            ImGui.Checkbox("Enable plugin (non-persistent)", ref p.Enabled);
            ImGui.Separator();
            ImGui.Text("Button to hold to temporarily disable plugin when active:");
            ImGui.SetNextItemWidth(200f);
            ImGuiEnumCombo("##HoldDisable", ref p.config.TempDisableButton);
            ImGui.Text("Button to hold to temporarily enable plugin when inactive:");
            ImGui.SetNextItemWidth(200f);
            ImGuiEnumCombo("##HoldEnable", ref p.config.TempEnableButton);
            ImGui.Separator();
            ImGui.Text("Functions: ");
            ImGui.Checkbox("Automatic quest accept", ref p.config.EnableQuestAccept);
            ImGui.Checkbox("Automatic quest complete", ref p.config.EnableQuestComplete);
            ImGui.Checkbox("Automatic talk skip", ref p.config.EnableTalkSkip);
            ImGui.Checkbox("Semi-automatic request handin", ref p.config.EnableRequestHandin);
            ImGui.Checkbox("Automatic ESC press during cutscene", ref p.config.EnableCutsceneEsc);
            ImGui.Checkbox("Automatic cutscene skip confirmation", ref p.config.EnableCutsceneSkipConfirm);
            ImGui.Separator();
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

        public void Dispose()
        {
            Svc.PluginInterface.UiBuilder.Draw -= ws.Draw;
        }
    }
}
