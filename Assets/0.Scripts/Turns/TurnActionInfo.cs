using System;
using System.Collections;
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
public abstract class TurnActionInfo
{
    public abstract void GoNext();
    public abstract void GoPrev();
    public abstract IEnumerator Play();

}

[Serializable]
public class TurnActionInfo_Move : TurnActionInfo
{
    public Vector3Int startLocation;
    public Vector3Int actionLocation;
    public CharacterBase effectedCharacter;
    public int effectedCharacterID;

    public override string ToString() => $"{effectedCharacter?.DisplayInitial}{TileManager.GetTileText(actionLocation)}";

    public TurnActionInfo_Move(Vector3Int currentLocation, Vector3Int wantLocation, int wantCharacterID, CharacterBase wantCharacter)
    {
        startLocation = currentLocation;
        actionLocation = wantLocation;
        effectedCharacter = wantCharacter;
        effectedCharacterID = wantCharacterID;
    }
    public TurnActionInfo_Move(Vector3Int wantLocation, int wantCharacterID, CharacterBase wantCharacter)
    {
        effectedCharacter = wantCharacter;
        effectedCharacterID = wantCharacterID;
        actionLocation = wantLocation;
        if (effectedCharacter) startLocation = effectedCharacter.CurrentTilePosition;
        else actionLocation = -Vector3Int.one;
    }

    public override void GoNext()
    {
        if (!effectedCharacter) return;
        TileManager.PlaceObjectOnTile(effectedCharacter.gameObject, actionLocation, startLocation);
    }

    public override void GoPrev()
    {
        if (!effectedCharacter) return;
        TileManager.PlaceObjectOnTile(effectedCharacter.gameObject, startLocation);
    }

    public override IEnumerator Play()
    {
        if (effectedCharacter)
        {
            if (effectedCharacter.TryGetModule(out ChessMovementModule movement))
            {
                yield return movement.PlayMove(startLocation, actionLocation);
            }
        }
    }
}