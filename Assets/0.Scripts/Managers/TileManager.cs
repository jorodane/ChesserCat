using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Rendering;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public struct TileMoveStruct
{
	public GameObject target;
	public ChessMovementModule movementModule;
	public Vector3Int previousTile; 
	public Vector3Int nextTile;
	public MoveCheckType moveType;
	public int moveDistance;
}

public struct TileInfo
{
	public GameObject objectOnTile;
	public CharacterBase characterOnTile;
	public Vector3Int position;
	public TileBaseType baseType;
	public TileDecoType decoType;

	public static readonly TileInfo Empty = new();
	public static readonly TileInfo Stone = new() { baseType = TileBaseType.Stone };
	public static readonly TileInfo Dirt  = new() { baseType = TileBaseType.Dirt };
	public static readonly TileInfo Grass = new() { baseType = TileBaseType.Dirt, decoType = TileDecoType.Grass };

	public readonly TileEnterException EnterCheck(in TileMoveStruct moveInfo)
	{
		switch (baseType)
		{
			case TileBaseType.None:
			case TileBaseType.Water:
				return TileEnterException.TileNotExist;
			default:
				break;
		}
		if (characterOnTile) return TileEnterException.AlreadyOwned;
		else if(objectOnTile) return TileEnterException.Block_Low;
		return TileEnterException.Possible;
	}
}

public delegate void TileMoveEvent(TileMoveStruct info);

public class TileManager : ManagerBase
{
	public readonly static Vector3Int diagonal_RU = new Vector3Int(1, 1);
	public readonly static Vector3Int diagonal_RD = new Vector3Int(1, -1);
	public readonly static Vector3Int diagonal_LU = new Vector3Int(-1, 1);
	public readonly static Vector3Int diagonal_LD = new Vector3Int(-1, -1);

	public static event TileMoveEvent VisualTileExitEvent;
	public static event TileMoveEvent VisualTilePassEvent;
	public static event TileMoveEvent VisualTileEnterEvent;


	//public static event TileMoveEvent ActualTileMoveEvent;

	Transform tileOffsetTransform;
	static Vector3 tileOffsetValue = new Vector3(-5.0f, -1.0f);
	static Vector3 tileOffsetVisual = new Vector3(0.0f, 0.0f);

	static TileInfo[,] tileInfos = new[,]
	{
		{TileInfo.Empty,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Empty,},
		{TileInfo.Empty,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,},
		{TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,},
		{TileInfo.Grass,TileInfo.Grass,TileInfo.Empty,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,},
		{TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,},
		{TileInfo.Empty,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,},
		{TileInfo.Empty,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Empty,},
	};

	static TileBase[,] tiles;

	List<GuideLine> guideLines = new();

	protected override IEnumerator OnConnected(GameManager newManager)
	{
		tileOffsetTransform = new GameObject("TileOffset").transform;
		tileOffsetTransform.position = tileOffsetVisual;
		CreateTileSet(tileInfos);

		VisualTileExitEvent -= OnVisualTileExit;
		VisualTileExitEvent += OnVisualTileExit;
		VisualTilePassEvent -= OnVisualTilePass;
		VisualTilePassEvent += OnVisualTilePass;
		VisualTileEnterEvent -= OnVisualTileEnter;
		VisualTileEnterEvent += OnVisualTileEnter;
		yield return null;
	}

	protected override void OnDisconnected()
	{
		VisualTileExitEvent -= OnVisualTileExit;
		VisualTilePassEvent -= OnVisualTilePass;
		VisualTileEnterEvent -= OnVisualTileEnter;
	}

	public void CreateTileSet(TileInfo[,] Infos)
	{
		int LengthX = Infos.GetLength(0);
		int LengthY = Infos.GetLength(1);
		tiles = new TileBase[LengthX, LengthY];

		for (int x = 0; x < LengthX; x++)
		{
			for (int y = 0; y < LengthY; y++)
			{
				TileInfo currentInfo = Infos[x, y];
				if (currentInfo.baseType == TileBaseType.None || currentInfo.decoType == TileDecoType.None) continue;
				currentInfo.position.x = x;
				currentInfo.position.y = y;
				CreateTile(currentInfo);
			}
		}
	}

