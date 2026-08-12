using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public delegate void TurnAddEvent(int newIndex, in TurnBaseInfo newTurnInfo);
public delegate void TurnSimulateEvent(in TurnBaseInfo simulatedTurnInfo);
public delegate void TurnIndexChangeEvent(int newIndex);
public delegate void ModeChangeEvent(bool value);

public enum TurnResult { Failed, TurnOnFinalTurn, TurnOnAnalysisMode }

public class BattleManager : ManagerBase, ISavable
{
    public static BattleManager instance => GameManager.Battle;

    public static TurnAddEvent OnTurnAdded;
    public static TurnSimulateEvent OnTurnSimulated;
    public static TurnSimulateEvent OnTurnPlayed;
    public static TurnIndexChangeEvent OnTurnIndexChanged;
    public static ModeChangeEvent OnAnalysisModeChange;
    public static ModeChangeEvent OnAnimationModeChange;


    static List<ControllerBase> players = new();
    List<CharacterBase> neutralCharacters = new();
    TurnBaseInfo simulatedTurn = null;
    int currentTurnIndex = -1;
    int currentBranchIndex = -1;
    int TurnPassed => currentTurnIndex / Mathf.Max(players.Count, 1);
    public ControllerBase CurrentTurnPlayer
    {
        get
        {
            if (players is null || players.Count == 0) return null;
            return players[currentTurnIndex % players.Count];
        }
    }

    readonly List<TurnBaseInfo> turns = new();
    readonly List<TurnBaseInfo> branches = new();
    readonly List<List<Vector3IntDirection>> guides = new() { new() };
    readonly List<List<Vector3IntDirection>> branchGuides = new() { new() };

    IEnumerator _currentPlay = null;
    IEnumerator CurrentPlay
    {
        get => _currentPlay;
        set
        {
            _currentPlay = value;
            OnAnimationModeChange(IsAnimationMode);
        }
    }

    public bool IsFirstTurn => currentTurnIndex < 0;
    public bool IsFirstBranch => currentBranchIndex < 0;
    public bool IsFinalTurn => currentTurnIndex >= turns.Count - 1;
    public bool IsFinalBranch => currentBranchIndex >= branches.Count - 1;
    public bool IsAnalysisMode => currentBranchIndex >= 0;
    public bool IsSimulationMode => simulatedTurn is not null;
    public bool IsAnimationMode => CurrentPlay is not null;


    protected override IEnumerator OnConnected(GameManager newManager)
	{
        players ??= new List<ControllerBase>();
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

    public static int GetTurnPassed() => instance ? instance.TurnPassed : 0;

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

    public string GetPGN()
    {
        return string.Empty;
    }

    public string GetFEN()
    {
        return string.Empty;
    }

    public void ShowPrevTurn(bool value)
    {
        CompletePlayTurn();
        if (IsAnalysisMode)
        {
            ShowPrevBranch();
            return;
        }
        if (currentTurnIndex < 0) return;
        else
        {
            turns[currentTurnIndex].TurnHighlightClear();
            turns[currentTurnIndex].GoPrev(true);
        }
        int originTurn = currentTurnIndex;
        currentTurnIndex = Mathf.Max(currentTurnIndex - 1, -1);
        TurnIndexChanged(originTurn);
    }
    void ShowPrevBranch()
    {
        if (currentBranchIndex < 0) return;
        else
        {
            branches[currentBranchIndex].TurnHighlightClear();
            branches[currentBranchIndex].GoPrev(true);
        }
        int originTurn = currentBranchIndex;
        currentBranchIndex = Mathf.Max(currentBranchIndex - 1, -1);
        if (currentBranchIndex < 0) AnalysisModeEnd();
        else BranchIndexChanged(originTurn);
    }

    public void ShowNextTurn(bool value)
    {
        CompletePlayTurn();
        if (IsAnalysisMode)
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
            turns[currentTurnIndex].GoNext(true);
            TurnIndexChanged(originTurn);
        }
    }

