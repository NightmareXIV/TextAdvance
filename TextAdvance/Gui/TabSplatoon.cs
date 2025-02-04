namespace TextAdvance.Gui
{
    public static class TabSplatoon
    {
        public static void Draw()
        {
            ImGuiEx.TextWrapped("These functions require Splatoon plugin installed and enabled.");
            if (Svc.PluginInterface.InstalledPlugins.TryGetFirst(x => x.InternalName == "Splatoon", out var plugin))
            {
                if (plugin.IsLoaded)
                {
                    ImGuiEx.TextWrapped(EColor.Green, $"You have Splatoon v{plugin.Version} installed and enabled.");
                }
                else
                {
                    ImGuiEx.TextWrapped(EColor.Red, $"You have Splatoon v{plugin.Version} installed but not enabled.");
                }
            }
            else
            {
                ImGuiEx.TextWrapped(EColor.Red, $"You do not have Splatoon installed.");
                if (ImGui.Button("Get Splatoon")) ShellStart("https://puni.sh/plugin/Splatoon");
            }
            ImGui.Checkbox($"Display quest target indicators", ref C.MainConfig.QTIQuestEnabled);
            ImGui.ColorEdit4($"Quest target indicator color", ref C.MainConfig.QTIQuestColor, ImGuiColorEditFlags.NoInputs);
            ImGui.Checkbox($"Quest target indicator tether", ref C.MainConfig.QTIQuestTether);
            ImGuiEx.SetNextItemWidthScaled(60f);
            ImGui.DragFloat($"Quest target indicator thickness", ref C.MainConfig.QTIQuestThickness, 0.02f, 1f, 10f);
            ImGui.Separator();
            ImGui.Checkbox($"Enable event object finder", ref C.EObjFinder);
            ImGui.Checkbox($"Enable event NPC finder", ref C.ENpcFinder);
            ImGuiEx.SetNextItemWidthScaled(150f);
            ImGuiEx.EnumCombo("Display only while holding key", ref C.FinderKey);
        }
    }
}
