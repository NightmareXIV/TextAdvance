using ECommons.SplatoonAPI;

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
        private int CurrentCnt = 0;

        internal SplatoonHandler()
        {
            Splatoon.SetOnConnect(this.OnConnect);
        }

        private void OnConnect() => this.Reset();

        internal void Reset()
        {
            this.Elements.Clear();
            this.CurrentCnt = 0;
        }

        public Element GetFreeElement(Vector3 pos)
        {
            if (this.CurrentCnt < this.Elements.Count)
            {
                var ret = this.Elements[this.CurrentCnt];
                this.CurrentCnt++;
                ret.SetRefCoord(pos);
                return ret;
            }
            else
            {
                var ret = new Element(ElementType.CircleAtFixedCoordinates)
                {
                    radius = 0,
                };
                this.Elements.Add(ret);
                this.ApplySettings(ret);
                this.CurrentCnt = this.Elements.Count;
                ret.SetRefCoord(pos);
                return ret;
            }
        }

        private void ApplySettings(Element e)
        {
            e.thicc = C.GetQTAQuestThickness();
            e.color = C.GetQTAQuestColor().ToUint();
            e.tether = C.GetQTAQuestTether();
        }

    }
}
