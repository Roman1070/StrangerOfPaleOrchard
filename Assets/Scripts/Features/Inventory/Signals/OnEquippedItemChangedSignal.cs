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
    public PlayerView Player;
    public int ItemNumericId;
    public int ItemLevel;
    public bool IsMine;

    public OnEquippedItemChangedSignal(Item item, ItemSlot slot)
    {
        Item = item;
        Slot = slot;
        IsMine = true;
    }

    public OnEquippedItemChangedSignal(int itemId,int itemLevel, ItemSlot slot, PlayerView player)
    {
        ItemNumericId = itemId;
        Slot = slot;
        Player = player;
        ItemLevel = itemLevel;
        IsMine = false;
    }

}
