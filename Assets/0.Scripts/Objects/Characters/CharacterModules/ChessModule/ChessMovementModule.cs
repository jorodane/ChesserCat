using System;
using System.Collections;
using System.Linq;
using UnityEngine;

[System.Serializable]
public struct MoveTypeInfo
{
    public MoveStyleType style;
    public MoveCheckType checker;
    public int maxDistance;
}

public class ChessMovementModule : MovementModule
{
	public override System.Type RegistrationType => typeof(ChessMovementModule);

    [SerializeField] MoveTypeInfo _moveType;
    public MoveTypeInfo MoveType => _moveType;

    [SerializeField] MoveTypeInfo _attackType;
    public MoveTypeInfo AttackType => _attackType;

    TileEnterCheck _moveChecker;
    TileEnterCheck _attackChecker;

    public int movedTime = 0;
    public int MovableDistance => (MoveType.style == MoveStyleType.Pawn && movedTime <= 0) ? MoveType.maxDistance + 1 : MoveType.maxDistance;
    public int AttackableDistance => (AttackType.style == MoveStyleType.Pawn && movedTime <= 0) ? AttackType.maxDistance + 1 : AttackType.maxDistance;

	public Vector3Int OppositeDirection
    {
        get
        {
            if (Owner) return Owner.OppositeDirection;
            else return Vector3Int.up;
        }
        set
        {
            if (Owner) Owner.OppositeDirection = value;
        }
    }

    public Vector3Int CurrentTile
	{
		get
		{
			if (Owner) return Owner.CurrentTilePosition;
			else return Vector3Int.zero;
		}
		set
		{
			if (Owner) Owner.CurrentTilePosition = value;
		}
	}
	Vector3Int[] highlightedTile;
	Vector3Int moveNextTile;
    Vector3Int moveStartTile;
	Vector3Int moveEndTile;

	float moveTimeTotal = 0.2f;
	float attackTimeTotal = 0.75f;
	float moveTimePassed = 0.0f;

    public Vector3Int[] GetMovableTiles() => TileManager.GetAvailableTilesOnStyle(MoveType.style, CurrentTile, GenerateMoveInfo(), MovableDistance, _moveChecker).ToArray();
    public Vector3Int[] GetAttackableTiles() => TileManager.GetAvailableTilesOnStyle(AttackType.style, CurrentTile, GenerateMoveInfo(), AttackableDistance, _attackChecker).ToArray();

    public bool GetIsAttackable(GameObject other)
    {
        if(!other) return false;
        if (other.TryGetComponent(out CharacterBase otherAsCharacter)) return GetIsAttackable(otherAsCharacter);
        else return true;
    }

    public bool GetIsAttackable(CharacterBase other)
    {
        if(!other) return false;
        if (!Owner) return true;
        /////////////////////////////////////////////////////////////FOR TEST///////////////////////////////////////////////////////////////////////
        return other.OppositeDirection != Owner.OppositeDirection;
        //return other.Controller != Owner.Controller;
    }


    public override void OnRegistration(CharacterBase newOwner)
	{
		base.OnRegistration(newOwner);
        UpdateMoveChecker();
        UpdateAttackChecker();
        newOwner.OnHovered -= OnMouseHoverChanged;
		newOwner.OnHovered += OnMouseHoverChanged;
	}

	public override void OnUnregistration(CharacterBase oldOwner)
	{
		base.OnUnregistration(oldOwner);
		oldOwner.OnHovered -= OnMouseHoverChanged;
	}

	public override void UpdateToDirection(float deltaTime)
	{
		moveTimePassed += deltaTime;
		float timeRatio = moveTimePassed / moveTimeTotal;
		if (timeRatio >= 1.0f)
		{
			transform.position = TileManager.GetTileWorldPosition(moveNextTile);
			CurrentTile = moveNextTile;
			targetDirection = null;
		}
		else
		{
			Vector3 fromPosition = TileManager.GetTileWorldPosition(CurrentTile);
			Vector3 toPosition = TileManager.GetTileWorldPosition(moveNextTile);
			transform.position = Vector3.Lerp(fromPosition, toPosition, timeRatio);
		}
	}

