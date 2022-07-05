using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextAdvance.Gui
{
    internal static class TabContribute
    {
        internal static void Draw()
        {
            ImGuiEx.TextWrapped("If you have found this plugin useful and wish to contribute, you may send any amount of the following " +
                "cryptocurrencies to any of my wallets which are listed below:");
            Donation.PrintDonationInfo();
        }
    }
}