	public TileBase CreateTile(TileInfo wantInfo)
	{
		TileBase result = null;
		GameObject instance = ObjectManager.CreateObject("Tile", tileOffsetTransform);
		if (instance)
		{
			result = instance.GetComponent<TileBase>();
			result.Set(wantInfo);
		}
		if (result)
		{
			tiles[wantInfo.position.x, wantInfo.position.y] = result;
		}
		return result;
	}

	protected void ClearGuideLine(List<GuideLine> guideLineList)
	{
		foreach(GuideLine current in guideLineList)
		{
			Destroy(current.gameObject);
		}
		guideLineList.Clear();
	}

	protected void ClearGuideLine() => ClearGuideLine(guideLines);
	public static void ClaimClearGuideLine() => GameManager.Tile.ClearGuideLine();

	protected GuideLine CreateGuideLine(List<GuideLine> guideLineList, Vector3Int from, Vector3Int to)
	{
		int removeCount = guideLineList.RemoveAll((target) => target?.TryRemove(from,to) ?? false);
		if (removeCount > 0) return null;
		if (!GetTile(from) || !GetTile(to)) return null;

		GameObject instance = ObjectManager.CreateObject("GuideLine");
		if(instance.TryGetComponent(out GuideLine result))
		{
			result.Set(from, to);
			guideLineList.Add(result);
		}
		return result;
	}
	protected GuideLine CreateGuideLine(Vector3Int from, Vector3Int to) => CreateGuideLine(guideLines, from, to);
	public static GuideLine ClaimCreateGuideLine(Vector3Int from, Vector3Int to) => GameManager.Tile.CreateGuideLine(from, to);

	public static bool PlaceObjectOnTile(GameObject target, Vector3Int wantPosition)
	{
		TileBase targetTile;
		TileBase lastTile;
		if (!TryGetTile(wantPosition, out targetTile)) return false;

		if (target.TryGetComponent(out ITilePlaceable asPlaceableObject))
		{
			if (TryGetTile(asPlaceableObject.CurrentTilePosition, out lastTile)) lastTile.SetObject(null);
		}

		return targetTile.SetObject(target);
	}

	public static void NotifyVisualTilePass(TileMoveStruct info) => VisualTilePassEvent?.Invoke(info);
	public void OnVisualTilePass(TileMoveStruct info)
	{
		if (TryGetTile(info.nextTile, out TileBase newTile)) newTile.VisualObjectPass(info);
	}

	public static void NotifyVisualTileEnter(TileMoveStruct info) => VisualTileEnterEvent?.Invoke(info);
	public void OnVisualTileEnter(TileMoveStruct info)
	{
		if (TryGetTile(info.nextTile, out TileBase newTile)) newTile.VisualObjectEnter(info);
	}

	public static void NotifyVisualTileExit(TileMoveStruct info) => VisualTileExitEvent?.Invoke(info);
	public void OnVisualTileExit(TileMoveStruct info)
	{
		if (TryGetTile(info.nextTile, out TileBase newTile)) newTile.VisualObjectExit(info);
	}

	public static void NoticeVisualTileMovable(Vector3Int info)
	{
		if (TryGetTile(info, out TileBase newTile)) newTile.VisualAvailableMove();
	}
	public static void NoticeVisualTileMovable(IEnumerable<Vector3Int> info)
	{
		foreach (Vector3Int currentTile in info) NoticeVisualTileMovable(currentTile);
	}

	public static void NoticeVisualTileMovable(ChessMovementModule movement)
	{
		if (movement == null) return;
		TileMoveStruct moveInfo = new()
		{
			moveType = movement.Checker,
			movementModule = movement,
			moveDistance = movement.MaxDistance,
			previousTile = movement.CurrentTile,
			target = movement.gameObject
		};
		foreach (Vector3Int currentTile in GetAvailableTilesOnStyle(movement.Style, movement.CurrentTile, moveInfo)) NoticeVisualTileMovable(currentTile);
	}

