using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct Vector3IntDirection
{
    public Vector3Int start;
    public Vector3Int destination;

    public readonly Vector3Int Direction => destination - start;
}

[Serializable]
public struct HealthDeltaData
{
    public CharacterBase character;
    public int delta;

    public override readonly string ToString() => $"{character} : {delta}";
}

[Serializable]
public class TurnBaseInfo : ISavable
{
    public TurnActionInfo[] actionList;
    public string turnContext;
    public int turnIndex;
    public int playerID;
    public ControllerBase player;
    public int characterID;
    public CharacterBase character;
    public Vector3Int start;
    public Vector3Int destination;
    int playCursor;

    bool IsPlayed => playCursor >= (actionList?.Length ?? -1);
    bool IsWait => playCursor <= 0;

    public void GoNext(bool resetAnim)
    {
        if (IsPlayed) return;
        while (playCursor < actionList.Length)
        {
            actionList[playCursor].GoNext(resetAnim);
            playCursor++;
        }
        NoticeMoved();
    }

    public IEnumerator Play()
    {
        if(!IsPlayed)
        {
            while (playCursor < actionList.Length)
            {
                yield return actionList[playCursor].Play();
                actionList[playCursor].GoNext(true);
                playCursor++;
            }
            NoticeMoved();
        }
    }

    public void GoPrev(bool resetAnim)
    {
        if (IsWait) return;
        while (playCursor > 0)
        {
            playCursor--;
            actionList[playCursor].GoPrev(resetAnim);
        }
        NoticeMoveCanceled();
    }

    void NoticeMoved()
    {
        if (character && character.TryGetModule(out ChessMovementModule asChessMove)) asChessMove.NoticeMoved();
    }

    void NoticeMoveCanceled()
    {
        if (character && character.TryGetModule(out ChessMovementModule asChessMove)) asChessMove.NoticeMoveCanceled();
    }

    public void TurnHighlight()
    {
        TileManager.NoticeHighlight(start, TileHighlightType.LastMove);
        TileManager.NoticeHighlight(destination, TileHighlightType.LastMove);
    }

    public void TurnHighlightClear()
    {
        TileManager.NoticeHighlightClear(start, TileHighlightType.LastMove);
        TileManager.NoticeHighlightClear(destination, TileHighlightType.LastMove);
    }

    public IEnumerable<HealthDeltaData> GetHealthDelta()
    {
        foreach (TurnActionInfo currentAction in actionList)
        {
            foreach (HealthDeltaData currentDelta in currentAction.GetHealthDelta())
            {
                yield return currentDelta;
            }
        }
    }

    public TurnSaveData MakeSaveData() => new()
    {
        saveDataList = this.MakeCustomSaveData(),
        actionList = actionList.MakeActionSaveDataArray(),
        characterID = characterID,
        playerID = playerID,
        turnContext = turnContext,
        turnIndex = turnIndex,
        start = start,
        destination = destination,
    };

    public void ConstructCustomSaveData(ref Dictionary<string, string> result){}
}