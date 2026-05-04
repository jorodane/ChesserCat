using UnityEngine;
using UnityEngine.EventSystems;

public class ChessMovementModule : MovementModule
{
	Vector3Int currentTile;
	public Vector3Int moveNextTile;
	public Vector3Int moveStartTile;
	public Vector3Int moveEndTile;

	float moveTimeTotal = 0.2f;
	float moveTimePassed = 0.0f;

	public override void UpdateToDirection(float deltaTime)
	{
		moveTimePassed += deltaTime;
		float timeRatio = moveTimePassed / moveTimeTotal;
		if (timeRatio >= 1.0f)
		{
			transform.position = TileManager.GetTileWorldPosition(moveNextTile);
			currentTile = moveNextTile;
			targetDirection = null;
		}
		else
		{
			Vector3 fromPosition = TileManager.GetTileWorldPosition(currentTile);
			Vector3 toPosition = TileManager.GetTileWorldPosition(moveNextTile);
			transform.position = Vector3.Lerp(fromPosition, toPosition, timeRatio);
		}
	}

	public override void UpdateToDestination(float deltaTime)
	{
		TileMoveStruct moveInfo = new TileMoveStruct()
		{
			previousTile = currentTile,
			nextTile = moveNextTile,
			moveType = MoveCheckType.Charge,
			target = gameObject
		};

		if (currentTile == moveEndTile)
		{
			targetDestination = null;
			moveInfo.nextTile = moveEndTile;
			TileManager.NotifyVisualTileEnter(moveInfo);
		}
		else
		{
			MoveToDirection(TileManager.GetNextTileDirection(currentTile, moveEndTile));

			if(currentTile == moveStartTile) TileManager.NotifyVisualTileExit(moveInfo);
			else							 TileManager.NotifyVisualTilePass(moveInfo);
		}
		return;
	}

	public override void MoveToDestination(Vector3 destination, float tolerance)
	{
		Vector3Int moveDestination = TileManager.GetTileCellPosition(destination);
		moveStartTile = currentTile;
		moveEndTile = moveDestination;
		targetDirection = null;
		targetDestination = destination;
		foreach(Vector3Int direction in TileManager.GetTilePathDirection(moveStartTile, moveEndTile))
		{
			Debug.Log(direction);
		}
	}

	public override void MoveToDirection(Vector3 direction)
	{
		if (direction.sqrMagnitude == 0.0f) return;

		Vector3Int moveDirection = new(direction.x.normalizedToInt(), direction.y.normalizedToInt());
		currentTile = moveNextTile;
		moveNextTile = currentTile + moveDirection;
		moveTimePassed = 0.0f;
		targetDirection = direction;
	}

	public override void StopMovement()
	{
		targetDirection = null;
		targetDestination = null;
	}
}
