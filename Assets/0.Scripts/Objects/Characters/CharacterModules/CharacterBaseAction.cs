using System.Collections.Generic;
using UnityEngine;

public partial class CharacterBase
{
    public virtual IEnumerable<TurnActionInfo> StartCharacterMove(ControllerBase wantPlayer, Vector3Int wantStart, Vector3Int wantDestination)
    {
        if (!wantPlayer) yield break;
        ChessMovementModule movement = GetModule<ChessMovementModule>();
        if (!movement) yield break;
        Vector3Int currentLocation = wantStart;
        switch (movement.MoveType.checker)
        {
            case MoveCheckType.Charge:
                {
                    foreach (Vector3Int nextTile in TileManager.GetTilePath(wantStart, wantDestination))
                    {
                        foreach (TurnActionInfo currentAction in MakeMoveAction(currentLocation, nextTile))
                        {
                            yield return currentAction;
                        }
                        currentLocation = nextTile;
                    }
                }
                break;

            default:
                foreach (TurnActionInfo currentAction in MakeMoveAction(currentLocation, wantDestination))
                {
                    yield return currentAction;
                }
                break;
        }
    }

    public virtual IEnumerable<TurnActionInfo> StartCharacterAttack(ControllerBase wantPlayer, Vector3Int wantStart, Vector3Int wantDestination)
    {
        CharacterBase wantTarget = TileManager.GetCharacter(wantDestination);
        if (!wantTarget) yield break;
        foreach (TurnActionInfo currentMove in StartCharacterMove(wantPlayer, wantStart, wantDestination))
        {
            if (currentMove is TurnActionInfo_Move asMovementInfo)
            {
                Vector3Int currentLocation = asMovementInfo.startLocation;
                Vector3Int nextLocation = asMovementInfo.actionLocation;
                GameObject obstacle = TileManager.GetObjectOnTile(nextLocation);
                if (obstacle)
                {
                    if (obstacle == wantTarget.gameObject)
                    {
                        foreach (TurnActionInfo currentAction in MakeAttackAction(currentLocation, nextLocation, obstacle, true))
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

    public virtual IEnumerable<TurnActionInfo> MakeMoveAction(Vector3Int currentLocation, Vector3Int wantDestination)
    {
        yield return new TurnActionInfo_Move(currentLocation, wantDestination, this);
    }

    public virtual IEnumerable<TurnActionInfo> MakeAttackAction(Vector3Int wantStart, Vector3Int wantDestination, GameObject wantTarget, bool tryEnterTile)
    {
        foreach (TurnActionInfo currentDamageAction in MakeDamageAction(wantStart, wantDestination, wantTarget)) yield return currentDamageAction;
        if (tryEnterTile)
        {
            if (TileManager.GetTileEnterable(wantDestination, wantDestination - wantStart, out TileEnterException exception))
            {
                yield return new TurnActionInfo_Move(wantStart, wantDestination, this);
            }
        }
    }

    public virtual IEnumerable<TurnActionInfo> MakeDamageAction(Vector3Int wantStart, Vector3Int wantDestination, GameObject wantTarget)
    {
        CharacterBase wantCharacter = wantTarget.GetComponent<CharacterBase>();
        Vector3Int knockbackDirection = wantStart.GetDirection(wantDestination);
        Vector3Int knockbackLocation = wantDestination + wantStart.GetDirection(wantDestination);
        if (TileManager.GetTileEnterable(knockbackLocation, knockbackDirection, out TileEnterException exception))
        {
            yield return new TurnActionInfo_Damage(wantDestination, this, wantDestination, wantCharacter);
        }
        else yield return new TurnActionInfo_Kill(wantStart, this, wantDestination, wantCharacter);
        yield break;
    }
}
