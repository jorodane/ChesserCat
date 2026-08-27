using System.Collections.Generic;
using UnityEngine;

public static class TurnActionBuilder
{
    public static TurnActionInfo[] BuildActionArray(this IEnumerable<TurnActionInfo> progress)
    {
        List<TurnActionInfo> result = new ();

        try
        {
            foreach (TurnActionInfo currentAction in progress)
            {
                if (currentAction is null) continue;
                currentAction.GoNext(false);
                result.Add(currentAction);
            }
        }
        finally
        {
            for (int i = result.Count - 1; i >= 0; --i)
            {
                result[i].GoPrev(false);
            }
        }

        return result.ToArray();
    }

    public static TurnBaseInfo MakeTurnInfo_Move(int wantTurnCount, ControllerBase wantPlayer, CharacterBase wantCharacter, in Vector3Int wantStart, in Vector3Int wantDestination) => new TurnBaseInfo()
    {
        turnContext = $"{wantCharacter.DisplayInitial}{TileManager.GetTileText(wantDestination)}",
        turnIndex = wantTurnCount,
        player = wantPlayer,
        playerID = BattleManager.GetPlayerID(wantPlayer),
        character = wantCharacter,
        characterID = wantCharacter ? wantCharacter.GetID() : -1,
        start = wantStart,
        destination = wantDestination,
        actionList = wantCharacter.StartCharacterMove(wantPlayer, wantStart, wantDestination).BuildActionArray()
    };
    public static TurnBaseInfo MakeTurnInfo_Move(ControllerBase wantPlayer, CharacterBase wantCharacter, in Vector3Int wantStart, in Vector3Int wantDestination)
    => MakeTurnInfo_Move(BattleManager.GetTurnPassed() + 1, wantPlayer, wantCharacter, wantStart, wantDestination);

    public static TurnBaseInfo MakeTurnInfo_Move(ControllerBase wantPlayer, CharacterBase wantCharacter, in Vector3Int wantDestination)
    => MakeTurnInfo_Move(BattleManager.GetTurnPassed() + 1, wantPlayer, wantCharacter, wantCharacter.CurrentTilePosition, wantDestination);

    public static TurnBaseInfo MakeTurnInfo_Move(CharacterBase wantCharacter, in Vector3Int wantDestination)
    => MakeTurnInfo_Move(BattleManager.GetTurnPassed() + 1, wantCharacter.Controller, wantCharacter, wantCharacter.CurrentTilePosition, wantDestination);

    public static TurnBaseInfo MakeTurnInfo_Attack(int wantTurnCount, ControllerBase wantPlayer, CharacterBase wantCharacter, in Vector3Int wantStart, in Vector3Int wantDestination) => new TurnBaseInfo()
    {
        turnContext = $"{wantCharacter.DisplayInitial}x{TileManager.GetTileText(wantDestination)}",
        turnIndex = wantTurnCount,
        player = wantPlayer,
        playerID = BattleManager.GetPlayerID(wantPlayer),
        character = wantCharacter,
        characterID = wantCharacter ? wantCharacter.GetID() : -1,
        start = wantStart,
        destination = wantDestination,
        actionList = wantCharacter.StartCharacterAttack(wantPlayer, wantStart, wantDestination).BuildActionArray()
    };
    public static TurnBaseInfo MakeTurnInfo_Attack(ControllerBase wantPlayer, CharacterBase wantCharacter, in Vector3Int wantStart, in Vector3Int wantDestination)
    => MakeTurnInfo_Attack(BattleManager.GetTurnPassed() + 1, wantPlayer, wantCharacter, wantStart, wantDestination);

    public static TurnBaseInfo MakeTurnInfo_Attack(ControllerBase wantPlayer, CharacterBase wantCharacter, in Vector3Int wantDestination)
    => MakeTurnInfo_Attack(BattleManager.GetTurnPassed() + 1, wantPlayer, wantCharacter, wantCharacter.CurrentTilePosition, wantDestination);

    public static TurnBaseInfo MakeTurnInfo_Attack(CharacterBase wantCharacter, in Vector3Int wantDestination)
    => MakeTurnInfo_Attack(BattleManager.GetTurnPassed() + 1, wantCharacter.Controller, wantCharacter, wantCharacter.CurrentTilePosition, wantDestination);


	public static TurnBaseInfo MakeTurnInfo_SimpleDamage(int wantTurnCount, params CharacterBase[] wantCharacter) => new TurnBaseInfo()
	{
		turnContext = $"damage",
		turnIndex = wantTurnCount,
		player = null,
		playerID = -1,
		character = null,
		characterID = -1,
		start = -Vector3Int.one,
		destination = -Vector3Int.one,
		actionList = SimpleDamage(wantCharacter).BuildActionArray()
	};

	public static IEnumerable<TurnActionInfo> SimpleDamage(params CharacterBase[] wantCharacter)
	{
		for(int i = 0; i < wantCharacter.Length; ++i)
		{
			CharacterBase currentCharacter = wantCharacter[i];
			foreach(TurnActionInfo currentAction in currentCharacter.MakeDamageAction(currentCharacter.CurrentTilePosition, currentCharacter.CurrentTilePosition, currentCharacter.gameObject, 1))
			{
				yield return currentAction;
			}
		}
	}
}
