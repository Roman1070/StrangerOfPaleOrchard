using System;

public enum ItemGroup
{
    Resource,
    Weapon,
    Gear
}
public enum WeaponType
{
    OneHanded,
    TwoHanded
}
[Serializable]
public class ItemGroupDef : Def
{
    public ItemGroup Group;
    public WeaponType WeaponType;
}
