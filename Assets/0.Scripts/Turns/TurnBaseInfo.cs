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
public class TurnBaseInfo
{
    public string turnContext;
    public int turnCount;
    public int playerID;
    public ControllerBase player;
    public int characterID;
    public CharacterBase character;
    public Vector3Int start;
    public Vector3Int destination;
    public TurnActionInfo[] actionList;
    int playCursor;

    bool IsPlayed => playCursor >= (actionList?.Length ?? -1);
    bool IsWait => playCursor <= 0;

    public void GoNext()
    {
        if (IsPlayed) return;
        while (playCursor < actionList.Length)
        {
            actionList[playCursor].GoNext();
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
                actionList[playCursor].GoNext();
                playCursor++;
            }
            NoticeMoved();
        }
    }

    public void GoPrev()
    {
        if (IsWait) return;
        while (playCursor > 0)
        {
            playCursor--;
            actionList[playCursor].GoPrev();
        }
        NoticeMoveCanceled();
    }

    void NoticeMoved()
    {
        if (character?.TryGetModule(out ChessMovementModule asChessMove) ?? false) asChessMove.NoticeMoved();
    }

    void NoticeMoveCanceled()
    {
        if (character?.TryGetModule(out ChessMovementModule asChessMove) ?? false) asChessMove.NoticeMoveCanceled();
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
}