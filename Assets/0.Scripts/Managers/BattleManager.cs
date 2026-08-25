using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.TextCore.Text;

public delegate void TurnAddEvent(int newIndex, in TurnBaseInfo newTurnInfo);
public delegate void TurnResetEvent();
public delegate void TurnSimulateEvent(in TurnBaseInfo simulatedTurnInfo);
public delegate void TurnIndexChangeEvent(int newIndex);
public delegate void ModeChangeEvent(bool value);
public delegate void LocalPlayerControllerChangeEvent(PlayerController newController);

public enum TurnResult { Failed, TurnOnFinalTurn, TurnOnAnalysisMode }

public class BattleManager : ManagerBase, ISavable<BattleSaveData>
{
    public static BattleManager instance => GameManager.Battle;

    public static TurnAddEvent OnTurnAdded;
    public static TurnResetEvent OnTurnReset;
    public static TurnSimulateEvent OnTurnSimulated;
    public static TurnSimulateEvent OnTurnPlayed;
    public static TurnIndexChangeEvent OnTurnIndexChanged;
    public static ModeChangeEvent OnAnalysisModeChange;
    public static ModeChangeEvent OnAnimationModeChange;
	public static LocalPlayerControllerChangeEvent OnLocalPlayerControllerChanged;

	PlayerController localPlayerController = null;
    static List<ControllerBase> players = new();
    static List<CharacterBase> characters = new();
    TurnBaseInfo simulatedTurn = null;
    StageSaveData? currentStage = null;
    int currentTurnIndex = -1;
    int currentBranchIndex = -1;
    int TurnPassed => currentTurnIndex / Mathf.Max(players.Count, 1);
    int TurnFinalIndex => turns.Count - 1;
    int BranchLastIndex => branches.Count - 1;
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
    public bool IsFinalTurn => currentTurnIndex >= TurnFinalIndex - 1;
    public bool IsFinalBranch => currentBranchIndex >= BranchLastIndex - 1;
    public bool IsAnalysisMode => currentBranchIndex >= 0;
    public bool IsSimulationMode => simulatedTurn is not null;
    public bool IsAnimationMode => CurrentPlay is not null;

    public BattleSaveData MakeSaveData()
    {
        int originTurnIndex = currentTurnIndex;
        ShowFirstTurn(false);
        BattleSaveData result = new()
        {
            saveDataList = this.MakeCustomSaveData(),
            playerSave = GetControllerFromID(0).MakeSaveData(),
            turnList = turns.MakeTurnSaveDataArray(),
            guideList = guides.MakeGuideSaveDataArray(),
			characterList = characters.MakeCharacterSaveDataArray(),
            stage = currentStage ?? new()
			{
                saveDataList = GameManager.Tile.MakeCustomSaveData(),
                fieldData = GameManager.Tile?.MakeSaveData() ?? new FieldSaveData(),
            }
        };
        ShowWantTurn(originTurnIndex);
        return result;
    }

    public void LoadData(in BattleSaveData data)
    {
		ResetAll();
		localPlayerController = CreatePlayerOnBattle<PlayerController>("PlayerController", data.playerSave);
		OnLocalPlayerControllerChanged?.Invoke(localPlayerController);
		//foreach(ControllerSaveData currentControllerData in data.stage.controllerList)
		//{
		//	CreatePlayerOnBattle<ControllerBase>(currentControllerData.prefabName, currentControllerData);
		//}
		characters = SpawnAllCharactersFromData(data.characterList).ToList();
        foreach (TurnBaseInfo currentTurn in data.turnList.MakeTurnFromData()) LoadFinalTurn(currentTurn);
		foreach (GuideSaveData currentGuide in data.guideList) guides[currentGuide.index] = currentGuide.guides.ToList();
        ShowFinalTurn(false);
    }

    public void ResetAll()
    {
        CompletePlayTurn();
		localPlayerController = null;
		OnLocalPlayerControllerChanged?.Invoke(null);
		RemoveAllCharacterOnBattle();
        RemoveAllPlayerOnBattle();
        ClaimTurnSimulationReset();
        ClearEveryTurn();
        OnAnalysisModeChange?.Invoke(false);
    }

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
	public static CharacterBase GetCharcterFromID(int id)
	{
		characters.TryGetValue(id, out CharacterBase result);
		return result;
	}

	public static ControllerBase GetControllerFromID(int id)
	{
		if (id < 0) id = 1;
		if (!players.TryGetValue(id, out ControllerBase result))
		{
			result = CreatePlayerOnBattle<ControllerBase>("ControllerBase", id);
		}
		return result;
	}


	public static CharacterBase AddCharacterOnBattle(CharacterBase newCharacter)
	{
		if (newCharacter && !characters.Contains(newCharacter))
		{
			int id = characters.Count;
			newCharacter.SetID(id);
			characters.Add(newCharacter);
		}
		return newCharacter;
	}

	public IEnumerable<CharacterBase> SpawnAllCharactersFromData(IEnumerable<CharacterSaveData> datas)
	{
		foreach (CharacterBase currentCharacter in datas.MakeCharacterFromData())
		{
			yield return AddCharacterOnBattle(currentCharacter);
		}
	}


	public static int GetPlayerID(ControllerBase wantPlayer) => players.FindIndex((target) => target == wantPlayer);

