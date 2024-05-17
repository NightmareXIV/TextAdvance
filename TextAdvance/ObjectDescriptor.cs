using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.Configuration;
using ECommons.ExcelServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextAdvance;
public struct ObjectDescriptor : IEquatable<ObjectDescriptor>
{
    public uint DataID;
    public uint TerritoryType;
    public ObjectKind Kind;
    public Vector3 Position;
    public string Name;

    public ObjectDescriptor(uint dataID, uint territoryType, ObjectKind kind, Vector3 position)
    {
        this.DataID = dataID;
        this.TerritoryType = territoryType;
        this.Kind = kind;
        this.Position = position;
    }

    public ObjectDescriptor(GameObject o, bool name = false)
    {
        this.DataID = o.DataId;
        this.TerritoryType = Svc.ClientState.TerritoryType;
        this.Kind = o.ObjectKind;
        this.Position = o.Position;
        if(name) this.Name = o.Name.ToString();
    }

    public string Serialize()
    {
        return EzConfig.DefaultSerializationFactory.Serialize(this, false);
    }

    public string AsCtorString()
    {
        return $"new({this.DataID}, {this.TerritoryType}, ObjectKind.{this.Kind}, new({this.Position.X:F1}f, {this.Position.Y:F1}f, {this.Position.Z:F1}f)), //{this.Name} at {ExcelTerritoryHelper.GetName(this.TerritoryType)}\n";
    }

    public override bool Equals(object obj)
    {
        return obj is ObjectDescriptor descriptor && this.Equals(descriptor);
    }

    public bool Equals(ObjectDescriptor other)
    {
        return this.DataID == other.DataID &&
               this.TerritoryType == other.TerritoryType &&
               this.Kind == other.Kind &&
               Vector3.DistanceSquared(this.Position, other.Position) < 1f;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(this.DataID, this.TerritoryType, this.Kind, this.Position);
    }

    public static bool operator ==(ObjectDescriptor left, ObjectDescriptor right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ObjectDescriptor left, ObjectDescriptor right)
    {
        return !(left == right);
    }
}
