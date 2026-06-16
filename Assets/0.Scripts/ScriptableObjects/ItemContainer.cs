using System;
using UnityEngine;

public enum ItemType
{
    Miscellaneous   = 0, 
    Material        = 10, 
    Quest           = 90, 
    Important       = 100,
    Consumable      = 400, 
	Equipment       = 500, 
}

[CreateAssetMenu(fileName = "ItemContainer", menuName = "Item/ItemBase")]
public class ItemContainer : InfoContainer
{
    [Header("Item Base Info")]
    public int id;
    [Space]
    [Header("Item Detail")]
    public ItemType type;
    public int maxStack;

    public virtual int CompareByType(ItemContainer other)
    {
        if (other == null) return 1;

        // - : 왼쪽이 작음
        // 0 : 같음
        // + : 왼쪽이 큼
        //          3                1    =  2
        //          7                7    =  0
        //          2                5    = -3
        int result = type - other.type;
        if (result != 0) return result;
        return id - other.id;
    }

    public virtual int CompareByType(ItemSlot mySlot, ItemSlot otherSlot)
    {
        return default;
    }
}
