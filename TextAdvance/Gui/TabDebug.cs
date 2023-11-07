using FFXIVClientStructs.FFXIV.Component.GUI;

namespace TextAdvance.Gui;

internal static unsafe class TabDebug
{
    internal static void Draw()
    {
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
