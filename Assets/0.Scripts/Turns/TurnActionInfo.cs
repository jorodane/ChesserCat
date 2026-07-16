using System;
using System.Collections;
using UnityEngine;


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

    public TurnActionInfo_Move(Vector3Int currentLocation, Vector3Int wantLocation, CharacterBase wantCharacter)
    {
        startLocation = currentLocation;
        actionLocation = wantLocation;
        effectedCharacter = wantCharacter;
        effectedCharacterID = wantCharacter?.GetID() ?? -1;
    }
    public TurnActionInfo_Move(Vector3Int wantLocation, CharacterBase wantCharacter)
    {
        effectedCharacter = wantCharacter;
        effectedCharacterID = wantCharacter?.GetID() ?? -1;
        actionLocation = wantLocation;
        if (effectedCharacter) startLocation = effectedCharacter.CurrentTilePosition;
        else actionLocation = -Vector3Int.one;
    }

    public override void GoNext()
    {
        if (!effectedCharacter) return;
        TileManager.PlaceObjectOnTile(effectedCharacter.gameObject, actionLocation);
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

[Serializable]
public class TurnActionInfo_Kill : TurnActionInfo
{
    public CharacterBase causeCharacter;
    public int causeCharacterID;

    public CharacterBase effectedCharacter;
    public int effectedCharacterID;

    public Vector3Int startLocation;
    public Vector3Int actionLocation;

    public override string ToString() => $"{causeCharacter?.DisplayInitial}x{(TileManager.GetTileText(actionLocation))}";

    public TurnActionInfo_Kill(in Vector3Int fromLocation, CharacterBase fromCharacter, in Vector3Int wantLocation, CharacterBase wantCharacter)
    {
        causeCharacter = fromCharacter;
        causeCharacterID = fromCharacter?.GetID() ?? -1;
        effectedCharacter = wantCharacter;
        effectedCharacterID = wantCharacter?.GetID() ?? -1;
        actionLocation = wantLocation;
        startLocation = fromLocation;
    }

    public override void GoNext()
    {
        if (!effectedCharacter) return;
        effectedCharacter.VisualizeKill();
    }

    public override void GoPrev()
    {
        if (!effectedCharacter) return;
        effectedCharacter.UnVisualizekill(actionLocation);
    }

    public override IEnumerator Play()
    {
        if (causeCharacter)
        {
            if (causeCharacter.TryGetModule(out ChessMovementModule movement))
            {
                yield return movement.PlayAttack(actionLocation, effectedCharacter);
            }
        }
    }
}