using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public enum TurnCommandType
{
    Move, Attack
}

public enum TurnActionType
{
    Walk, Jump, Attack
}

[Serializable]
public abstract class TurnActionInfo
{
    public abstract void GoNext();
    public abstract void GoPrev();
    public abstract IEnumerator Play();

}

[Serializable]
public class TurnActionInfo_Move : TurnActionInfo
{
    public Vector3Int startLocation;
    public Vector3Int actionLocation;
    public CharacterBase effectedCharacter;
    public int effectedCharacterID;

    public override string ToString() => $"{effectedCharacter?.DisplayInitial}{TileManager.GetTileText(actionLocation)}";

    public TurnActionInfo_Move(Vector3Int currentLocation, Vector3Int wantLocation, int wantCharacterID, CharacterBase wantCharacter)
    {
        startLocation = currentLocation;
        actionLocation = wantLocation;
        effectedCharacter = wantCharacter;
        effectedCharacterID = wantCharacterID;
    }
    public TurnActionInfo_Move(Vector3Int wantLocation, int wantCharacterID, CharacterBase wantCharacter)
    {
        effectedCharacter = wantCharacter;
        effectedCharacterID = wantCharacterID;
        actionLocation = wantLocation;
        if(effectedCharacter) startLocation = effectedCharacter.CurrentTilePosition;
        else actionLocation = -Vector3Int.one;
    }

    public override void GoNext()
    {
        if (!effectedCharacter) return;
        TileManager.PlaceObjectOnTile(effectedCharacter.gameObject, actionLocation, startLocation);
    }

    public override void GoPrev()
    {
        if(!effectedCharacter) return;
        TileManager.PlaceObjectOnTile(effectedCharacter.gameObject, startLocation);
    }

    public override IEnumerator Play()
    {
        if(effectedCharacter)
        {
            if(effectedCharacter.TryGetModule(out ChessMovementModule movement))
            {
                yield return movement.PlayMove(startLocation, actionLocation);
            }
        }
    }
}


[Serializable]
public struct TurnBaseInfo
{
    public int turnCount;
    public int playerID;
    public ControllerBase player;
    public int characterID;
    public CharacterBase character;
    public Vector3Int start;
    public Vector3Int destination;
    public TurnActionInfo[] actionList;
    int playedIndex;

    public void GoNext()
    {
        for (; playedIndex < actionList.Length; playedIndex++)
        {
            actionList[playedIndex].GoNext();
        }
    }

    public IEnumerator Play()
    {
        for (; playedIndex < actionList.Length; playedIndex++)
        {
            yield return actionList[playedIndex].Play();
            actionList[playedIndex].GoNext();
        }
    }

    public void GoPrev()
    {
        for(; playedIndex >= 0; playedIndex--) actionList[playedIndex].GoPrev();
    }
}

public class BattleManager : ManagerBase
{
    public static BattleManager instance => GameManager.Battle;

    static List<ControllerBase> players;
    ControllerBase currentTurnPlayer;
    int currentTurnIndex = -1;
    int turnPassed = 0;
    
    List<TurnBaseInfo> turns = new();

    IEnumerator currentPlay = null;

    protected override IEnumerator OnConnected(GameManager newManager)
	{
        players = new List<ControllerBase>();
        InputManager.OnGoNextTurn   -= ShowNextTurn;
        InputManager.OnGoNextTurn   += ShowNextTurn;
        InputManager.OnGoPrevTurn   -= ShowPrevTurn;
        InputManager.OnGoPrevTurn   += ShowPrevTurn;
        InputManager.OnGoFirstTurn  -= ShowFirstTurn;
        InputManager.OnGoFirstTurn  += ShowFirstTurn;
        InputManager.OnGoFinalTurn  -= ShowFinalTurn;
        InputManager.OnGoFinalTurn  += ShowFinalTurn;
		yield return null;
	}

	protected override void OnDisconnected()
    {
        InputManager.OnGoNextTurn -= ShowNextTurn;
        InputManager.OnGoPrevTurn -= ShowPrevTurn;
        InputManager.OnGoFirstTurn -= ShowFirstTurn;
        InputManager.OnGoFinalTurn -= ShowFinalTurn;
    }

