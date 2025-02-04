namespace TextAdvance;
public struct ObjectQuestID : IEquatable<ObjectQuestID>
{
    public uint DataID;
    public int[] QuestState;

    public ObjectQuestID(uint dataID, int[] questState)
    {
        this.DataID = dataID;
        this.QuestState = questState;
    }

    public ObjectQuestID(uint dataID)
    {
        this.DataID = dataID;
        this.QuestState = Utils.GetQuestArray().SelectMulti(x => x.Item1, x => x.Item2).ToArray();
    }

    public override bool Equals(object obj)
    {
        return obj is ObjectQuestID iD && this.Equals(iD);
    }

    public bool Equals(ObjectQuestID other)
    {
        return this.DataID == other.DataID &&
               this.QuestState.SequenceEqual(other.QuestState);
    }

    public override int GetHashCode()
    {
        var ret = HashCode.Combine(this.DataID);
        foreach (var x in this.QuestState) ret = HashCode.Combine(ret, x);
        return ret;
    }

    public static bool operator ==(ObjectQuestID left, ObjectQuestID right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ObjectQuestID left, ObjectQuestID right)
    {
        return !(left == right);
    }

    public override readonly string ToString()
    {
        return $"[DataID: {this.DataID}, QuestState: {this.QuestState.Print(",")}]";
    }
}
