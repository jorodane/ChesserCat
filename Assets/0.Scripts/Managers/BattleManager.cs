using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum TurnCommandType
{
    Move, Attack
}

public enum TurnActionType
{
    Walk, Jump, Attack
}

[Serializable]
public struct TurnActionInfo
{
    public TurnActionType actionType;
    public Vector3Int startLocation;
    public Vector3Int actionLocation;
    public int actionValue;
    public CharacterBase effectedCharacter;
    public int effectedCharacterID;

    public override readonly string ToString() => $"{actionType}({actionValue}) : [{startLocation} -> {actionLocation}] {effectedCharacter} got affect";

    public TurnActionInfo(TurnActionType wantType, int wantValue, Vector3Int currentLocation, Vector3Int wantLocation, int wantCharacterID, CharacterBase wantCharacter)
    {
        actionType = wantType;
        startLocation = currentLocation;
        actionLocation = wantLocation;
        actionValue = wantValue;
        effectedCharacter = wantCharacter;
        effectedCharacterID = wantCharacterID;
    }

    public TurnActionInfo(TurnActionType wantType, int wantValue, Vector3Int currentLocation, Vector3Int wantLocation)
    {
        actionType = wantType;
        startLocation = currentLocation;
        actionLocation = wantLocation;
        actionValue = wantValue;
        effectedCharacter = null;
        effectedCharacterID = -1;
    }

    public TurnActionInfo(TurnActionType wantType, int wantValue, Vector3Int currentLocation, int wantCharacterID, CharacterBase wantCharacter)
    {
        actionType = wantType;
        actionValue = wantValue;
        effectedCharacter = wantCharacter;
        effectedCharacterID = wantCharacterID;
        startLocation = currentLocation;
        if(effectedCharacter) actionLocation = effectedCharacter.CurrentTilePosition;
        else actionLocation = -Vector3Int.one;
    }
}


[Serializable]
public struct TurnBaseInfo
{
    public int turnCount;
    public int playerID;
    public           ControllerBase player;
    public int characterID;
    public           CharacterBase character;
    public Vector3Int start;
    public Vector3Int destination;
    public TurnActionInfo[] actionList;
}

public class BattleManager : ManagerBase
{
    static List<ControllerBase> players;
    ControllerBase currentTurnPlayer;
    int currentTurnIndex = -1;
    int turnPassed = 0;
    
    List<TurnBaseInfo> turns = new();

	protected override IEnumerator OnConnected(GameManager newManager)
	{
        players = new List<ControllerBase>();
		yield return null;
	}

	protected override void OnDisconnected()
	{

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
}
