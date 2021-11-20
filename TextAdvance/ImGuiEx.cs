using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextAdvance
{
    internal class ImGuiEx
    {
        static public void ImGuiEnumCombo<T>(string name, ref T refConfigField) where T : IConvertible
        {
            var values = Enum.GetValues(typeof(T)).Cast<T>().Select(x => x.ToString()).ToArray();
            var num = Convert.ToInt32(refConfigField);
            ImGui.Combo(name, ref num, values, values.Length);
            refConfigField = Enum.GetValues(typeof(T)).Cast<T>().ToArray()[num];
        }
    }
}
