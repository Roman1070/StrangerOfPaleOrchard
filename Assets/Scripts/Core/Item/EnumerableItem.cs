using System;

[Serializable]
public struct EnumerableItem
{
    public string Id;
    public int Count;

    public EnumerableItem(string id, int count)
    {
        Id = id;
        Count = count;
    }
}