    void ShowNextBranch()
    {
        if (currentBranchIndex >= branches.Count - 1) return;
        if (currentBranchIndex >= 0) branches[currentBranchIndex].TurnHighlightClear();
        int originTurn = currentBranchIndex;
        currentBranchIndex = Mathf.Min(currentBranchIndex + 1, branches.Count - 1);
        if (currentBranchIndex < branches.Count)
        {
            branches[currentBranchIndex].GoNext(true);
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
        int originTurn = currentTurnIndex;
        currentTurnIndex = Mathf.Min(currentTurnIndex + 1, turns.Count - 1);
        if (currentTurnIndex < turns.Count)
        {
            TurnBaseInfo currentTurn = turns[currentTurnIndex];
            if(currentTurn is not null)
            {
                CurrentPlay = turns[currentTurnIndex].Play();
                OnTurnPlayed?.Invoke(currentTurn);
            }
            TurnIndexChanged(originTurn);
            yield return CurrentPlay;
            OnTurnPlayed?.Invoke(null);
        }
        CurrentPlay = null;
    }

    public IEnumerator PlayNextBranch()
    {
        if (currentBranchIndex >= branches.Count - 1) yield break;
        CompletePlayTurn();
        int originTurn = currentBranchIndex;
        currentBranchIndex = Mathf.Min(currentBranchIndex + 1, branches.Count - 1);
        if (currentBranchIndex >= 0)
        {
            TurnBaseInfo currentTurn = turns[currentBranchIndex];
            if(currentTurn is not null)
            {
                CurrentPlay = branches[currentBranchIndex].Play();
                OnTurnPlayed?.Invoke(currentTurn);
            }
            BranchIndexChanged(originTurn);
            yield return CurrentPlay;
            OnTurnPlayed?.Invoke(null);
        }
        CurrentPlay = null;
    }

    public void TurnIndexChanged(int originTurn)
    {
        if (originTurn >= 0 && originTurn < turns.Count) turns[originTurn].TurnHighlightClear();
        if (currentTurnIndex >= 0 && currentTurnIndex < turns.Count)
        {
            turns[currentTurnIndex].TurnHighlight();
        }

        int guideTurn = originTurn + 1;
        if (guideTurn >= 0 && guideTurn < guides.Count)
        {
            guides[guideTurn] = TileManager.ClaimGetGuideLineDirections();
            TileManager.ClaimSetGuideLineDirections(guides[currentTurnIndex + 1]);
        }
        else
        {
            TileManager.ClaimResetGuideLine();
        }

        OnTurnIndexChanged?.Invoke(currentTurnIndex);
        InputManager.ResetCharacterInput();
    }

    public void TurnIndexRefresh()
    {
        if (currentTurnIndex >= 0 && currentTurnIndex < turns.Count)
        {
            turns[currentTurnIndex].TurnHighlight();
        }
        int guideTurn = currentTurnIndex + 1;
        if (guideTurn >= 0 && guideTurn < guides.Count)
        {
            TileManager.ClaimSetGuideLineDirections(guides[guideTurn]);
        }
        else
        {
            TileManager.ClaimResetGuideLine();
        }
        InputManager.ResetCharacterInput();
    }

    public void BranchIndexChanged(int originTurn)
    {
        if (originTurn >= 0)
        {
            if (originTurn < branches.Count) branches[originTurn].TurnHighlightClear();
        }
        else
        {
            guides[currentTurnIndex + 1] = TileManager.ClaimGetGuideLineDirections();
            if (currentTurnIndex >= 0) turns[currentTurnIndex].TurnHighlightClear();
        }

        if (currentBranchIndex >= 0 && currentBranchIndex < branches.Count)
        {
            branches[currentBranchIndex].TurnHighlight();
        }

        int guideTurn = originTurn + 1;
        if (guideTurn >= 0 && guideTurn < branchGuides.Count)
        {
            branchGuides[guideTurn] = TileManager.ClaimGetGuideLineDirections();
            TileManager.ClaimSetGuideLineDirections(branchGuides[currentBranchIndex + 1]);
            InputManager.ResetCharacterInput();
        }
        else
        {
            TurnIndexRefresh();
        }
    }

    public void CompletePlayTurn()
    {
        if (CurrentPlay != null)
        {
            StopCoroutine(CurrentPlay);
            CurrentPlay = null;
            TurnBaseInfo targetTurn = null;
            if (IsAnalysisMode) targetTurn = branches[currentBranchIndex];
            else if (!IsFirstTurn) targetTurn = turns[currentTurnIndex];
            if (targetTurn is null) return;
            targetTurn.GoNext(true);
            OnTurnPlayed?.Invoke(null);
        }
    }

    public static void ClaimCompletePlayTurn() => instance?.CompletePlayTurn();

    TurnResult AddTurn(in TurnBaseInfo newTurnInfo)
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

    void AddFinalTurn(in TurnBaseInfo newTurnInfo)
    {
        turns.Add(newTurnInfo);
        guides.Add(null);
        OnTurnAdded?.Invoke(TurnPassed, newTurnInfo);
        ClaimTurnSimulationReset();
        StartCoroutine(PlayNextTurn());
    }

    void AddBranchTurn(in TurnBaseInfo newTurnInfo)
    {
        if(!IsFinalBranch) RemoveBranchUntilCurrentIndex();
        branches.Add(newTurnInfo);
        branchGuides.Add(null);
        StartCoroutine(PlayNextBranch());
        ClaimTurnSimulationReset();
        OnAnalysisModeChange?.Invoke(true);
    }

    public bool RemoveBranchTurn()
    {
        int branchCount = branches.Count;
        if (branchCount == 0) return false;
        int finalBranchIndex = branchCount - 1;
        if (finalBranchIndex == currentBranchIndex)
        {
            branches[currentBranchIndex].GoPrev(true);
            branches[currentBranchIndex].TurnHighlightClear();
           --currentBranchIndex;
            BranchIndexChanged(finalBranchIndex);
        }
        if (finalBranchIndex < branches.Count)
        {
            branches.RemoveAt(finalBranchIndex);
            if (finalBranchIndex >= 0) branchGuides.RemoveAt(finalBranchIndex + 1);
            else branchGuides[0].Clear();
        }
        
        if(currentBranchIndex < 0)
        {
            OnAnalysisModeChange?.Invoke(false);
            TurnIndexRefresh();
        }
        return true;
    }

    public void RemoveBranchUntilCurrentIndex()
    {
        int targetCount = currentBranchIndex + 1;
        while (branches.Count > targetCount && RemoveBranchTurn()) ;
    }

    public void AnalysisModeEnd() 
    { 
        while(RemoveBranchTurn());
    }
    public static bool ClaimAnalysisModeEnd()
    {
        if (!instance) return false;
        if(instance.IsAnalysisMode)
        {
            instance.AnalysisModeEnd();
            return true;
        }
        return false;
    }
    public void TurnSimulation(in TurnBaseInfo simulate)
    {
        if (simulate == simulatedTurn) return;
        simulatedTurn = simulate;
        OnTurnSimulated?.Invoke(simulatedTurn); 
    }
    public static void ClaimTurnSimulation(in TurnBaseInfo simulate) => instance?.TurnSimulation(simulate);
    public static void ClaimTurnSimulationReset() => instance?.TurnSimulation(null);

    public TurnResult TurnSimulationConfirm()
    {
        if (!IsSimulationMode) return TurnResult.Failed;
        return AddTurn(simulatedTurn);
    }
    public static TurnResult ClaimTurnSimulationConfirm() => instance?.TurnSimulationConfirm() ?? TurnResult.Failed;

    public BattleSaveData MakeSaveData()
    {
        ShowFinalTurn(false);
        return new ()
        {
            saveDataList = this.MakeCustomSaveData(),
            controllerList = players.MakeControllerSaveDataArray(),
            neutralCharacterList = neutralCharacters.MakeCharacterSaveDataArray(),
            turnList = turns.MakeTurnSaveDataArray(),
            guideList = guides.MakeGuideSaveDataArray(),
            tileList = TileManager.MakeTileSaveData(),
        };
    }

    public virtual void ConstructCustomSaveData(ref Dictionary<string,string> result){ }
}
