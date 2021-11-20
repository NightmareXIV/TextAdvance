using Dalamud.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextAdvance
{
    [Serializable]
    internal class Config : IPluginConfiguration
    {
        public int Version { get; set; } = 1;
        public HashSet<string> AutoEnableNames = new();
        public Button TempEnableButton = Button.NONE;
        public Button TempDisableButton = Button.NONE;
    }

    public enum Button
    {
        NONE, ALT, SHIFT, CTRL
    }
}
