using UnityEngine;

public class ChessMovementModule : MovementModule
{
	Vector2Int currentTile;
	public Vector2Int moveNextTile;
	public Vector2Int moveStartTile;
	public Vector2Int moveEndTile;
	public bool isMoving;

	float moveTimeTotal = 0.5f;
	float moveTimePassed = 0.0f;

	public override void UpdateToDirection(float deltaTime){}

	public override void UpdateToDestination(float deltaTime)
	{
		if(isMoving)
		{
			moveTimePassed += deltaTime;
			float timeRatio = moveTimePassed / moveTimeTotal;
			if(timeRatio >= 1.0f)
			{
				transform.position = (Vector3Int)moveNextTile;
				currentTile = moveNextTile;
				if(currentTile != moveNextTile)
				{
					Vector2Int diff = moveNextTile - currentTile;
					Vector2Int absDiff = new Vector2Int(Mathf.Abs(diff.x), Mathf.Abs(diff.y));
					Vector2Int nextDirection = Vector2Int.down;
					if (absDiff.x != absDiff.y)
					{
						if(absDiff.x > absDiff.y)
						{
							nextDirection.x = diff.x / absDiff.x;
							nextDirection.y = 0;
						}
						else
						{
							nextDirection.x = 0;
							nextDirection.y = diff.y / absDiff.y;
						}
					}
					else
					{
						nextDirection.x = diff.x / absDiff.x;
						nextDirection.y = diff.y / absDiff.y;
					}
				
					moveNextTile = currentTile;
				}
			}
			else
			{
				transform.position = Vector3.Lerp((Vector3Int)currentTile, (Vector3Int)moveNextTile, timeRatio);
			}
			return;
		}
	}

	public override void MoveToDestination(Vector3 destination, float tolerance)
	{
		Vector2Int moveDestination = new (Mathf.RoundToInt(destination.x), Mathf.RoundToInt(destination.y));
		moveStartTile = currentTile;
		moveEndTile = currentTile + moveDestination;
		targetDestination = destination;
		isMoving = true;
	}

	public override void MoveToDirection(Vector3 direction)
	{
		Vector2Int moveDirection = new(direction.x.normalizedToInt(), direction.y.normalizedToInt());
		moveStartTile = currentTile;
		moveEndTile = currentTile + moveDirection;
		targetDestination = transform.position + direction;
		isMoving = true;
	}

	public override void StopMovement()
	{

	}
}
