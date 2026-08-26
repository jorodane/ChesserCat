using System.Collections.Generic;
using UnityEngine;

public class ChessAIBaseController : AIController
{
	protected override void Think()
	{
		Dictionary<string, List<PossibleActionInfo>> possibleActions = new();

		foreach(var currrentCharacter in Characters)
		{
			foreach (var current in currrentCharacter.GetPossibleActions())
			{
				if(!possibleActions.TryGetValue(current.tag, out List<PossibleActionInfo> currentList)) currentList = possibleActions[current.tag] = new();
				currentList.Add(current);
			}
		}

		if (possibleActions.TryGetValue("Attack", out List<PossibleActionInfo> attackList))
		{
			int randomAttack = Random.Range(0, attackList.Count);
			PossibleActionInfo selectedAttack = attackList[randomAttack];

			CharacterBase attackCharacter = selectedAttack.from.Owner;
			BattleManager.ClaimAddFinalTurn(TurnActionBuilder.MakeTurnInfo_Attack(this, attackCharacter, attackCharacter.CurrentTilePosition, selectedAttack.location));
			//BattleManager.ClaimTurnEnd(this);
		}
		else if (possibleActions.TryGetValue("Move", out List<PossibleActionInfo> moveList))
		{
			int randomMove = Random.Range(0, moveList.Count);
			PossibleActionInfo selectedMove = moveList[randomMove];

			CharacterBase moveCharacter = selectedMove.from.Owner;
			BattleManager.ClaimAddFinalTurn(TurnActionBuilder.MakeTurnInfo_Move(this, moveCharacter, moveCharacter.CurrentTilePosition, selectedMove.location));
			//BattleManager.ClaimTurnEnd(this);
		}
	}

	public override void TurnRequested()
	{
		Think();
	}
}
