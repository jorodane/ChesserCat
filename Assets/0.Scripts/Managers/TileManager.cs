using System.Collections.Generic;
using System.Collections;
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
	public static event TileMoveEvent VisualTileExitEvent;
	public static event TileMoveEvent VisualTilePassEvent;
	public static event TileMoveEvent VisualTileEnterEvent;

	public static event TileMoveEvent ActualTileMoveEvent;

	Transform tileOffsetTransform;
	static Vector3 tileOffsetValue = new Vector3(-5.0f,-1.0f);
	static Vector3 tileOffsetVisual = new Vector3(0.0f, 0.0f);

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

	TileBase[,] tiles;

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
		if(instance)
		{
			result = instance.GetComponent<TileBase>();
			result.Set(wantInfo);
		}
		if(result)
		{
			tiles[wantInfo.position.x, wantInfo.position.y] = result;
		}
		return result;
	}

	public static void NotifyVisualTilePass(TileMoveStruct info) => VisualTilePassEvent?.Invoke(info);
	public void OnVisualTilePass(TileMoveStruct info)
	{
		if(TryGetTile(info.nextTile, out TileBase newTile)) newTile.VisualObjectPass(info);
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

	public static Vector3Int GetTileCellPosition(Vector3 wantTile)
	{
		wantTile -= tileOffsetValue;
		return new Vector3Int(Mathf.RoundToInt(wantTile.x), Mathf.RoundToInt(wantTile.y * 1.33333f));
	}

	public static Vector3 GetTileWorldPosition(in Vector3Int wantTile)
	{
		return new Vector3(wantTile.x, wantTile.y * 0.75f) + tileOffsetValue;
	}

	public bool TryGetTileInfo(in Vector3Int wantTile, out TileInfo result)
	{
		if (tileInfos.TryGetValue(wantTile.x, wantTile.y, out result)) return true;
		return false;
	}
	public TileInfo GetTileInfo(in Vector3Int wantTile)
	{
		if(tileInfos.TryGetValue(wantTile.x, wantTile.y, out TileInfo result)) return result;
		return new();
	}

	public bool TryGetTile(in Vector3Int wantTile, out TileBase result)
	{
		if (tiles.TryGetValue(wantTile.x, wantTile.y, out result)) return result;
		return false;
	}
	public TileBase GetTile(in Vector3Int wantTile)
	{
		if (tiles.TryGetValue(wantTile.x, wantTile.y, out TileBase result)) return result;
		return null;
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

	public static Queue<Vector3Int> GetTilePath(in Vector3Int start, in Vector3Int end)
	{
		Queue<Vector3Int> result = new();
		Vector3Int current = start;
		while (current != end)
		{
			Vector3Int next = GetNextTileDirection(current, end);
			result.Enqueue(next);
			current = next;
		}
		return result;
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
