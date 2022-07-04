using Dalamud.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextAdvance
{
    [Serializable]
    public class Config : IPluginConfiguration
    {
        public int Version { get; set; } = 2;
        public HashSet<string> AutoEnableNames = new();
        public Button TempEnableButton = Button.NONE;
        public Button TempDisableButton = Button.NONE;
        public bool EnableQuestAccept = true;
        public bool EnableQuestComplete = true;
        public bool EnableRequestHandin = true;
        public bool EnableCutsceneEsc = true;
        public bool EnableCutsceneSkipConfirm = true;
        public bool EnableTalkSkip = true;
        public TerritoryConfig MainConfig = new();
        public Dictionary<uint, TerritoryConfig> TerritoryConditions = new();
    }

    public enum Button
    {
        NONE, ALT, SHIFT, CTRL
    }

    [Serializable]
    public class TerritoryConfig
    {
        public bool EnableQuestAccept = true;
        public bool EnableQuestComplete = true;
        public bool EnableRequestHandin = true;
        public bool EnableCutsceneEsc = true;
        public bool EnableCutsceneSkipConfirm = true;
        public bool EnableTalkSkip = true;
    }
}
