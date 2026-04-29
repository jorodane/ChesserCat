using System.Collections;
using UnityEditor.Experimental;
using UnityEngine;

public struct TileMoveStruct
{
	public GameObject target;
	public Vector3Int previousTile; 
	public Vector3Int nextTile;
	public MoveCheckType moveType;
}

public struct TileInfo
{
	public GameObject objectOnTile;
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

		return TileEnterException.Possible;
	}
}

public delegate void TileMoveEvent(TileMoveStruct info);

public class TileManager : ManagerBase
{
	public event TileMoveEvent OnTileMove;

	TileInfo[,] tileInfos = new[,] 
	{
		{TileInfo.Empty,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Empty,},
		{TileInfo.Empty,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,},
		{TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,},
		{TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,},
		{TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,},
		{TileInfo.Empty,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,},
		{TileInfo.Empty,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Empty,},
	};

	protected override IEnumerator OnConnected(GameManager newManager)
	{

		yield return null;
	}

	protected override void OnDisconnected()
	{

	}

	public static Vector3Int GetTileCellPosition(in Vector3 wantTile)
	{
		return new Vector3Int(Mathf.RoundToInt(wantTile.x), Mathf.RoundToInt(wantTile.y * 1.33333f));
	}

	public static Vector3 GetTileWorldPosition(in Vector3Int wantTile)
	{
		return new Vector3(wantTile.x, wantTile.y * 0.75f);
	}

	public bool TryGetTileInfo(in Vector3Int wantTile, out TileInfo result)
	{
		if (tileInfos.TryGetValue(wantTile.x, wantTile.y, out result))
		{
			return true;
		}
		return false;
	}
	public TileInfo GetTileInfo(in Vector3Int wantTile)
	{
		if(tileInfos.TryGetValue(wantTile.x, wantTile.y, out TileInfo result))
		{
			return result;
		}
		return new();
	}

	public bool GetTileEnterable(in TileMoveStruct moveInfo, out TileInfo targetTileInfo, out TileEnterException exception)
	{
		if(TryGetTileInfo(moveInfo.nextTile, out targetTileInfo))
		{
			exception = targetTileInfo.EnterCheck(moveInfo);
		}
		else
		{
			exception = TileEnterException.TileNotExist;
		}

		return exception == TileEnterException.Possible;
	}

	public static Vector3Int GetNextTileDirection(in Vector3Int start, in Vector3Int end)
	{
		Vector3Int result = Vector3Int.zero;
		if (start == end) return result;

		Vector3Int diff = end - start;
		Vector3Int absDiff = new(Mathf.Abs(diff.x), Mathf.Abs(diff.y));
		if (absDiff.x != absDiff.y)
		{
			if (absDiff.x > absDiff.y)
			{
				result.x = diff.x / absDiff.x;
				result.y = 0;
			}
			else
			{
				result.x = 0;
				result.y = diff.y / absDiff.y;
			}
		}
		else
		{
			result.x = diff.x / absDiff.x;
			result.y = diff.y / absDiff.y;
		}
		return result;
	}
}
