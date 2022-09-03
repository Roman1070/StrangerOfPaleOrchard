using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerModelUpdateService : LoadableService
{
    private InventoryService _inventoryService;
    private PlayerView _player;
    private EquipedWeaponOffsetConfig _offsetConfig;
    private OtherPlayersContainer _otherPlayers;
    private RenderSpace _renderModel;
    private Dictionary<PlayerView, Dictionary<Item, GameObject[]>> _cachedModels = new Dictionary<PlayerView, Dictionary<Item, GameObject[]>>();
    private Dictionary<PlayerView, Dictionary<ItemSlot, GameObject[]>> _equippedGear = new Dictionary<PlayerView, Dictionary<ItemSlot, GameObject[]>>();

    public PlayerModelUpdateService(SignalBus signalBus, RenderSpace renderSpace, PlayerView player, EquipedWeaponOffsetConfig offsetConfig, OtherPlayersContainer otherPlayers) : base(signalBus)
    {
        _renderModel = renderSpace;
        _player = player;
        _offsetConfig = offsetConfig;
        _otherPlayers = otherPlayers;
        _cachedModels = new Dictionary<PlayerView, Dictionary<Item, GameObject[]>>()
        {
            {_player, new Dictionary<Item, GameObject[]>() }
        };
        _equippedGear = new Dictionary<PlayerView, Dictionary<ItemSlot, GameObject[]>>()
        {
            {
                _player , new Dictionary<ItemSlot, GameObject[]>()
                {
                    { ItemSlot.Weapon, null }
                }
            }
        };
        _otherPlayers.OnPlayerJoined += OnPlayerJoined;
        _otherPlayers.OnPlayerRemoved += OnPlayerLeft;
        signalBus.Subscribe<OnEquippedItemChangedSignal>(OnEquipementChanged, this);
    }

    private void OnPlayerJoined(PlayerView player)
    {
        _equippedGear.Add(player, new Dictionary<ItemSlot, GameObject[]>() { { ItemSlot.Weapon, null } });
        _cachedModels.Add(player, new Dictionary<Item, GameObject[]>());
        _signalBus.FireSignal(new OnEquippedItemChangedSignal(1, 1, 0, player));
    }
    private void OnPlayerLeft(PlayerView player)
    {
        _equippedGear.Remove(player);
        _cachedModels.Remove(player);
    }

    private void OnEquipementChanged(OnEquippedItemChangedSignal obj)
    {
        if (obj.IsMine) 
            obj.Player = _player; 
        else
            obj.Item = _inventoryService.GetItem(obj.ItemNumericId);

        if (_equippedGear[obj.Player].Keys.Contains(obj.Slot) && _equippedGear[obj.Player][obj.Slot]!=null)
        {
            foreach(var ob in _equippedGear[obj.Player][obj.Slot])
                ob.SetActive(false);
        }

        if (_cachedModels[obj.Player].Keys.Contains(obj.Item))
        {
            foreach (var ob in _cachedModels[obj.Player][obj.Item])
                 ob.SetActive(true);
        }
        else
        {
            int level = _inventoryService.ItemsLevels[obj.Item.Id];
            if (!obj.IsMine) level = obj.ItemLevel;
            var prefab = obj.Item.PrafabDef.Prefabs[level-1].Prefab;

            var newModels = new GameObject[] { GameObject.Instantiate(prefab, obj.Player.WeaponsHolder), GameObject.Instantiate(prefab, _renderModel.HandAnchor) };
            switch (obj.Slot)
            {
                case ItemSlot.Weapon:
                    foreach(var model in newModels)
                    {
                        model.transform.localPosition = _offsetConfig.GetOffsetData(obj.Item.Id).PositionOffset;
                        model.transform.localEulerAngles = _offsetConfig.GetOffsetData(obj.Item.Id).RotationOffest;
                        model.transform.localScale = _offsetConfig.GetOffsetData(obj.Item.Id).Scale;
                    }
                    break;
            }
            _cachedModels[obj.Player].Add(obj.Item, newModels);
        }
        _equippedGear[obj.Player][obj.Slot] = _cachedModels[obj.Player][obj.Item];

        if(obj.IsMine) _player.Photon.RPC("OnEquipementChangedRemote" , Photon.Pun.RpcTarget.Others , obj.Item.NumericId, _inventoryService.ItemsLevels[obj.Item.Id]);
    }

    public override void OnServicesLoaded(params LoadableService[] services)
    {
        _inventoryService = services.First(s => s is InventoryService) as InventoryService;
        _signalBus.FireSignal(new OnEquippedItemChangedSignal(_inventoryService.GetItem("WEAPON_SWORD_1"), ItemSlot.Weapon));
    }
}
