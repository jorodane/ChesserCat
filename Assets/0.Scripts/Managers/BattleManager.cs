using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public delegate void TurnAddEvent(int newIndex, in TurnBaseInfo newTurnInfo);
public delegate void AnalysisModeChangeEvent(bool value);

public enum TurnResult { TurnOnFinalTurn, TurnOnAnalysisMode }

public class BattleManager : ManagerBase
{
    public static BattleManager instance => GameManager.Battle;

    public static TurnAddEvent OnTurnAdded;
    public static AnalysisModeChangeEvent OnAnalysisModeChange;

    static List<ControllerBase> players;
    ControllerBase currentTurnPlayer;
    int currentTurnIndex = -1;
    int currentBranchIndex = -1;
    int turnPassed = 0;
    
    List<TurnBaseInfo> turns = new();
    List<TurnBaseInfo> branches = new();
    List<List<Vector3IntDirection>> guides = new() { new() };
    List<List<Vector3IntDirection>> branchGuides = new() { new() };

    IEnumerator currentPlay = null;

    public bool IsFirstTurn => currentTurnIndex < 0;
    public bool IsFirstBranch => currentBranchIndex < 0;
    public bool IsFinalTurn => currentTurnIndex >= turns.Count - 1;
    public bool IsFinalBranch => currentBranchIndex >= branches.Count - 1;
    public bool IsAnalysisMode => currentBranchIndex >= 0;


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


    public void ShowPrevTurn(bool value)
    {
        if (IsAnalysisMode)
        {
            ShowPrevBranch();
            return;
        }
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
    public void ShowPrevBranch()
    {
        if (currentBranchIndex < 0) return;
        else
        {
            branches[currentBranchIndex].TurnHighlightClear();
            branches[currentBranchIndex].GoPrev();
        }
        int originTurn = currentBranchIndex;
        currentBranchIndex = Mathf.Max(currentBranchIndex - 1, -1);
        if (currentBranchIndex < 0) AnalysisModeEnd();
        else BranchIndexChanged(originTurn);
    }

    public void ShowNextTurn(bool value)
    {
        if(IsAnalysisMode)
        {
            ShowNextBranch();
            return;
        }
        if (currentTurnIndex >= turns.Count - 1) return;
        if (currentTurnIndex >= 0)turns[currentTurnIndex].TurnHighlightClear();
        int originTurn = currentTurnIndex;
        currentTurnIndex = Mathf.Min(currentTurnIndex + 1, turns.Count - 1);
        if (currentTurnIndex < turns.Count)
        {
            turns[currentTurnIndex].GoNext();
            TurnIndexChanged(originTurn);
        }
    }

    public void ShowNextBranch()
    {
        if (currentBranchIndex >= branches.Count - 1) return;
        if (currentBranchIndex >= 0) branches[currentBranchIndex].TurnHighlightClear();
        int originTurn = currentBranchIndex;
        currentBranchIndex = Mathf.Min(currentBranchIndex + 1, branches.Count - 1);
        if (currentBranchIndex < branches.Count)
        {
            branches[currentBranchIndex].GoNext();
            BranchIndexChanged(originTurn);
        }
    }

    public void ShowFirstTurn(bool value)
    {
        if(IsAnalysisMode) AnalysisModeEnd();
        while (!IsFirstTurn) ShowPrevTurn(value);
    }

    public void ShowFinalTurn(bool value)
    {
        if (IsAnalysisMode) AnalysisModeEnd();
        while (!IsFinalTurn) ShowNextTurn(value);
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

    public IEnumerator PlayNextBranch()
    {
        if (currentBranchIndex >= branches.Count - 1) yield break;
        CompletePlayTurn();
        if (currentBranchIndex >= 0) branches[currentBranchIndex].TurnHighlightClear();
        else if(currentTurnIndex >= 0) turns[currentTurnIndex].TurnHighlightClear();
        int originTurn = currentBranchIndex;
        currentBranchIndex = Mathf.Min(currentBranchIndex + 1, branches.Count - 1);
        if (currentBranchIndex < branches.Count)
        {
            currentPlay = branches[currentBranchIndex].Play();
            BranchIndexChanged(originTurn);
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

    public void BranchIndexChanged(int originTurn)
    {
        if (currentBranchIndex >= 0 && currentBranchIndex < branches.Count)
        {
            branches[currentBranchIndex].TurnHighlight();
        }

        int guideTurn = originTurn + 1;
        if (guideTurn >= 0 && guideTurn < branchGuides.Count)
        {
            if(originTurn < 0)  guides[currentTurnIndex + 1]     = TileManager.ClaimGetGuideLineDirections();
            else                branchGuides[guideTurn]      = TileManager.ClaimGetGuideLineDirections();
            TileManager.ClaimSetGuideLineDirections(branchGuides[currentBranchIndex + 1]);
        }
        else
        {
            TurnIndexChanged(currentTurnIndex);
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



    public TurnResult AddTurn(in TurnBaseInfo newTurnInfo)
    {
        if (IsFinalTurn)
        {
            AddFinalTurn(newTurnInfo);
            return TurnResult.TurnOnFinalTurn;
        }
        else
        {
            AddBranchTurn(newTurnInfo);
            return TurnResult.TurnOnAnalysisMode;
        }
    }

    public void AddFinalTurn(in TurnBaseInfo newTurnInfo)
    {
        turns.Add(newTurnInfo);
        guides.Add(null);
        OnTurnAdded?.Invoke(turnPassed, newTurnInfo);
        //turnPassed = turns.Count;
        StartCoroutine(PlayNextTurn());
    }

    public void AddBranchTurn(in TurnBaseInfo newTurnInfo)
    {
        branches.Add(newTurnInfo);
        branchGuides.Add(null);
        StartCoroutine(PlayNextBranch());
        OnAnalysisModeChange?.Invoke(true);
    }

    public bool RemoveBranchTurn()
    {
        int branchCount = branches.Count;
        if (branchCount == 0) return false;
        int finalBranchIndex = branchCount - 1;
        if (finalBranchIndex <= currentBranchIndex) branches[finalBranchIndex].GoPrev();
        branches.RemoveAt(finalBranchIndex);
        if (finalBranchIndex >= 0) branchGuides.RemoveAt(finalBranchIndex + 1);
        else branchGuides[0].Clear();
        finalBranchIndex -= 1;
        currentBranchIndex = Mathf.Min(currentBranchIndex, finalBranchIndex);
        if(currentBranchIndex < 0)
        {
            currentBranchIndex = -1;
            OnAnalysisModeChange?.Invoke(false);
        }
        return true;
    }

    public void RemoveBranchUntilCurrentIndex()
    {
        while (branches.Count > 0 && RemoveBranchTurn()) ;
    }

    public void AnalysisModeEnd() 
    { 
        while(RemoveBranchTurn());
        TileManager.ClaimResetGuideLine();
        TurnIndexChanged(currentTurnIndex);
    }
    public static void ClaimAnalasysModeEnd() => instance?.AnalysisModeEnd();

    public void OnMove(ControllerBase controllerBase, CharacterBase selectedCharacter, in Vector3Int destination)
    {
        TurnBaseInfo newInfo = MakeTurnInfo_Move(turnPassed + 1, controllerBase, selectedCharacter, selectedCharacter.CurrentTilePosition, destination);
        AddTurn(newInfo);
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
