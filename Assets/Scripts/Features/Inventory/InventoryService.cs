using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Inventory
{
    public WeaponType CurrentWeaponType;
}

public class InventoryService : LoadableService
{
    public ItemsMap ItemsMap { get; private set; }
    private Dictionary<string, int> _itemsCount;

    public Dictionary<string, int> ItemsCount
    {
        get
        {
            if (_itemsCount == null)
            {
                _itemsCount = new Dictionary<string, int>();
                foreach (var item in ItemsMap.Items)
                {
                    _itemsCount.Add(item.Id, 0);
                }
            }
            return _itemsCount;
        }
        private set { }
    }

    private Dictionary<string, int> _itemsLevels;

    public Dictionary<string, int> ItemsLevels
    {
        get
        {
            if (_itemsLevels == null)
            {
                _itemsLevels = new Dictionary<string, int>();
                foreach (var item in ItemsMap.Items)
                {
                    _itemsLevels.Add(item.Id, item.Item.Definitions.Contains(typeof(ItemGearScoreDef)) ? 1 : 0);
                }
            }
            return _itemsLevels;
        }
        private set { }
    }

    private List<InventoryControllerBase> _controllers;
    private Inventory _inventory;

    public Inventory Inventory
    { 
        get
        {
            if (_inventory == null)
            {
                _inventory = new Inventory();
                _inventory.CurrentWeaponType = WeaponType.OneHanded;
            }
            return _inventory;
        } 
    }

    public InventoryService(SignalBus signalBus, ItemsMap itemsMap) : base(signalBus)
    {
        ItemsMap = itemsMap;
        _signalBus.Subscribe<OnItemCountChangedSignal>(ChangeItemCount, this);
        _signalBus.Subscribe<OnEquippedItemChangedSignal>(OnEquipementChanged, this);
        _signalBus.FireSignal(new OnItemCountChangedSignal(new EnumerableItem[]
        {
            new EnumerableItem("WEAPON_SWORD_1",1),
            new EnumerableItem("WEAPON_SWORD_2",1),
            new EnumerableItem("WEAPON_SWORD_3",1),
            new EnumerableItem("WEAPON_SWORD_4",1),
            new EnumerableItem("WEAPON_SWORD_5",1),
         }));
        InitControllers();
    }

    private void OnEquipementChanged(OnEquippedItemChangedSignal obj)
    {
        if (obj.Item == null) return;

        if (obj.Slot == ItemSlot.Weapon)
            Inventory.CurrentWeaponType = obj.Item.GroupDef.WeaponType;
    }

    public void InitControllers()
    {
        new InventoryEquipementController(_signalBus, ItemsMap, this);
    }

    private void ChangeItemCount(OnItemCountChangedSignal signal)
    {
        foreach (var item in signal.Items)
        {
            ItemsCount[item.Id] += item.Count;
        }

    }

    public Sprite GetIcon(string id)
    {
        var item = GetItem(id);
        if (item.GroupDef.Group == ItemGroup.Resource)
            return item.IconDef.Icon;

        return item.PrafabDef.Prefabs.First(p => p.Level == ItemsLevels[id]).Icon;
    }

    public int GetGearScore(string id) 
    {
        var item = GetItem(id);
        return item.GearScoreDef.Mappings.First(_ => _.Level == ItemsLevels[id]).GearScore; 
    }
    

    public int GetItemCount(string id) => ItemsCount[id];

    public Item GetItem(string id) => ItemsMap.Items.First(_ => _.Id == id).Item;
    public Item GetItem(int id)=>ItemsMap.Items.First(_ => _.Item.NumericId == id).Item;

    public override void OnServicesLoaded(params LoadableService[] services)
    {

    }
}
