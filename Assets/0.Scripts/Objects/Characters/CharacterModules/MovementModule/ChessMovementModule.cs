using System;
using UnityEngine;

public class ChessMovementModule : MovementModule
{
	[SerializeField] MoveStyleType _style;
	public MoveStyleType Style => _style;

	[SerializeField] MoveCheckType _checker;
	public MoveCheckType Checker => _checker;

	[SerializeField] int _maxDistance;
	public int MaxDistance => _maxDistance;

	Vector3Int _oppositeDirection;
	public Vector3Int OppositeDirection => _oppositeDirection;

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
	Vector3Int moveNextTile;
	Vector3Int moveStartTile;
	Vector3Int moveEndTile;

	float moveTimeTotal = 0.2f;
	float moveTimePassed = 0.0f;

	public override void OnRegistration(CharacterBase newOwner)
	{
		base.OnRegistration(newOwner);
		newOwner.OnHovered -= ShowMovementTiles;
		newOwner.OnHovered += ShowMovementTiles;
	}

	public override void OnUnregistration(CharacterBase oldOwner)
	{
		base.OnUnregistration(oldOwner);
		oldOwner.OnHovered -= ShowMovementTiles;
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
			MoveToDirection(TileManager.GetNextTileDirection(CurrentTile, moveEndTile));

			if(CurrentTile == moveStartTile) TileManager.NotifyVisualTileExit(moveInfo);
			else							 TileManager.NotifyVisualTilePass(moveInfo);
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

	public void ShowMovementTiles(bool isHovered)
	{
		if(isHovered)
		{
			TileManager.NoticeVisualTileMovable(this);
		}
		else
		{
			TileManager.NoticeVisualTileClearAll();
		}
	}
}
