using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct Vector3IntDirection
{
    public Vector3Int start;
    public Vector3Int destination;

    public readonly Vector3Int Direction => start - destination;
}

[Serializable]
public struct TurnBaseInfo
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
    int playedIndex;

    public void GoNext()
    {
        for (; playedIndex < actionList.Length; playedIndex++) actionList[playedIndex].GoNext();
        playedIndex = actionList.Length - 1;
        if (character?.TryGetModule(out ChessMovementModule asChessMove) ?? false) asChessMove.NoticeMoved();
    }

    public IEnumerator Play()
    {
        for (; playedIndex < actionList.Length; playedIndex++)
        {
            yield return actionList[playedIndex].Play();
            actionList[playedIndex].GoNext();
        }
        playedIndex = actionList.Length - 1;
        if (character?.TryGetModule(out ChessMovementModule asChessMove) ?? false) asChessMove.NoticeMoved();
    }

    public void GoPrev()
    {
        for (; playedIndex >= 0; playedIndex--) actionList[playedIndex].GoPrev();
        playedIndex = 0;
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