using UnityEngine;

[CreateAssetMenu(fileName = "Objective_PlaceOnTile", menuName = "Objective/PlaceOnTile")]
public class Objective_PlaceOnTile : ObjectiveBase
{
    public Vector3Int[] targetTiles;

    public override bool CheckClearCondition()
    {
        if (targetTiles is not null)
        {
            foreach (Vector3Int currentTile in targetTiles)
            {
                if (!CheckTile(currentTile)) return false;
            }
        }

        return true;
    }

    public virtual bool CheckTile(in Vector3Int targetTile)
    {
        CharacterBase currentCharacter = TileManager.GetCharacterOnTile(targetTile);
        return currentCharacter == null;
    }
}
