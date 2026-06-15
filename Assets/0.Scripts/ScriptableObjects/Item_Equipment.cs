using UnityEngine;

[CreateAssetMenu(fileName = "Item_Equipment", menuName = "Item/Equipment")]
public class Item_Equipment : ItemContainer
{
    public virtual void OnEquip(CharacterBase target)
    {

    }

    public virtual void OnUnequip(CharacterBase target) 
    { 
        
    }
}
