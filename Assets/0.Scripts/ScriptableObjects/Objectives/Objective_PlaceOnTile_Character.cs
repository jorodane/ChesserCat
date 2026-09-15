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


}
