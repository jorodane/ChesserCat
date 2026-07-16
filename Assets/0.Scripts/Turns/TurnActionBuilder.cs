using System.Collections.Generic;
using UnityEngine;

public static class TurnActionBuilder
{
    public static TurnActionInfo[] BuildActionArray(this IEnumerable<TurnActionInfo> progress)
    {
        List<TurnActionInfo> result = new ();

        try
        {
            foreach (TurnActionInfo currentAction in progress)
            {
                if (currentAction is null) continue;
                currentAction.GoNext();
                result.Add(currentAction);
            }
        }
        finally
        {
            for (int i = result.Count - 1; i >= 0; --i)
            {
                result[i].GoPrev();
            }
        }

        return result.ToArray();
    }


    public static IEnumerable<TurnActionInfo> StartCharacterMove(ControllerBase wantPlayer, CharacterBase wantCharacter, Vector3Int wantStart, Vector3Int wantDestination)
    {
        if (!wantCharacter || !wantPlayer) yield break;
        ChessMovementModule movement = wantCharacter.GetModule<ChessMovementModule>();
        if (!movement) yield break;
        Vector3Int currentLocation = wantStart;
        switch (movement.MoveType.checker)
        {
            case MoveCheckType.Charge:
                {
                    foreach (Vector3Int nextTile in TileManager.GetTilePath(wantStart, wantDestination))
                    {
                        foreach (TurnActionInfo currentAction in wantCharacter.MakeMoveAction(currentLocation, nextTile))
                        {
                            yield return currentAction;
                        }
                        currentLocation = nextTile;
                    }
                }
                break;

            default:
                foreach (TurnActionInfo currentAction in wantCharacter.MakeMoveAction(currentLocation, wantDestination))
                {
                    yield return currentAction;
                }
                break;
        }
    }

    public static IEnumerable<TurnActionInfo> StartCharacterAttack(ControllerBase wantPlayer, CharacterBase wantCharacter, Vector3Int wantStart, Vector3Int wantDestination)
    {
        CharacterBase wantTarget = TileManager.GetCharacter(wantDestination);
        if (!wantTarget) yield break;
        foreach (TurnActionInfo currentMove in StartCharacterMove(wantPlayer, wantCharacter, wantStart, wantDestination))
        {
            if (currentMove is TurnActionInfo_Move asMovementInfo)
            {
                Vector3Int currentLocation = asMovementInfo.startLocation;
                Vector3Int nextLocation = asMovementInfo.actionLocation;
                GameObject obstacle = TileManager.GetObjectOnTile(nextLocation);
                if (obstacle)
                {
                    if(obstacle == wantTarget.gameObject)
                    {
                        foreach (TurnActionInfo currentAction in wantCharacter.MakeAttackAction(currentLocation, nextLocation, obstacle, true))
                        {
                            yield return currentAction;
                        }
                    }
                    yield break;
                }
            }
            yield return currentMove;
        }
    }
}
