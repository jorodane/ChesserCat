using System.Collections;
using System.Linq;
using UnityEngine;

public class ChessMovementModule : MovementModule
{
	public override System.Type RegistrationType => typeof(ChessMovementModule);

	[SerializeField] MoveStyleType _style;
	public MoveStyleType Style => _style;

	[SerializeField] MoveCheckType _checker;
	public MoveCheckType Checker => _checker;

    public int movedTime = 0;

	[SerializeField] int _maxDistance;
    public int MaxDistance => (Style == MoveStyleType.Pawn && movedTime == 0) ? _maxDistance + 1 : _maxDistance;

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

    public Vector3Int[] GetMovableTiles()
    {
        return TileManager.GetAvailableTilesOnStyle(Style, CurrentTile, GenerateMoveInfo(), MaxDistance).ToArray();
    }

	public void ShowMovementTiles(bool isHovered)
	{
		if(isHovered)
		{
			TileManager.NoticeHighlightMovable(this);
		}
		else
		{
			TileManager.NoticeHighlightClearAll(TileHighlightType.Movable);
        }
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
        float totalTime = 0.0f;;
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
        //CurrentTile = destination;
        yield return null;
    }
}