    public static int GetPlayerID(ControllerBase wantPlayer) => players.FindIndex((target) => target == wantPlayer);

    public static void AddPlayerOnBattle(ControllerBase newPlayer)
    {
        if (newPlayer && !players.Contains(newPlayer))
        {
            players.Add(newPlayer);
        }
    }

    public static void RemovePlayerOnBattle(ControllerBase wantPlayer)
    {
        players.Remove(wantPlayer);
    }

    public static TurnBaseInfo MakeTurnInfo_Move(int wantTurnCount, ControllerBase wantPlayer, CharacterBase wantCharacter, Vector3Int wantStart, Vector3Int wantDestination) => new TurnBaseInfo()
    { 
        turnCount = wantTurnCount,
        player = wantPlayer, 
        playerID = GetPlayerID(wantPlayer), 
        character = wantCharacter, 
        characterID = wantPlayer?.GetCharacterToID(wantCharacter) ?? -1,
        start = wantStart,
        destination = wantDestination,
        actionList = TileManager.StartCharacterMove(wantPlayer, wantCharacter, wantStart, wantDestination).ToArray()
    };
    public void ShowFirstTurn(bool value)
    {
        while (-1 < currentTurnIndex)
        {
            ShowPrevTurn(value);
        }
    }

    public void ShowPrevTurn(bool value)
    {
        if (currentTurnIndex < 0) return;
        else turns[currentTurnIndex].GoPrev();
        currentTurnIndex = Mathf.Max(currentTurnIndex - 1, -1);
    }
    public void ShowNextTurn(bool value)
    {
        if (currentTurnIndex >= turns.Count - 1) return;
        currentTurnIndex = Mathf.Min(currentTurnIndex + 1, turns.Count - 1);
        if (currentTurnIndex < turns.Count) turns[currentTurnIndex].GoNext();
    }
    public IEnumerator PlayNextTurn()
    {
        if (currentTurnIndex >= turns.Count - 1) yield break;
        CompletePlayTurn();
        currentTurnIndex = Mathf.Min(currentTurnIndex + 1, turns.Count - 1);
        if (currentTurnIndex < turns.Count)
        {
            currentPlay = turns[currentTurnIndex].Play();
            yield return currentPlay;
        }
        currentPlay = null;
    }

    public void CompletePlayTurn()
    {
        if (currentPlay != null)
        {
            StopCoroutine(currentPlay);
            turns[currentTurnIndex].GoNext();
        }
    }

    public void ShowFinalTurn(bool value)
    {
        while(currentTurnIndex < turnPassed - 1)
        {
            ShowNextTurn(value);
        }
    }

    public void AddTurn(TurnBaseInfo newTurnInfo)
    {
        turns.Add(newTurnInfo);
        turnPassed = turns.Count;
        StartCoroutine(PlayNextTurn());
    }

    public void OnMove(ControllerBase controllerBase, CharacterBase selectedCharacter, Vector3Int destination)
    {
        ShowFinalTurn(true);
        AddTurn(MakeTurnInfo_Move(turns.Count, controllerBase, selectedCharacter, selectedCharacter.CurrentTilePosition, destination));
    }

    public static void ClaimMove(ControllerBase controllerBase, CharacterBase selectedCharacter, Vector3Int destination)
    {
        instance?.OnMove(controllerBase, selectedCharacter, destination);
    }

    public void OnAttack(ControllerBase controllerBase, CharacterBase selectedCharacter, Vector3Int destination)
    {
        ShowFinalTurn(true);
        //AddTurn(MakeTurnInfo_Attack(turns.Count, controllerBase, selectedCharacter, selectedCharacter.CurrentTilePosition, destination));
    }

    public static void ClaimAttack(ControllerBase controllerBase, CharacterBase selectedCharacter, Vector3Int destination)
    {
        instance?.OnAttack(controllerBase, selectedCharacter, destination);
    }
}
