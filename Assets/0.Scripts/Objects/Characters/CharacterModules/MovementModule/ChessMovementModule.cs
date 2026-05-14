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

	Vector3Int _currentTile;
	public Vector3Int CurrentTile => _currentTile;
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
			_currentTile = moveNextTile;
			targetDirection = null;
		}
		else
		{
			Vector3 fromPosition = TileManager.GetTileWorldPosition(_currentTile);
			Vector3 toPosition = TileManager.GetTileWorldPosition(moveNextTile);
			transform.position = Vector3.Lerp(fromPosition, toPosition, timeRatio);
		}
	}

	public override void UpdateToDestination(float deltaTime)
	{
		TileMoveStruct moveInfo = new TileMoveStruct()
		{
			previousTile = _currentTile,
			nextTile = moveNextTile,
			moveType = MoveCheckType.Charge,
			target = gameObject
		};

		if (_currentTile == moveEndTile)
		{
			targetDestination = null;
			moveInfo.nextTile = moveEndTile;
			TileManager.NotifyVisualTileEnter(moveInfo);
		}
		else
		{
			MoveToDirection(TileManager.GetNextTileDirection(_currentTile, moveEndTile));

			if(_currentTile == moveStartTile) TileManager.NotifyVisualTileExit(moveInfo);
			else							 TileManager.NotifyVisualTilePass(moveInfo);
		}
		return;
	}

	public override void MoveToDestination(Vector3 destination, float tolerance)
	{
		Vector3Int moveDestination = TileManager.GetTileCellPosition(destination);
		moveStartTile = _currentTile;
		moveEndTile = moveDestination;
		targetDirection = null;
		targetDestination = destination;
	}

	public override void MoveToDirection(Vector3 direction)
	{
		if (direction.sqrMagnitude == 0.0f) return;

		Vector3Int moveDirection = new(direction.x.normalizedToInt(), direction.y.normalizedToInt());
		_currentTile = moveNextTile;
		moveNextTile = _currentTile + moveDirection;
		moveTimePassed = 0.0f;
		targetDirection = direction;
	}

	public override void StopMovement()
	{
		targetDirection = null;
		targetDestination = null;
	}

	private void ShowMovementTiles(bool isHovered)
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