	public override void UpdateToDestination(float deltaTime)
	{
		TileMoveStruct moveInfo = new TileMoveStruct()
		{
			previousTile = CurrentTile,
			nextTile = moveNextTile,
			moveType = MoveCheckType.Charge,
			target = gameObject
		};

		if (CurrentTile == moveEndTile)
		{
			targetDestination = null;
			moveInfo.nextTile = moveEndTile;
			TileManager.NotifyVisualTileEnter(moveInfo);
		}
		else
		{
			if(CurrentTile == moveStartTile) TileManager.NotifyVisualTileExit(moveInfo);
			else							 TileManager.NotifyVisualTilePass(moveInfo);
			MoveToDirection(TileManager.GetNextTileDirection(CurrentTile, moveEndTile));
        }
        return;
	}

	public override void MoveToDestination(Vector3 destination, float tolerance)
	{
        Vector3Int moveDestination = TileManager.GetTileCellPosition(destination);
		moveStartTile = CurrentTile;
		moveEndTile = moveDestination;
		targetDirection = null;
		targetDestination = destination;
	}

    public override void MoveToDirection(Vector3 direction)
	{
		if (direction.sqrMagnitude == 0.0f) return;
        Vector3Int moveDirection = new(direction.x.normalizedToInt(), direction.y.normalizedToInt());
		CurrentTile = moveNextTile;
		moveNextTile = CurrentTile + moveDirection;
		moveTimePassed = 0.0f;
		targetDirection = direction;
	}

	public override void StopMovement()
	{
		targetDirection = null;
		targetDestination = null;
	}

    public void OnMouseHoverChanged(bool isHovered)
	{
		if(isHovered) ShowPossibleTiles();
		else          HideHighlightTiles();
    }

    public void GetPossibleTiles(out Vector3Int[] movable, out Vector3Int[] attackable)
    {
        movable = GetMovableTiles();
        attackable = GetAttackableTiles();
    }

    public void ShowPossibleTiles()
    {
        if (TileManager.IsWaitInput()) return;
        HideHighlightTiles();
        GetPossibleTiles(out Vector3Int[] movable, out Vector3Int[] attackable);
        TileManager.NoticeHighlight(movable, TileHighlightType.Movable);
        TileManager.NoticeHighlight(attackable, TileHighlightType.Attackable);
        highlightedTile = attackable.Concat(movable).ToArray();
    }

    public void ShowMovementTiles()
    {
        HideHighlightTiles();
        highlightedTile = GetMovableTiles();
        TileManager.NoticeHighlight(highlightedTile, TileHighlightType.Movable);
    }

    public void ShowAttackTiles()
    {
        HideHighlightTiles();
        highlightedTile = GetAttackableTiles();
        TileManager.NoticeHighlight(highlightedTile, TileHighlightType.Attackable);
    }


    public void HideHighlightTiles()
    {
        if (TileManager.IsWaitInput()) return;
        if (highlightedTile is not null) TileManager.NoticeHighlightClear(highlightedTile, TileHighlightType.Movable, TileHighlightType.Attackable);
    }

    public TileMoveStruct GenerateMoveInfo()
    {
        TileMoveStruct result = new(this);

        return result;
    }

    public void NoticeMoved()
    {
        movedTime++;
    }

    public void NoticeMoveCanceled()
    {
        movedTime--;
    }

    public IEnumerator PlayMove(Vector3Int start, Vector3Int destination)
    {
        float totalTime = 0.0f;
        Vector3 fromPosition = TileManager.GetTileWorldPosition(start);
        Vector3 toPosition = TileManager.GetTileWorldPosition(destination);
        Vector3 direction = toPosition - fromPosition;
        while (totalTime < moveTimeTotal)
        {
            transform.position = Vector3.Lerp(fromPosition, toPosition, totalTime / moveTimeTotal);
            totalTime += Time.deltaTime;
            Owner.MovementNotify(direction);
            yield return null;
        }
        transform.position = toPosition;
        yield return null;
    }

