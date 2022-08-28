using UnityEngine;

public enum ItemSlot
{
    Weapon,
    Helmet,
    Body,
    Gloves,
    Legs,
    Boots,
    Consumable1,
    Consumable2
}

public class OnEquippedItemChangedSignal : ISignal
{
    public Item Item;
    public ItemSlot Slot;

    public OnEquippedItemChangedSignal(Item item, ItemSlot slot)
    {
        Item = item;
        Slot = slot;
    }
}
