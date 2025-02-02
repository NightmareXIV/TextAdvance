using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using ECommons.GameFunctions;
using ECommons.Interop;
using ECommons.SplatoonAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Dalamud.Interface.Utility.Raii.ImRaii;

namespace TextAdvance
{
    public unsafe class SplatoonHandler
    {
        public static class Markers
        {
            public static readonly uint[] MSQ = [71201, 71202, 71203, 71204, 71205, 70983];
            public static readonly uint[] ImportantSideProgress = [71343, 71344, 71345];
            public static readonly uint[] ImportantSideInitiate = [71341, 71342];
            public static readonly uint[] SideProgress = [71223, 71225, 71224];
            public static readonly uint[] SideInitiate = [];
            public static readonly string[] EventObjNameWhitelist = ["Destination", "指定地点", "Zielort"];
            public static readonly uint[] EventObjWhitelist = [2010816, 2011073, 2011072, 2011071];

            public static class Map
            {
                public static readonly uint[] MSQ = [71001, 71002, 71003, 71005, 70961, 70963];
                public static readonly uint[] MSQXZ = [60490, 60494];
                public static readonly uint[] ImportantSideProgress = [70995, 71025, 71143, 71145];
            }
        }

        internal List<Element> Elements = [];
        int CurrentCnt = 0;

        internal SplatoonHandler()
        {
            Splatoon.SetOnConnect(OnConnect);
        }

        void OnConnect() => Reset();

        internal void Reset()
        {
            Elements.Clear();
            CurrentCnt = 0;
        }

        public Element GetFreeElement(Vector3 pos)
        {
            if (CurrentCnt < Elements.Count)
            {
                var ret = Elements[CurrentCnt];
                CurrentCnt++;
                ret.SetRefCoord(pos);
                return ret;
            }
            else
            {
                var ret = new Element(ElementType.CircleAtFixedCoordinates)
                {
                    radius = 0,
                };
                Elements.Add(ret);
                ApplySettings(ret);
                CurrentCnt = Elements.Count;
                ret.SetRefCoord(pos);
                return ret;
            }
        }

        void ApplySettings(Element e)
        {
            e.thicc = P.config.GetQTAQuestThickness();
            e.color = P.config.GetQTAQuestColor().ToUint();
            e.tether = P.config.GetQTAQuestTether();
        }

    }
}
