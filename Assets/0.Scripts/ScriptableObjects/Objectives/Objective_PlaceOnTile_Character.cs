using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Objective_PlaceOnTile_Character", menuName = "Objective/PlaceOnTile/Character")]
public class Objective_PlaceOnTile_Character : Objective_PlaceOnTile
{
	public bool onlyPlayerCharacters;

	public override bool CheckTile(in Vector3Int targetTile, ref GameObject clearClaimer)
	{
		CharacterBase currentCharacter = TileManager.GetCharacterOnTile(targetTile);
		if (!currentCharacter) return false;
		bool result;
		if (onlyPlayerCharacters) result = currentCharacter.Controller == BattleManager.GetLocalPlayerOnBattle();
		else result = true;
		if (result) clearClaimer ??= currentCharacter.gameObject;
		return result;
	}

	protected override IEnumerator OnObjectiveStart()
	{
		yield return base.OnObjectiveStart();

		UI_IngameAreaVisalizer.ClaimIngameAreaBlock("ObjectiveShower");
		foreach(Vector3Int currentTile in targetTiles)
		{
			Vector3 tilePosition = TileManager.GetTileWorldPosition(currentTile);
			tilePosition.z = CameraManager.cameraDistance;
			yield return CameraManager.ClaimLockSmooth(tilePosition, 3.0f, 0.5f, 1.0f);
		}
		yield return CameraManager.ClaimUnlockSmooth(0.2f);
		UI_IngameAreaVisalizer.ClaimIngameAreaUnblock("ObjectiveShower");
	}
}
