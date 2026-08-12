using System.Collections.Generic;
using UnityEngine;

public partial class CharacterBase
{
    public virtual IEnumerable<TurnActionInfo> StartCharacterMove(ControllerBase wantPlayer, Vector3Int wantStart, Vector3Int wantDestination)
    {
        if (!wantPlayer) yield break;
        if (!IsAlive) yield break;
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
                            currentLocation = nextTile;
                        }
                        if (!IsAlive) yield break;
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
        if (!IsAlive) yield break;
        CharacterBase wantTarget = TileManager.GetCharacter(wantDestination);
        if (!wantTarget) yield break;
        foreach (TurnActionInfo currentMove in StartCharacterMove(wantPlayer, wantStart, wantDestination))
        {
            yield return currentMove;
        }
        if (!IsAlive) yield break;
        foreach (TurnActionInfo currentAction in MakeAttackAction(CurrentTilePosition, wantTarget.CurrentTilePosition, wantTarget.gameObject))
        {
            yield return currentAction;
        }
    }

    public virtual IEnumerable<TurnActionInfo> MakeMoveAction(Vector3Int currentLocation, Vector3Int wantDestination)
    {
        if (!TileManager.GetTileEnterable(wantDestination, currentLocation, out TileEnterException exception)) yield break;

        yield return new TurnActionInfo_Move(currentLocation, wantDestination, this);
        if(IsDamaged) foreach (TurnActionInfo currentAttack in MakeRestoreAction(currentLocation, wantDestination, gameObject, 1)) yield return currentAttack;
    }

    public virtual IEnumerable<TurnActionInfo> MakeTryAttackAction(GameObject wantTarget)
    {
        if (!IsAlive) yield break;
        CharacterBase wantCharacter = wantTarget.GetComponent<CharacterBase>();
        if (!wantCharacter.IsAlive) yield break;
        yield return new TurnActionInfo_BaseAttackAnim(this, wantCharacter);
    }

    public virtual IEnumerable<TurnActionInfo> MakeAttackAction(Vector3Int wantStart, Vector3Int wantDestination, GameObject wantTarget)
    {
        foreach (TurnActionInfo currentAction in MakeTryAttackAction(wantTarget)) yield return currentAction;
        foreach (TurnActionInfo currentAction in MakeDamageAction(wantStart, wantDestination, wantTarget)) yield return currentAction;
        foreach (TurnActionInfo currentAction in MakeKnockBackAction(wantStart.GetDirection(wantDestination), wantTarget)) yield return currentAction;
        if (!IsAlive) yield break;
        bool completed = false;
        foreach (TurnActionInfo currentAction in MakeMoveAction(CurrentTilePosition, wantDestination))
        {
            yield return currentAction;
            completed = true;
        }
        if(!completed)
        {
            yield return new TurnActionInfo_ReturnToCurrentTile(this);
        }
    }

    public virtual IEnumerable<TurnActionInfo> MakeDamageAction(Vector3Int wantStart, Vector3Int wantDestination, GameObject wantTarget, int damage)
    {
        CharacterBase wantCharacter = wantTarget.GetComponent<CharacterBase>();
        yield return new TurnActionInfo_Damage(this, wantCharacter, damage);
        if(!wantCharacter.IsAlive) yield return new TurnActionInfo_Out(wantStart, this, wantCharacter.CurrentTilePosition, wantCharacter);
        yield break;
    }

    public virtual IEnumerable<TurnActionInfo> MakeRestoreAction(Vector3Int wantStart, Vector3Int wantDestination, GameObject wantTarget, int heal)
    {
        CharacterBase wantCharacter = wantTarget.GetComponent<CharacterBase>();
        yield return new TurnActionInfo_Restore(this, wantCharacter, heal);
        if (!wantCharacter.IsAlive) yield return new TurnActionInfo_Out(wantStart, this, wantCharacter.CurrentTilePosition, wantCharacter);
        yield break;
    }

    public virtual IEnumerable<TurnActionInfo> MakeDamageAction(Vector3Int wantStart, Vector3Int wantDestination, GameObject wantTarget)
        => MakeDamageAction(wantStart, wantDestination, wantTarget, GetAttackDamage(wantTarget.GetComponent<CharacterBase>()));

    public virtual IEnumerable<TurnActionInfo> MakeKnockBackAction(Vector3Int knockbackDirection, GameObject wantTarget)
    {
        CharacterBase wantCharacter = wantTarget.GetComponent<CharacterBase>();
        if (!wantCharacter.IsAlive) yield break;

        Vector3Int knockbackLocation = wantCharacter.CurrentTilePosition + knockbackDirection;
        if (TileManager.GetTileEnterable(knockbackLocation, knockbackDirection, out TileEnterException exception))
        {
            yield return new TurnActionInfo_KnockBack(wantCharacter.CurrentTilePosition, knockbackLocation, wantCharacter);
        }
    }

}
