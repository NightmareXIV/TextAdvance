
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace TextAdvance.Executors;

internal static unsafe class ExecSkipTalk
{
    internal static bool IsEnabled = false;

    private static string lastText = string.Empty;
    private static string lastSpeaker = string.Empty;
    private static long lineFirstSeenTime = 0;
    private static bool currentLineIsVoiced = false;

    internal static void Init()
    {
        Svc.AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "Talk", Click);
        Svc.AddonLifecycle.RegisterListener(AddonEvent.PostUpdate, "Talk", Click);
        Svc.AddonLifecycle.RegisterListener(AddonEvent.PreFinalize, "Talk", OnFinalize);
    }

    internal static void Shutdown()
    {
        Svc.AddonLifecycle.UnregisterListener(AddonEvent.PostSetup, "Talk", Click);
        Svc.AddonLifecycle.UnregisterListener(AddonEvent.PostUpdate, "Talk", Click);
        Svc.AddonLifecycle.UnregisterListener(AddonEvent.PreFinalize, "Talk", OnFinalize);
        Reset();
    }

    internal static void Reset()
    {
        lastText = string.Empty;
        lastSpeaker = string.Empty;
        lineFirstSeenTime = 0;
        currentLineIsVoiced = false;
    }

    private static void OnFinalize(AddonEvent type, AddonArgs args)
    {
        Reset();
    }

    private static void Click(AddonEvent type, AddonArgs args)
    {
        if (!IsEnabled) return;

        var addonTalk = (AddonTalk*)args.Addon.Address;
        if (addonTalk == null || !addonTalk->AtkUnitBase.IsVisible) return;

        var textNode = addonTalk->AtkTextNode228;
        var speakerNode = addonTalk->AtkTextNode220;
        var text = textNode != null ? textNode->NodeText.ToString() : string.Empty;
        var speaker = speakerNode != null ? speakerNode->NodeText.ToString() : string.Empty;

        if (string.IsNullOrWhiteSpace(text)) return;

        if (!P.InCutscene)
        {
            new AddonMaster.Talk(args.Addon).Click();
            return;
        }

        if (!C.GetSkipTalkInCutscenes()) return;

        if (C.GetSkipVoicedDialogue())
        {
            new AddonMaster.Talk(args.Addon).Click();
            return;
        }

        var now = Environment.TickCount64;

        if (text != lastText || speaker != lastSpeaker)
        {
            lastText = text;
            lastSpeaker = speaker;
            lineFirstSeenTime = now;
            currentLineIsVoiced = S.VoiceTracker != null && (now - S.VoiceTracker.LastVoicePlayTime <= 500);
        }
        else if (!currentLineIsVoiced && S.VoiceTracker != null && (now - S.VoiceTracker.LastVoicePlayTime <= 500) && S.VoiceTracker.LastVoicePlayTime >= lineFirstSeenTime)
        {
            currentLineIsVoiced = true;
        }

        if (currentLineIsVoiced) return;
        if (now - lineFirstSeenTime < 150) return;

        new AddonMaster.Talk(args.Addon).Click();
    }
}
