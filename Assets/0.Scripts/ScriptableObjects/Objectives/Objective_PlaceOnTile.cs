using UnityEngine;

[CreateAssetMenu(fileName = "Objective_PlaceOnTile", menuName = "Objective/PlaceOnTile/Object")]
public class Objective_PlaceOnTile : ObjectiveBase
{
    public Vector3Int[] targetTiles;

    public override bool CheckClearCondition(TurnBaseInfo lastTurn, out GameObject clearClaimer)
    {
		GameObject resultClaimer = null;
		if (targetTiles is not null)
		{
			foreach (Vector3Int currentTile in targetTiles)
			{
				if (!CheckTile(currentTile, ref resultClaimer))
				{
					clearClaimer = null;
					return false;
				}
			}
		}
		clearClaimer = resultClaimer;
        return true;
	}

    public virtual bool CheckTile(in Vector3Int targetTile, ref GameObject clearClaimer)
    {
		GameObject currentObject = TileManager.GetObjectOnTile(targetTile);
        bool result = currentObject;
		if (result) clearClaimer ??= currentObject;
		return result;
    }
}