    public IEnumerator PlayAttack(Vector3Int destination, CharacterBase targetCharacter)
    {
        Vector3 fromPosition = TileManager.GetTileWorldPosition(CurrentTile);
        Vector3 toPosition = TileManager.GetTileWorldPosition(destination);
        Vector3 direction = toPosition - fromPosition;
        Vector3 endPosition = toPosition - (direction * 0.5f);
        Owner.AnimationTriggerNotify(AnimationTriggerType.JumpAttack);
        Owner.MovementNotify(direction);
        yield return new WaitForSeconds(.25f);
        float totalTime = 0.25f;
        while (totalTime < 0.5f)
        {
            float percent = (totalTime - 0.25f) / 0.25f;
            transform.position = Vector3.Lerp(fromPosition, toPosition, percent);
            totalTime += Time.deltaTime;
            yield return null;
        }
        yield return new WaitForSeconds(.1f);
        while (totalTime < 0.7f)
        {
            float percent = (totalTime - 0.6f) / 0.1f;
            transform.position = Vector3.Lerp(toPosition, endPosition, percent);
            totalTime += Time.deltaTime;
            yield return null;
        }
        yield return new WaitForSeconds(.3f);

        Owner?.AnimationTriggerNotify(AnimationTriggerType.Reset);
        transform.position = fromPosition;
        yield return null;
    }




    public void UpdateMoveChecker()
    {
        _moveChecker = null;
        _moveChecker += TileChecker_MoveDistance;
        _moveChecker += TileChecker_Enterable;
        if (MoveType.style == MoveStyleType.Pawn) _moveChecker += TileCheckeer_OnlyForward;
    }

    public void UpdateAttackChecker()
    {
        _attackChecker = null;
        _attackChecker += TileChecker_AttackDistance;
        _attackChecker += TileChecker_Attackable;
        if (MoveType.style == MoveStyleType.Pawn) _attackChecker += TileCheckeer_OnlyForward;
    }

    private void TileCheckeer_OnlyForward(ref TileCheckStruct tileChecker)
    {
        if (!tileChecker.result) return;
        if (tileChecker.currentMoveInfo.IsForwardDirection) return;
        tileChecker.result = false;
        tileChecker.isStop = true;
    }

    void TileChecker_Enterable(ref TileCheckStruct tileChecker)
    {
        if (!tileChecker.result) return;

        if (TileManager.GetTileEnterable(tileChecker.currentMoveInfo, out TileInfo targetTileInfo, out TileEnterException exception))
        {
            tileChecker.accepter.Add(this);
            tileChecker.result &= true;
        }
        else
        {
            if (TileManager.GetTileExceptionValid(tileChecker.currentMoveInfo.moveType, exception))
            {
                tileChecker.result = false;
                if (tileChecker.currentMoveInfo.moveType == MoveCheckType.Charge || tileChecker.currentMoveInfo.moveType == MoveCheckType.Range) tileChecker.isStop = true;
            }
            else
            {
                tileChecker.accepter.Add(this);
                tileChecker.result &= true;
            }
        }
        if (!tileChecker.isObjectPassed) tileChecker.isObjectPassed = targetTileInfo.characterOnTile != null || targetTileInfo.objectOnTile != null;
    }

    void TileChecker_Attackable(ref TileCheckStruct tileChecker)
    {
        if (!tileChecker.result) return;

        if (TileManager.GetTileEnterable(tileChecker.currentMoveInfo, out TileInfo targetTileInfo, out TileEnterException exception))
        {
            tileChecker.result = false;
        }
        else
        {
            GameObject attackTarget = targetTileInfo.objectOnTile;
            if (exception == TileEnterException.AlreadyOwned)
            {
                if(targetTileInfo.characterOnTile) tileChecker.result &= GetIsAttackable(targetTileInfo.characterOnTile);
                else                               tileChecker.result &= GetIsAttackable(targetTileInfo.objectOnTile);
                if(tileChecker.result)             tileChecker.accepter.Add(this);
            }
            else if (TileManager.GetTileExceptionValid(tileChecker.currentMoveInfo.moveType, exception))
            {
                tileChecker.result = false;
            }

            if (tileChecker.currentMoveInfo.moveType == MoveCheckType.Charge || tileChecker.currentMoveInfo.moveType == MoveCheckType.Range) tileChecker.isStop = true;
        }
    }

    void TileChecker_MoveDistance(ref TileCheckStruct tileChecker)
    {
        if (!tileChecker.result) return;
        if (MovableDistance > 0 && tileChecker.currentMoveInfo.moveDistance > MovableDistance)
        {
            tileChecker.result = false;
            tileChecker.isStop = true;
        }
    }

    void TileChecker_AttackDistance(ref TileCheckStruct tileChecker)
    {
        if (!tileChecker.result) return;
        if (AttackableDistance > 0 && tileChecker.currentMoveInfo.moveDistance > AttackableDistance)
        {
            tileChecker.result = false;
            tileChecker.isStop = true;
        }
    }
}