	public static void NoticeVisualTileClear(TileBase targetTile) { if (targetTile) targetTile.VisualAvailableClear(); }
	public static void NoticeVisualTileClear(Vector3Int info) { if (TryGetTile(info, out TileBase newTile)) NoticeVisualTileClear(newTile); }
	public static void NoticeVisualTileClearAll()
	{
		foreach (TileBase currentTile in tiles) { NoticeVisualTileClear(currentTile); }
	}

	public static Vector3Int GetTileCellPosition(Vector3 wantPosition)
	{
		wantPosition -= tileOffsetValue;
		return new Vector3Int(Mathf.RoundToInt(wantPosition.x), Mathf.RoundToInt(wantPosition.y * 1.33333f));
	}

	public static Vector3 GetTileWorldPosition(in Vector3Int wantTile)
	{
		return new Vector3(wantTile.x, wantTile.y * 0.75f) + tileOffsetValue;
	}

	public static bool TryGetTileInfo(in Vector3Int wantTile, out TileInfo result)
	{
		if (tileInfos.TryGetValue(wantTile.x, wantTile.y, out result)) return true;
		return false;
	}
	public static TileInfo GetTileInfo(in Vector3Int wantTile)
	{
		if (tileInfos.TryGetValue(wantTile.x, wantTile.y, out TileInfo result)) return result;
		return new();
	}

	public static bool TryGetTile(in Vector3Int wantTile, out TileBase result)
	{
		if (tiles.TryGetValue(wantTile.x, wantTile.y, out result)) return result;
		return false;
	}
	public static TileBase GetTile(in Vector3Int wantTile)
	{
		if (tiles.TryGetValue(wantTile.x, wantTile.y, out TileBase result)) return result;
		return null;
	}

	public static bool GetTileValid(in Vector3Int wantTile)
	{
		if (tiles.TryGetValue(wantTile.x, wantTile.y, out TileBase result)) return result;
		return false;
	}

	public static bool GetTileEnterable(in TileMoveStruct moveInfo, out TileInfo targetTileInfo, out TileEnterException exception)
	{
		if (TryGetTileInfo(moveInfo.nextTile, out targetTileInfo))
		{
			exception = targetTileInfo.EnterCheck(moveInfo);
		}
		else
		{
			exception = TileEnterException.TileNotExist;
		}

		return exception == TileEnterException.Possible;
	}

	public static bool IsDiagonal(in Vector3Int direction) => Mathf.Abs(direction.x) == Mathf.Abs(direction.y);
	public static bool IsStraight(in Vector3Int direction) => direction.x == 0 || direction.y == 0;
	public static bool IsDiagonalOrStraight(in Vector3Int direction) => IsDiagonal(direction) || IsStraight(direction);
	public static bool IsNotDiagonalOrStraight(Vector3Int direction) => !(IsDiagonal(direction) || IsStraight(direction));
	public static int GetDistance(in Vector3Int diff) => Mathf.Max(Mathf.Abs(diff.x), Mathf.Abs(diff.y));
	public static int GetDistance(in Vector3Int start, in Vector3Int end) => GetDistance(end - start);

