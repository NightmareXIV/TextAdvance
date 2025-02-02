using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.GameHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextAdvance.Navmesh;
public class MoveData
{
    public Vector3 Position;
    public uint DataID;
    public bool NoInteract;
    public bool? Mount = null;
    public bool? Fly = null;

    public MoveData(Vector3 destination, uint dataID, bool noInteract)
    {
        this.Position = destination;
        this.DataID = dataID;
        this.NoInteract = noInteract;
    }

    public IGameObject GetIGameObject(float maxDistance = 20f)
    {
        if (!Player.Available) return null;
        if (this.DataID == 0)
        {
            if (Vector3.Distance(Player.Object.Position, this.Position) > maxDistance) return null;
            foreach (var x in Svc.Objects.OrderBy(z => Vector3.Distance(Player.Object.Position, z.Position)))
            {
                if (x.IsMTQ()) return x;
            }
            foreach (var x in Svc.Objects.OrderBy(z => Vector3.Distance(Player.Object.Position, z.Position)))
            {
                if (x.ObjectKind.EqualsAny(ObjectKind.EventNpc, ObjectKind.EventObj) && x.IsTargetable && Vector3.Distance(x.Position, this.Position) < 10f)
                {
                    return x;
                }
            }
        }
        else
        {
            foreach (var x in Svc.Objects)
            {
                if (x.DataId == this.DataID && Vector3.Distance(this.Position, x.Position) < 1f) return x;
            }
        }
        return null;
    }
}
