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
    internal unsafe class SplatoonHandler
    {
        static class Markers
        {
            public static readonly uint[] MSQ = [71201, 71202, 71203, 71204, 71205, 70983];
            public static readonly uint[] ImportantSideProgress = [71343, 71344, 71345];
            public static readonly uint[] ImportantSideInitiate = [71341, 71342];
            public static readonly uint[] SideProgress = [71223, 71225, 71224];
            public static readonly uint[] SideInitiate = [];
            public static readonly string[] EventObjNameWhitelist = ["Destination", "指定地点", "Zielort"];
            public static readonly uint[] EventObjWhitelist = [2010816, 2011073, 2011072, 2011071];
        }

        internal List<Element> Elements = [];
        int CurrentCnt = 0;

        internal SplatoonHandler() 
        {
            Splatoon.SetOnConnect(OnConnect);
        }

        internal void Tick()
        {
            if(P.config.GetQTAQuestEnabled() && Splatoon.IsConnected())
            {
                foreach(var x in Svc.Objects)
                {
                    var id = x.Struct()->NamePlateIconId;
                    if (Markers.MSQ.Contains(id) || Markers.ImportantSideProgress.Contains(id) || Markers.SideProgress.Contains(id))
                    {
                        var e = GetFreeElement(x.Position);
                        Splatoon.DisplayOnce(e);
                    }
                    else if (x.ObjectKind == ObjectKind.EventObj && x.IsTargetable && (Markers.EventObjWhitelist.Contains(x.DataId) || Markers.EventObjNameWhitelist.ContainsIgnoreCase(x.Name.ToString()) ) )
                    {
                        var e = GetFreeElement(x.Position);
                        Splatoon.DisplayOnce(e);
                    }
                    else if(x.IsTargetable)
                    {
                        var display = false;
                        if (x.ObjectKind == ObjectKind.EventObj && P.config.EObjFinder)
                        {
                            display = P.config.FinderKey == LimitedKeys.None || IsKeyPressed(P.config.FinderKey);
                        }
                        if (x.ObjectKind == ObjectKind.EventNpc && P.config.ENpcFinder)
                        {
                            display = P.config.FinderKey == LimitedKeys.None || IsKeyPressed(P.config.FinderKey);
                        }
                        if (display)
                        {
                            var e = GetFreeElement(x.Position);
                            Splatoon.DisplayOnce(e);
                        }
                    }
                }
            }
        }

        void OnConnect() => Reset();

        internal void Reset()
        {
            Elements.Clear();
            CurrentCnt = 0;
        }

        Element GetFreeElement(Vector3 pos)
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