	public static T CreatePlayerOnBattle<T>(string prefabName, in ControllerSaveData saveData) where T : ControllerBase
	{

		GameObject instance = ObjectManager.CreateObject(prefabName);
		T result = instance.GetComponent<T>();
		if (result)
		{
			result.LoadData(saveData);
			AddPlayerOnBattle(result, saveData.id);
		}
		else
		{
			ObjectManager.DestroyObject(instance);
		}
		return result;
	}

	public static T CreatePlayerOnBattle<T>(string prefabName, int? id = null) where T : ControllerBase
	{

		GameObject instance = ObjectManager.CreateObject(prefabName);
		T result = instance.GetComponent<T>();
		if (result)
		{
			AddPlayerOnBattle(result, id);
		}
		else
		{
			ObjectManager.DestroyObject(instance);
		}
		return result;
	}

	public static void AddPlayerOnBattle(ControllerBase newPlayer, int? wantID)
    {
        if (newPlayer && !players.Contains(newPlayer))
        {
			int id = Mathf.Max(0, wantID ?? players.Count);
			newPlayer.SetID(id);
			players.Insert(id, newPlayer);
        }
    }

    public static void RemoveAllPlayerOnBattle()
    {
        if (players is null) return;
        foreach(ControllerBase currentPlayer in players.ToArray()) RemovePlayerOnBattle(currentPlayer);
		players.Clear();
    }

	public static void RemoveAllCharacterOnBattle()
	{
		if (characters is null) return;
		foreach (CharacterBase currentCharacter in characters.ToArray()) RemoveCharacterOnBattle(currentCharacter);
		characters.Clear();
	}

	public static void RemovePlayerOnBattle(ControllerBase wantPlayer)
    {
		ObjectManager.DestroyObject(wantPlayer.gameObject);
		players.Remove(wantPlayer);
	}

	public static void RemoveCharacterOnBattle(CharacterBase wantCharacter)
	{
		wantCharacter.SetID(-1);
		characters.Remove(wantCharacter);
		ObjectManager.DestroyObject(wantCharacter.gameObject);
	}

	public void ClearEveryTurn()
    {
        turns.Clear();
        branches.Clear();
        guides.Clear();
        guides.Add(new());
        branchGuides.Clear();
        branchGuides.Add(new());
        currentTurnIndex = -1;
        currentBranchIndex = -1;
        OnTurnReset?.Invoke();
    }

    public void ShowPrevTurn(bool activeByKey) => ShowPrevTurn();
    public bool ShowPrevTurn()
    {
        CompletePlayTurn();
        if (IsAnalysisMode)
        {
            ShowPrevBranch();
            return true;
        }
        if (currentTurnIndex < 0) return false;
        else
        {
            turns[currentTurnIndex].TurnHighlightClear();
            turns[currentTurnIndex].GoPrev(true);
        }
        int originTurn = currentTurnIndex;
        currentTurnIndex = Mathf.Max(currentTurnIndex - 1, -1);
        TurnIndexChanged(originTurn);
        return true;
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

    public void ShowNextTurn(bool activeByKey) => ShowNextTurn();
    public bool ShowNextTurn()
    {
        CompletePlayTurn();
        if (IsAnalysisMode)
        {
            ShowNextBranch();
            return true;
        }
        if (currentTurnIndex >= turns.Count - 1) return false;
        if (currentTurnIndex >= 0)turns[currentTurnIndex].TurnHighlightClear();
        int originTurn = currentTurnIndex;
        currentTurnIndex = Mathf.Min(currentTurnIndex + 1, turns.Count - 1);
        if (currentTurnIndex < turns.Count)
        {
            turns[currentTurnIndex].GoNext(true);
            TurnIndexChanged(originTurn);
        }
        return true;
    }

    public void ShowWantTurn(int index)
    {
        CompletePlayTurn();
        if (IsAnalysisMode) AnalysisModeEnd();
        if (currentTurnIndex >= 0)turns[currentTurnIndex].TurnHighlightClear();
        int originTurn = currentTurnIndex;
        int finalTurn = turns.Count - 1;
        while(index != currentTurnIndex)
        {
            if (index < currentTurnIndex)
            {
                if (currentTurnIndex < 0) break;
                turns[currentTurnIndex].GoPrev(true);
                --currentTurnIndex;
            }
            else if (index > currentTurnIndex)
            {
                if (currentTurnIndex > finalTurn) break;
                ++currentTurnIndex;
                turns[currentTurnIndex].GoNext(true);
            }
        }

        TurnIndexChanged(originTurn);
    }

    public static bool ClaimShowWantTurn(int index)
    {
        if (!instance) return false;
        instance.ShowWantTurn(index);
        return true;
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
        ShowWantTurn(-1);
    }

    public void ShowFinalTurn(bool value)
    {
        if (IsAnalysisMode) AnalysisModeEnd();
        ShowWantTurn(TurnFinalIndex);
    }

    public static bool ClaimShowFinalTurn()
    {
        if (!instance) return false;
        instance.ShowFinalTurn(false);
        return true;
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

	void LoadFinalTurn(in TurnBaseInfo newTurnInfo)
	{
		turns.Add(newTurnInfo);
		guides.Add(null);
		OnTurnAdded?.Invoke(TurnPassed, newTurnInfo);
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
}
