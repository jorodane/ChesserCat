using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public delegate void TurnAddEvent(int newIndex, in TurnBaseInfo newTurnInfo);

public class BattleManager : ManagerBase
{
    public static BattleManager instance => GameManager.Battle;

    public static TurnAddEvent OnTurnAdded;

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

    public static TurnBaseInfo MakeTurnInfo_Move(int wantTurnCount, ControllerBase wantPlayer, CharacterBase wantCharacter, in Vector3Int wantStart, in Vector3Int wantDestination) => new TurnBaseInfo()
    { 
        turnContext = $"{wantCharacter.DisplayInitial}{TileManager.GetTileText(wantDestination)}",
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
        else
        {
            turns[currentTurnIndex].TurnHighlightClear();
            turns[currentTurnIndex].GoPrev();
        }
        currentTurnIndex = Mathf.Max(currentTurnIndex - 1, -1);
        TurnIndexChanged();
    }
    public void ShowNextTurn(bool value)
    {
        if (currentTurnIndex >= turns.Count - 1) return;
        if(currentTurnIndex >= 0)turns[currentTurnIndex].TurnHighlightClear();
        currentTurnIndex = Mathf.Min(currentTurnIndex + 1, turns.Count - 1);
        if (currentTurnIndex < turns.Count)
        {
            turns[currentTurnIndex].GoNext();
            TurnIndexChanged();
        }
    }
    public IEnumerator PlayNextTurn()
    {
        if (currentTurnIndex >= turns.Count - 1) yield break;
        CompletePlayTurn();
        if(currentTurnIndex >= 0) turns[currentTurnIndex].TurnHighlightClear();
        currentTurnIndex = Mathf.Min(currentTurnIndex + 1, turns.Count - 1);
        if (currentTurnIndex < turns.Count)
        {
            currentPlay = turns[currentTurnIndex].Play();
            TurnIndexChanged();
            yield return currentPlay;
        }
        currentPlay = null;
    }

    public void TurnIndexChanged()
    {
        if (currentTurnIndex >= 0) turns[currentTurnIndex].TurnHighlight();
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

    public void AddTurn(in TurnBaseInfo newTurnInfo)
    {
        turns.Add(newTurnInfo);
        OnTurnAdded?.Invoke(turnPassed, newTurnInfo);
        //turnPassed = turns.Count;
        StartCoroutine(PlayNextTurn());
    }

    public void OnMove(ControllerBase controllerBase, CharacterBase selectedCharacter, in Vector3Int destination)
    {
        ShowFinalTurn(true);
        AddTurn(MakeTurnInfo_Move(turnPassed + 1, controllerBase, selectedCharacter, selectedCharacter.CurrentTilePosition, destination));
    }

    public static void ClaimMove(ControllerBase controllerBase, CharacterBase selectedCharacter, in Vector3Int destination)
    {
        instance?.OnMove(controllerBase, selectedCharacter, destination);
    }

    public void OnAttack(ControllerBase controllerBase, CharacterBase selectedCharacter, in Vector3Int destination)
    {
        ShowFinalTurn(true);
        //AddTurn(MakeTurnInfo_Attack(turns.Count, controllerBase, selectedCharacter, selectedCharacter.CurrentTilePosition, destination));
    }

    public static void ClaimAttack(ControllerBase controllerBase, CharacterBase selectedCharacter, in Vector3Int destination)
    {
        instance?.OnAttack(controllerBase, selectedCharacter, destination);
    }
}