	public static Vector3Int GetStraightDirection(in Vector3Int direction)
	{
		Vector3Int result = Vector3Int.zero;
		if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y)) result.x = direction.x.normalized();
		else												 result.y = direction.y.normalized();
		return result;
	}
	public static Vector3Int GetDiagonalDirection(Vector3Int direction)
	{
		direction.x = direction.x.normalized();
		direction.y = direction.y.normalized();
		return direction;
	}
	public static Vector3Int GetNextTileDirection(in Vector3Int start, in Vector3Int end)
	{
		if (start == end) return Vector3Int.zero;
		Vector3Int diff = end - start;
		if (IsDiagonal(diff)) return GetDiagonalDirection(diff);
		else					 return GetStraightDirection(diff);
	}

	public static IEnumerable<Vector3Int> GetTilePathDirection(Vector3Int start, Vector3Int end)
	{
		Vector3Int current = start;
		while (current != end)
		{
			Vector3Int next = GetNextTileDirection(current, end);
			current += next;
			yield return next;
		}
	}

	public static List<Vector3> GetTilePathPositions(Vector3Int start, Vector3Int end)
	{
		List<Vector3> result = new();
		result.Add(GetTileWorldPosition(start));
		Vector3Int current = start;
		while (current != end)
		{
			Vector3Int next = GetNextTileDirection(current, end);
			current += next;
			result.Add(GetTileWorldPosition(current));
		}
		return result;
	}

	public static IEnumerable<Vector3Int> GetTilesInRange(Vector3Int start, int range, System.Predicate<Vector3Int> relativePositionCondition = null, System.Predicate<TileBase> tileCondition = null)
	{

		Vector3Int leftDown = start;
		leftDown.x -= range;
		leftDown.y -= range;

		Vector3Int rightUp = start;
		rightUp.x += range;
		rightUp.y += range;

		Vector3Int current = Vector3Int.zero;
		Vector3Int diff;
		for(int x = leftDown.x; x <= rightUp.x; x++)
		{
			for (int y = leftDown.y; y <= rightUp.y; y++)
			{
				current.x = x;
				current.y = y;
				if (current == start) continue;
				if (TryGetTile(current, out TileBase currentTile))
				{
					diff = current - start;
					bool isContained = (relativePositionCondition?.Invoke(diff) ?? true) && (tileCondition?.Invoke(currentTile) ?? true);
					if(isContained) yield return current;
				}
			}
		}
	}

	public static IEnumerable<Vector3Int> GetAvailableTilesInRange(Vector3Int start, TileMoveStruct moveInfo, System.Predicate<Vector3Int> relativePositionCondition = null, System.Predicate<TileBase> tileCondition = null)
	{
		List<Vector3Int> passed = new();
		foreach(Vector3Int currentEndPoint in GetTilesInRange(start, moveInfo.moveDistance, relativePositionCondition, tileCondition))
		{
			foreach(Vector3Int currentPassPoint in GetAvailableTilesOnDestination(start, currentEndPoint, moveInfo))
			{
				if (currentPassPoint != currentEndPoint) continue;
				if (passed.Contains(currentPassPoint)) continue;
				passed.Add(currentPassPoint);
				yield return currentPassPoint;
			}
		}
	}
	public static IEnumerable<Vector3Int> GetAvailableTilesOnDirections(Vector3Int start, TileMoveStruct moveInfo, params Vector3Int[] directions)
	{
		foreach (Vector3Int currentDirection in directions)
		{
			foreach (Vector3Int directionTile in GetAvailableTilesOnPath(GetTileContinousDirection(start, currentDirection), start, moveInfo))
			{
				yield return directionTile;
			}
		}
	}
	public static IEnumerable<Vector3Int> GetAvailableTilesOnDirection(Vector3Int start, Vector3Int direction, TileMoveStruct moveInfo) => GetAvailableTilesOnPath(GetTileContinousDirection(start, direction), start, moveInfo);
	public static IEnumerable<Vector3Int> GetAvailableTilesOnDestination(Vector3Int start, Vector3Int end, TileMoveStruct moveInfo) => GetAvailableTilesOnPath(GetTilePathDirection(start, end), start, moveInfo);
	public static IEnumerable<Vector3Int> GetAvailableTilesOnPath(IEnumerable<Vector3Int> movementRule, Vector3Int start, TileMoveStruct moveInfo)
	{
		Vector3Int current = start;
		bool isObjectPassed = false;
		foreach (Vector3Int currentDirection in movementRule)
		{
			moveInfo.previousTile = current;
			moveInfo.nextTile = moveInfo.previousTile + currentDirection;
			++moveInfo.moveDistance;
			if (!GetTileEnterable(moveInfo, out TileInfo targetTileInfo, out TileEnterException exception))
			{
				if (exception == TileEnterException.Block_All) yield break;
				switch (moveInfo.moveType)
				{
					case MoveCheckType.Charge:
						switch (exception)
						{
							case TileEnterException.TileNotExist:
							case TileEnterException.AlreadyOwned:
							case TileEnterException.Block_Low:
								yield break;
						}
						break;
					case MoveCheckType.Through:
					case MoveCheckType.Jump:
						if (exception == TileEnterException.Block_High) yield break;
						break;
					case MoveCheckType.Range:
						if (exception == TileEnterException.Block_Low) yield break;
						break;
				}
				if (!isObjectPassed) isObjectPassed = targetTileInfo.characterOnTile != null || targetTileInfo.objectOnTile != null;
			}
			else if (moveInfo.moveType != MoveCheckType.Through || isObjectPassed) yield return moveInfo.nextTile;
			current = moveInfo.nextTile;
		}
	}
	public static IEnumerable<Vector3Int> GetTileContinousDirection(Vector3Int start, Vector3Int direction)
	{
		Vector3Int current = start;
		Vector3Int next = current + direction;
		while (tiles.TryGetValue(next.x, next.y, out TileBase tileInfo))
		{
			yield return direction;
			next += direction;
		}
		yield break;
	}


	public static IEnumerable<Vector3Int> GetAvailableTilesOnMainDiagonal(Vector3Int start, TileMoveStruct moveInfo) => GetAvailableTilesOnDirections(start, moveInfo, diagonal_LD, diagonal_RU);
	public static IEnumerable<Vector3Int> GetAvailableTilesOnAntiDiagonal(Vector3Int start, TileMoveStruct moveInfo) => GetAvailableTilesOnDirections(start, moveInfo, diagonal_LU, diagonal_RD);
	public static IEnumerable<Vector3Int> GetAvailableTilesOnDiagonals(Vector3Int start, TileMoveStruct moveInfo) => GetAvailableTilesOnDirections(start, moveInfo, diagonal_LD, diagonal_LU, diagonal_RD, diagonal_RU);
	public static IEnumerable<Vector3Int> GetAvailableTilesOnVertical(Vector3Int start, TileMoveStruct moveInfo) => GetAvailableTilesOnDirections(start, moveInfo, Vector3Int.up, Vector3Int.down);
	public static IEnumerable<Vector3Int> GetAvailableTilesOnHorizontal(Vector3Int start, TileMoveStruct moveInfo) => GetAvailableTilesOnDirections(start, moveInfo, Vector3Int.left, Vector3Int.right);
	public static IEnumerable<Vector3Int> GetAvailableTilesOnCross(Vector3Int start, TileMoveStruct moveInfo) => GetAvailableTilesOnDirections(start, moveInfo, Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right);
	public static IEnumerable<Vector3Int> GetAvailableTilesOnAllDirections(Vector3Int start, TileMoveStruct moveInfo) => GetAvailableTilesOnDirections(start, moveInfo, Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right, diagonal_LD, diagonal_LU, diagonal_RD, diagonal_RU);

	public static IEnumerable<Vector3Int> GetAvailableTilesOnStyle(MoveStyleType style, Vector3Int start, TileMoveStruct moveInfo) => style switch
	{
		MoveStyleType.King	 => GetAvailableTilesInRange(start, moveInfo),
		MoveStyleType.Queen	 => GetAvailableTilesOnAllDirections(start, moveInfo),
		MoveStyleType.Rook	 => GetAvailableTilesOnCross(start, moveInfo),
		MoveStyleType.Bishop => GetAvailableTilesOnDiagonals(start, moveInfo),
		MoveStyleType.Knight => GetAvailableTilesInRange(start, moveInfo, IsNotDiagonalOrStraight),
		MoveStyleType.Pawn	 => GetAvailableTilesOnDirection(start, moveInfo.movementModule?.OppositeDirection??Vector3Int.up, moveInfo),
		_					 => Enumerable.Empty<Vector3Int>(),
	};

}
