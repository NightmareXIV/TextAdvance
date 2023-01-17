using ClickLib.Bases;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace TextAdvance;

public sealed unsafe class ClickJournalAccept : ClickBase<ClickJournalAccept, AtkUnitBase>
{
    public ClickJournalAccept(IntPtr addon = default)
        : base("JournalAccept", addon)
    {
    }

    public static implicit operator ClickJournalAccept(IntPtr addon) => new(addon);

    public static ClickJournalAccept Using(IntPtr addon) => new(addon);

    public void Accept(AtkComponentButton* acceptButton)
    {
        ClickAddonButton(acceptButton, 1);
    }
}
