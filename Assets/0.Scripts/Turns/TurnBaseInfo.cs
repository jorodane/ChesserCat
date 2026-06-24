using System;
using System.Collections;
using UnityEngine;

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
        for (; playedIndex >= 0; playedIndex--) actionList[playedIndex].GoPrev();
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