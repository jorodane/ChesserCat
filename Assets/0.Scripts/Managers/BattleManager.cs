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
    List<List<Vector3IntDirection>> guides = new() { new() };

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
        int originTurn = currentTurnIndex;
        currentTurnIndex = Mathf.Max(currentTurnIndex - 1, -1);
        TurnIndexChanged(originTurn);
    }
    public void ShowNextTurn(bool value)
    {
        if (currentTurnIndex >= turns.Count - 1) return;
        if(currentTurnIndex >= 0)turns[currentTurnIndex].TurnHighlightClear();
        int originTurn = currentTurnIndex;
        currentTurnIndex = Mathf.Min(currentTurnIndex + 1, turns.Count - 1);
        if (currentTurnIndex < turns.Count)
        {
            turns[currentTurnIndex].GoNext();
            TurnIndexChanged(originTurn);
        }
    }
    public IEnumerator PlayNextTurn()
    {
        if (currentTurnIndex >= turns.Count - 1) yield break;
        CompletePlayTurn();
        if(currentTurnIndex >= 0) turns[currentTurnIndex].TurnHighlightClear();
        int originTurn = currentTurnIndex;
        currentTurnIndex = Mathf.Min(currentTurnIndex + 1, turns.Count - 1);
        if (currentTurnIndex < turns.Count)
        {
            currentPlay = turns[currentTurnIndex].Play();
            TurnIndexChanged(originTurn);
            yield return currentPlay;
        }
        currentPlay = null;
    }

    public void TurnIndexChanged(int originTurn)
    {
        if (currentTurnIndex >= 0 && currentTurnIndex < turns.Count)
        {
            turns[currentTurnIndex].TurnHighlight();
        }

        int guideTurn = originTurn + 1;
        if (guideTurn >= 0 && guideTurn < guides.Count)
        {
            guides[originTurn + 1] = TileManager.ClaimGetGuideLineDirections();
            TileManager.ClaimSetGuideLineDirections(guides[currentTurnIndex + 1]);
        }
        else
        {
            TileManager.ClaimResetGuideLine();
        }
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
        while(currentTurnIndex < turns.Count - 1)
        {
            ShowNextTurn(value);
        }
    }

    public void AddTurn(in TurnBaseInfo newTurnInfo)
    {
        turns.Add(newTurnInfo);
        guides.Add(null);
        OnTurnAdded?.Invoke(turnPassed, newTurnInfo);
        //turnPassed = turns.Count;
        StartCoroutine(PlayNextTurn());
    }

    public void OnMove(ControllerBase controllerBase, CharacterBase selectedCharacter, in Vector3Int destination)
    {
        ShowFinalTurn(true);
        AddTurn(MakeTurnInfo_Move(turnPassed + 1, controllerBase, selectedCharacter, selectedCharacter.CurrentTilePosition, destination));
        if(selectedCharacter.TryGetModule(out ChessMovementModule asChessMove))
        {
            asChessMove.NoticeMoved();
        }
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
