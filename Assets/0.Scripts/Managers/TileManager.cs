using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct TileMoveStruct
{
	public GameObject target;
	public ChessMovementModule movementModule;
	public Vector3Int previousTile;
	public Vector3Int nextTile;
	public Vector3Int oppositeDirection;
	public MoveCheckType moveType;
	public int moveDistance;

	public TileMoveStruct(MoveCheckType wantMoveType, ChessMovementModule wantMovementModule, int wantMoveDistance, Vector3Int wantNextTile, GameObject wantTarget)
	{
		moveType				= wantMoveType;
		movementModule			= wantMovementModule;
        if (movementModule)     oppositeDirection = movementModule.OppositeDirection;
        else                    oppositeDirection = Vector3Int.down;
        moveDistance = wantMoveDistance;
		nextTile = previousTile = wantNextTile;
		target					= wantTarget;
	}

	public TileMoveStruct(ChessMovementModule targetModule)
	{
		moveType = targetModule.MoveType.checker;
		movementModule = targetModule;
        if (movementModule) oppositeDirection = movementModule.OppositeDirection;
        else oppositeDirection = Vector3Int.down;
        moveDistance = 0;
		nextTile = previousTile = targetModule.CurrentTile;
		target = targetModule.gameObject;
	}

    public TileMoveStruct(ChessMovementModule targetModule, Vector3Int startTile)
    {
        moveType = targetModule.MoveType.checker;
        movementModule = targetModule;
        if (movementModule) oppositeDirection = movementModule.OppositeDirection;
        else oppositeDirection = Vector3Int.down;
        moveDistance = 0;
        nextTile = previousTile = startTile;
        target = targetModule.gameObject;
    }
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

public struct PathInfo
{
	public GameObject moveObject;
	public ChessMovementModule movementModule;
	public Vector3Int endPoint;
	public Vector3Int[] path;
}


public delegate void TileMoveEvent(TileMoveStruct info);
public delegate void TileHoverEvent(Vector3Int hoverPosition, TileBase tile);
public delegate void TileEnterCheck(in TileMoveStruct moveInfo, in TileInfo targetTileInfo, ref bool result, ref bool stop);

public class TileManager : ManagerBase
{
    public readonly static Vector3    TileSize    = new Vector3(1.0f, 0.9f, 1.0f);

	public readonly static Vector3Int diagonal_RU = new Vector3Int(1, 1);
	public readonly static Vector3Int diagonal_RD = new Vector3Int(1, -1);
	public readonly static Vector3Int diagonal_LU = new Vector3Int(-1, 1);
	public readonly static Vector3Int diagonal_LD = new Vector3Int(-1, -1);

	public static event TileMoveEvent VisualTileExitEvent;
	public static event TileMoveEvent VisualTilePassEvent;
	public static event TileMoveEvent VisualTileEnterEvent;
    public static event TileHoverEvent TileHoverEvent;


	//public static event TileMoveEvent ActualTileMoveEvent;

	static Transform tileOffsetTransform;
	static Vector3 tileOffsetValue => tileOffsetTransform?.position ?? Vector3.zero;
	static Vector3 tileOffsetVisual = new Vector3(0.0f, 0.0f);

	static TileInfo[,] tileInfos = new[,]
	{
		{TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,},
		{TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,},
		{TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,},
		{TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,},
		{TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,},
		{TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,},
		{TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,},
		{TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,TileInfo.Grass,},
	};

	static TileBase[,] tiles;
	static CharacterBase inputWaitTarget;
    static Vector3Int[] inputWaitMovePositions;
    static Vector3Int[] inputWaitAttackPositions;

    Vector3Int _tileHoverPosition;
    public static Vector3Int TileHoverPosition => GameManager.Tile?._tileHoverPosition ?? Vector3Int.zero;

    Vector3 boardEntireSize;
    Vector3 boardHalfSize;
    Vector3 boardCenterPosition;
    Vector3 tileMoveDirection;
    float tileMoveSpeed = 5.0f;

	List<GuideLine> guideLines = new();

	protected override IEnumerator OnConnected(GameManager newManager)
	{
		tileOffsetTransform = new GameObject("TileOffset").transform;
		CreateTileSet(tileInfos);

		VisualTileExitEvent -= OnVisualTileExit;
		VisualTileExitEvent += OnVisualTileExit;
		VisualTilePassEvent -= OnVisualTilePass;
		VisualTilePassEvent += OnVisualTilePass;
		VisualTileEnterEvent -= OnVisualTileEnter;
		VisualTileEnterEvent += OnVisualTileEnter;
        GameManager.OnUpdateManager -= UpdateTilePosition;
        GameManager.OnUpdateManager += UpdateTilePosition;
        InputManager.OnMouseMove -= UpdateMousePosition;
        InputManager.OnMouseMove += UpdateMousePosition;
        InputManager.OnResetTilePosition -= ResetTilePosition;
        InputManager.OnResetTilePosition += ResetTilePosition;
        InputManager.OnTileMove -= MoveTilePosition;
        InputManager.OnTileMove += MoveTilePosition;

		yield return null;
	}

    private void UpdateTilePosition(float deltaTime)
    {
        if (tileOffsetTransform && tileMoveDirection.sqrMagnitude > 0)
        {
            Vector3 resultPosition = tileOffsetTransform.position + deltaTime * tileMoveSpeed * tileMoveDirection;

            resultPosition.x = Mathf.Clamp(resultPosition.x, boardCenterPosition.x - boardHalfSize.x, boardCenterPosition.x + boardHalfSize.x);
            resultPosition.y = Mathf.Clamp(resultPosition.y, boardCenterPosition.y - boardHalfSize.y, boardCenterPosition.y + boardHalfSize.y);
            tileOffsetTransform.position = resultPosition;
        }
    }

    private void UpdateMousePosition(Vector2 screenPosition, Vector3 worldPosition)
    {
        Vector3Int currentHoverTile = GetTileCellPosition(worldPosition);
        if(currentHoverTile != _tileHoverPosition)
        {
            _tileHoverPosition = currentHoverTile;
            TileHoverEvent?.Invoke(_tileHoverPosition, GetTile(TileHoverPosition));
        }
    }

    protected override void OnDisconnected()
	{
        GameManager.OnUpdateManager -= UpdateTilePosition;
        InputManager.OnMouseMove -= UpdateMousePosition;
		VisualTileExitEvent -= OnVisualTileExit;
        VisualTilePassEvent -= OnVisualTilePass;
		VisualTileEnterEvent -= OnVisualTileEnter;
        InputManager.OnResetTilePosition -= ResetTilePosition;
        InputManager.OnTileMove -= MoveTilePosition;
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
        boardEntireSize = new Vector3((LengthX - 1) * TileSize.x, (LengthY - 1) * TileSize.y);
        boardHalfSize = boardEntireSize * 0.5f;
        boardCenterPosition = (boardEntireSize * -0.5f) + tileOffsetVisual;
        ResetTilePosition();
	}

    public void ResetTilePosition(bool value = true)
    {
        if (!tileOffsetTransform) return;
        tileOffsetTransform.position = boardCenterPosition;
    }


    public static void SetTileOffsetVisual(in Vector3 newValue)
    {
        tileOffsetVisual = newValue;
        tileOffsetVisual.z = 0;
        GameManager.Tile?.ResetTilePosition();
    }

    private void MoveTilePosition(Vector2 value)
    {
        tileMoveDirection = value;
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


    public List<Vector3IntDirection> GetGuideLineDirections()
    {
        List<Vector3IntDirection> result = new();
        foreach (GuideLine current in guideLines)
        {
            result.Add(new() { start = current.StartPosition, destination = current.EndPosition });
        }
        return result;
    }

    public static List<Vector3IntDirection> ClaimGetGuideLineDirections() => GameManager.Tile?.GetGuideLineDirections();

    public void SetGuideLineDirections(List<Vector3IntDirection> directions)
    {
        ResetGuideLine();
        if (directions is null) return;
        foreach (Vector3IntDirection current in directions) CreateGuideLine(current.start, current.destination);
    }
    public static void ClaimSetGuideLineDirections(List<Vector3IntDirection> directions) => GameManager.Tile?.SetGuideLineDirections(directions);

    protected void ResetGuideLine(List<GuideLine> guideLineList)
	{
		foreach(GuideLine current in guideLineList)
		{
			Destroy(current.gameObject);
		}
		guideLineList.Clear();
	}

	protected void ResetGuideLine() => ResetGuideLine(guideLines);
	public static void ClaimResetGuideLine() => GameManager.Tile?.ResetGuideLine();

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
        if (!TryGetTile(wantPosition, out TileBase targetTile)) return false;

        if (target.TryGetComponent(out ITilePlaceable asPlaceableObject))
		{
			if (TryGetTile(asPlaceableObject.CurrentTilePosition, out TileBase lastTile)) lastTile.SetObject(null);
		}
		return targetTile.SetObject(target);
	}

    public static bool PlaceObjectOnTile(GameObject target, Vector3Int wantPosition, Vector3Int originPosition)
    {
        if (!TryGetTile(wantPosition, out TileBase targetTile)) return false;

        if (target.TryGetComponent(out ITilePlaceable asPlaceableObject))
        {
            if (TryGetTile(originPosition, out TileBase lastTile)) lastTile.SetObject(null);
        }
        return targetTile.SetObject(target);
    }

    public static void SetMovePositionInput(CharacterBase target)
    {
        if (!target)
        {
            inputWaitMovePositions = null;
            return;
        }
        ChessMovementModule inputWaitMovement = target.GetModule<ChessMovementModule>();
        if (!inputWaitMovement) return;
        inputWaitMovePositions = inputWaitMovement.GetMovableTiles();
        NoticeHighlight(inputWaitMovePositions, TileHighlightType.Movable);
    }

    public static void SetAttackPositionInput(CharacterBase target)
    {
        if (!target)
        {
            inputWaitAttackPositions = null;
            return;
        }
        ChessMovementModule inputWaitMovement = target.GetModule<ChessMovementModule>();
        if (!inputWaitMovement) return;
        inputWaitAttackPositions = inputWaitMovement.GetAttackableTiles();
        NoticeHighlight(inputWaitAttackPositions, TileHighlightType.Attackable);
    }

    public static bool SetCharacterInput(CharacterBase target)
    {
        NoticeHighlightClearAll(TileHighlightType.Movable, TileHighlightType.Attackable);
        inputWaitTarget = target;
        SetMovePositionInput(target);
        SetAttackPositionInput(target);
        return true;
    }

    public static bool SetCharacterMoveInput(CharacterBase target)
	{
        NoticeHighlightClearAll(TileHighlightType.Movable, TileHighlightType.Attackable);
		inputWaitTarget = target;
        SetMovePositionInput(target);
        inputWaitAttackPositions = null;
        return true;
	}

    public static IEnumerable<TurnActionInfo_Move> StartCharacterMove(ControllerBase wantPlayer, CharacterBase wantCharacter, Vector3Int wantStart, Vector3Int wantDestination)
    {
        if(!wantCharacter || !wantPlayer) yield break;
        ChessMovementModule movement = wantCharacter.GetModule<ChessMovementModule>();
        Vector3Int currentLocation = wantStart;

        switch(movement.MoveType.checker)
        {
            case MoveCheckType.Charge:
            {
                foreach (Vector3Int nextTile in GetTilePath(wantStart, wantDestination))
                {
                    yield return new(currentLocation, nextTile, wantPlayer.GetCharacterToID(wantCharacter), wantCharacter);
                    currentLocation = nextTile;
                }
            }
            break;

            default:
                yield return new(currentLocation, wantDestination, wantPlayer.GetCharacterToID(wantCharacter), wantCharacter);
            break;
        }

    }

    public static bool StartCharacterAttackInput(CharacterBase target)
	{
        NoticeHighlightClearAll(TileHighlightType.Movable, TileHighlightType.Attackable);
        inputWaitTarget = target;
        SetAttackPositionInput(target);
        inputWaitMovePositions = null;
        return true;
	}

	public static bool EndInput()
	{
        if(inputWaitTarget)
        {
            inputWaitTarget = null;
            inputWaitMovePositions = null;
            inputWaitAttackPositions = null;
        }
        NoticeHighlightClearAll(TileHighlightType.Movable, TileHighlightType.Attackable);
		return true;
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
		if (TryGetTile(info.previousTile, out TileBase newTile)) newTile.VisualObjectExit(info);
	}

    public static void NoticeHighlight(Vector3Int info, TileHighlightType wantType)
    {
        if (TryGetTile(info, out TileBase newTile))
        {
			newTile.AddHighlight(wantType);
        }
    }

    public static void NoticeHighlight(IEnumerable<Vector3Int> info, TileHighlightType wantType)
    {
        if (info is null) return;
        foreach (Vector3Int currentTile in info) NoticeHighlight(currentTile, wantType);
    }

    public static void NoticeHighlightClear(IEnumerable<Vector3Int> info, TileHighlightType wantType)
    {
        if (info is null) return;
        foreach (Vector3Int currentTile in info) NoticeHighlightClear(currentTile, wantType);
    }

    public static void NoticeHighlightClear(IEnumerable<Vector3Int> info, params TileHighlightType[] wantType)
    {
        if (info is null) return;
        foreach (Vector3Int currentTile in info) NoticeHighlightClear(currentTile, wantType);
    }

    public static void NoticeHighlightClear(TileBase targetTile, TileHighlightType wantType)
    {
        if (targetTile) targetTile.RemoveHighlight(wantType);
    }

    public static void NoticeHighlightClear(TileBase targetTile, params TileHighlightType[] wantTypes) 
	{
		if (targetTile) targetTile.RemoveHighlight(wantTypes);
	}
	public static void NoticeHighlightClear(Vector3Int info, TileHighlightType wantType) { if (TryGetTile(info, out TileBase newTile)) NoticeHighlightClear(newTile, wantType); }
	public static void NoticeHighlightClear(Vector3Int info, params TileHighlightType[] wantType) { if (TryGetTile(info, out TileBase newTile)) NoticeHighlightClear(newTile, wantType); }
	public static void NoticeHighlightClearAll(TileHighlightType wantType)
	{
        if (tiles is null) return;
		foreach (TileBase currentTile in tiles) { NoticeHighlightClear(currentTile, wantType); }
	}

    public static void NoticeHighlightClearAll(params TileHighlightType[] wantType)
    {
        if (tiles is null) return;
        TileHighlightType mask = TileHighlightType.None;
        foreach (TileHighlightType currentType in wantType) mask |= currentType;
        foreach (TileBase currentTile in tiles) { NoticeHighlightClear(currentTile, mask); }
    }

    public static TileBase GetTileFromText(string text)
    {
        if (string.IsNullOrEmpty(text)) return null;
        if (!text.AsAlgebraicChessNotation(out Vector3Int position)) return null;
        if(!TryGetTile(position, out TileBase result)) return null;
        return result;
    }

    public static string GetTileHorizonText(int index) => $"{(char)('A' + index)}";
    public static string GetTileHorizonText_Lower(int index) => $"{(char)('a' + index)}";
    public static string GetTileVerticalText(int index) => $"{1 + index}";

    public static string GetTileText(Vector3Int wantTile) => GetTileHorizonText_Lower(wantTile.x) + GetTileVerticalText(wantTile.y);

	public static Vector3Int GetTileCellPosition(Vector3 wantPosition)
	{
		wantPosition -= tileOffsetValue;
		return new Vector3Int(Mathf.RoundToInt(wantPosition.x), Mathf.RoundToInt(wantPosition.y / TileSize.y));
	}

	public static Vector3 GetTileWorldPosition(in Vector3Int wantTile) => new (GetTileWorldPositionX(wantTile), GetTileWorldPositionY(wantTile));

    public static float GetTileWorldPositionX(in Vector3Int wantTile) => wantTile.x * TileSize.x + tileOffsetValue.x;
    public static float GetTileWorldPositionY(in Vector3Int wantTile) => wantTile.y * TileSize.y + tileOffsetValue.y;

    public static Vector3 GetTileScreenPosition(in Vector3Int wantTile) => CameraManager.GetScreenPosition(GetTileWorldPosition(wantTile));
    public static Vector3 GetTileScreenPositionHorizontal(int index) => CameraManager.GetScreenPosition(GetTileWorldPosition(Vector3Int.right * index));
    public static Vector3 GetTileScreenPositionVertical(int index) => CameraManager.GetScreenPosition(GetTileWorldPosition(Vector3Int.up * index));

    public static bool TryGetTileInfo(in Vector3Int wantTile, out TileInfo result)
	{
        if(TryGetTile(wantTile, out TileBase resultTile))
        {
            if(resultTile)
            {
                result = resultTile.Info;
                return true;
            }
        }
        result = default;
		return false;
	}

    public static bool TryGetTileInfo(in string algebraicNotation, out TileInfo result)
    {
        if (algebraicNotation.AsAlgebraicChessNotation(out Vector3Int position)) return TryGetTileInfo(position, out result);
        result = default;
        return false;
    }

    public static TileInfo GetTileInfo(in Vector3Int wantTile)
	{
        if (TryGetTile(wantTile, out TileBase resultTile))
        {
            if (resultTile) return resultTile.Info;
        }
        return default;
    }
    public static TileInfo GetTileInfo(in string algebraicNotation)
    {
        if (algebraicNotation.AsAlgebraicChessNotation(out Vector3Int position)) return GetTileInfo(position);
        return default;
    }


    public static bool TryGetTile(in Vector3Int wantTile, out TileBase result)
	{
		if (tiles.TryGetValue(wantTile.x, wantTile.y, out result)) return result;
		return false;
	}

    public static bool TryGetTile(in string algebraicNotation, out TileBase result)
    {
        if (algebraicNotation.AsAlgebraicChessNotation(out Vector3Int position)) return TryGetTile(position, out result);
        result = null;
        return false;
    }

    public static TileBase GetTile(in Vector3Int wantTile)
	{
		if (tiles.TryGetValue(wantTile.x, wantTile.y, out TileBase result)) return result;
		return null;
	}

    public static TileBase GetTile(in string algebraicNotation)
    {
        if (algebraicNotation.AsAlgebraicChessNotation(out Vector3Int position)) return GetTile(position);
        return null;
    }

    public static CharacterBase GetCharacter(in string algebraicNotation)
    {
        if(algebraicNotation.AsAlgebraicChessNotation(out Vector3Int position)) return GetCharacter(position);
        return null;
    }

    public static CharacterBase GetCharacter(in Vector3Int wantTile)
    {
        if(TryGetTileInfo(wantTile, out TileInfo result)) return result.characterOnTile;
        return null;
    }

    public static GameObject GetObject(in string algebraicNotation)
    {
        if (algebraicNotation.AsAlgebraicChessNotation(out Vector3Int position)) return GetObject(position);
        return null;
    }

    public static GameObject GetObject(in Vector3Int wantTile)
    {
        if (TryGetTileInfo(wantTile, out TileInfo result)) return result.objectOnTile;
        return null;
    }


    public static bool GetTileValid(in Vector3Int wantTile)
	{
		if (tiles.TryGetValue(wantTile.x, wantTile.y, out TileBase result)) return result;
		return false;
	}

    public static bool GetTileExceptionValid(MoveCheckType moveType, TileEnterException exception)
    {
        switch (moveType)
        {
            case MoveCheckType.Charge:
                switch (exception)
                {
                    case TileEnterException.TileNotExist:
                    case TileEnterException.AlreadyOwned:
                    case TileEnterException.Block_Low:
                        return true;
                }
                break;
            case MoveCheckType.Through:
            case MoveCheckType.Jump:
                if (exception == TileEnterException.Block_High) return true;
                break;
            case MoveCheckType.Range:
                if (exception == TileEnterException.Block_Low) return true;
                break;
        }
        return false;
    }

    public static bool GetTileEnterable(in TileMoveStruct moveInfo, out TileInfo targetTileInfo, out TileEnterException exception)
	{
		if (TryGetTileInfo(moveInfo.nextTile, out targetTileInfo))
        {
			exception = GetTileEnterable(moveInfo, targetTileInfo);
        }
		else
		{
			exception = TileEnterException.TileNotExist;
		}

		return exception == TileEnterException.Possible;
	}

    public static TileEnterException GetTileEnterable(in TileMoveStruct moveInfo, in TileInfo targetTileInfo) => targetTileInfo.EnterCheck(moveInfo);

    public static CharacterBase GetWaitInputCharacter() => inputWaitTarget;
    public static bool IsWaitInput() => GetWaitInputCharacter() != null;
    public static bool HasLegalMove(in CharacterBase target) => target != null && target == inputWaitTarget && inputWaitMovePositions is not null && inputWaitMovePositions.Length > 0;
    public static bool IsLegalMove(in Vector3Int position) => inputWaitMovePositions.Contains(position);
    public static bool IsLegalMove(in CharacterBase target, in Vector3Int position) => HasLegalMove(target) && IsLegalMove(position);
    public static bool IsIllegalMove(in Vector3Int position) => !IsLegalMove(position);
    public static bool IsIllegalMove(in CharacterBase target, in Vector3Int position) => !IsLegalMove(target, position);
    public static bool HasLegalAttack(in CharacterBase target) => target != null && target == inputWaitTarget && inputWaitAttackPositions is not null && inputWaitAttackPositions.Length > 0;
    public static bool IsLegalAttack(in Vector3Int position) => inputWaitAttackPositions.Contains(position);
    public static bool IsLegalAttack(in CharacterBase target, in Vector3Int position) => HasLegalAttack(target) && IsLegalAttack(position);
    public static bool IsIllegalAttack(in Vector3Int position) => !IsLegalAttack(position);
    public static bool IsIllegalAttack(in CharacterBase target, in Vector3Int position) => !IsLegalAttack(target, position);

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

    public static IEnumerable<Vector3Int> GetTilePath(Vector3Int start, Vector3Int end)
    {
        Vector3Int current = start;
        foreach (Vector3Int nextDirection in GetTilePathDirection(start, end))
        {
            yield return current += nextDirection;
        }
    }

    public static IEnumerable<Vector3> GetTilePathPositions(Vector3Int start, Vector3Int end)
	{
		yield return GetTileWorldPosition(start);
		Vector3Int current = start;
		while (current != end)
		{
			Vector3Int next = GetNextTileDirection(current, end);
			current += next;
            yield return GetTileWorldPosition(current);
		}
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








    public static IEnumerable<Vector3Int> GetAvailableTilesOnPath(IEnumerable<Vector3Int> movementDelta, Vector3Int start, TileMoveStruct moveInfo, TileEnterCheck checker)
    {
        Vector3Int current = start;
        bool isObjectPassed = false;
        foreach (Vector3Int currentDirection in movementDelta)
        {
            moveInfo.previousTile = current;
            moveInfo.nextTile = moveInfo.previousTile + currentDirection;
            ++moveInfo.moveDistance;

            if (checker is null) yield return moveInfo.nextTile;
            else
            {
                if(TryGetTile(current, out TileBase currentTile))
                {
                    TileInfo targetTileInfo = currentTile.Info;
                    bool result = true, stop = false;
                    checker(moveInfo, targetTileInfo, ref result, ref stop);
                    if (result) yield return moveInfo.nextTile;
                    if (stop)   yield break;
                    if (!isObjectPassed) isObjectPassed = targetTileInfo.characterOnTile != null || targetTileInfo.objectOnTile != null;
                }
            }
            current = moveInfo.nextTile;
        }
    }



    public static IEnumerable<Vector3Int> GetAvailableTilesInRange(Vector3Int start, TileMoveStruct moveInfo, int range, TileEnterCheck checker, System.Predicate<Vector3Int> relativePositionCondition = null, System.Predicate<TileBase> tileCondition = null)
    {
        List<Vector3Int> passed = new();
        foreach (Vector3Int currentEndPoint in GetTilesInRange(start, range, relativePositionCondition, tileCondition))
        {
            foreach (Vector3Int currentPassPoint in GetAvailableTilesOnDestination(start, currentEndPoint, moveInfo, checker))
            {
                if (currentPassPoint != currentEndPoint) continue;
                if (passed.Contains(currentPassPoint)) continue;
                passed.Add(currentPassPoint);
                yield return currentPassPoint;
            }
        }
    }

    public static IEnumerable<Vector3Int> GetAvailableTilesOnDirections(Vector3Int start, TileMoveStruct moveInfo, TileEnterCheck checker, params Vector3Int[] directions)
	{
		foreach (Vector3Int currentDirection in directions)
		{
			TileMoveStruct currentMoveInfo = moveInfo;
			foreach (Vector3Int directionTile in GetAvailableTilesOnPath(GetTileContinousDirection(start, currentDirection), start, currentMoveInfo, checker))
			{
				yield return directionTile;
			}
		}
	}
	public static IEnumerable<Vector3Int> GetAvailableTilesOnDirection(Vector3Int start, Vector3Int direction, TileMoveStruct moveInfo, TileEnterCheck checker) => GetAvailableTilesOnPath(GetTileContinousDirection(start, direction), start, moveInfo, checker);
	public static IEnumerable<Vector3Int> GetAvailableTilesOnDestination(Vector3Int start, Vector3Int end, TileMoveStruct moveInfo, TileEnterCheck checker) => GetAvailableTilesOnPath(GetTilePathDirection(start, end), start, moveInfo, checker);
	public static IEnumerable<Vector3Int> GetTileContinousDirection(Vector3Int start, Vector3Int direction)
	{
        if (direction == Vector3Int.zero) yield break;
		Vector3Int current = start;
		Vector3Int next = current + direction;
		while (tiles.TryGetValue(next.x, next.y, out TileBase tileInfo))
		{
			yield return direction;
			next += direction;
		}
		yield break;
	}


	public static IEnumerable<Vector3Int> GetAvailableTilesOnMainDiagonal(Vector3Int start, TileMoveStruct moveInfo, TileEnterCheck checker) => GetAvailableTilesOnDirections(start, moveInfo, checker, diagonal_LD, diagonal_RU);
	public static IEnumerable<Vector3Int> GetAvailableTilesOnAntiDiagonal(Vector3Int start, TileMoveStruct moveInfo, TileEnterCheck checker) => GetAvailableTilesOnDirections(start, moveInfo, checker, diagonal_LU, diagonal_RD);
	public static IEnumerable<Vector3Int> GetAvailableTilesOnDiagonals(Vector3Int start, TileMoveStruct moveInfo, TileEnterCheck checker) => GetAvailableTilesOnDirections(start, moveInfo, checker, diagonal_LD, diagonal_LU, diagonal_RD, diagonal_RU);
	public static IEnumerable<Vector3Int> GetAvailableTilesOnVertical(Vector3Int start, TileMoveStruct moveInfo, TileEnterCheck checker) => GetAvailableTilesOnDirections(start, moveInfo, checker, Vector3Int.up, Vector3Int.down);
	public static IEnumerable<Vector3Int> GetAvailableTilesOnHorizontal(Vector3Int start, TileMoveStruct moveInfo, TileEnterCheck checker) => GetAvailableTilesOnDirections(start, moveInfo, checker, Vector3Int.left, Vector3Int.right);
	public static IEnumerable<Vector3Int> GetAvailableTilesOnCross(Vector3Int start, TileMoveStruct moveInfo, TileEnterCheck checker) => GetAvailableTilesOnDirections(start, moveInfo, checker, Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right);
	public static IEnumerable<Vector3Int> GetAvailableTilesOnAllDirections(Vector3Int start, TileMoveStruct moveInfo, TileEnterCheck checker) => GetAvailableTilesOnDirections(start, moveInfo, checker, Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right, diagonal_LD, diagonal_LU, diagonal_RD, diagonal_RU);

	public static IEnumerable<Vector3Int> GetAvailableTilesOnStyle(MoveStyleType style, Vector3Int start, TileMoveStruct moveInfo, int range, TileEnterCheck checker) => style switch
	{
		MoveStyleType.King	 => GetAvailableTilesInRange(start, moveInfo, range, checker),
		MoveStyleType.Queen	 => GetAvailableTilesOnAllDirections(start, moveInfo, checker),
		MoveStyleType.Rook	 => GetAvailableTilesOnCross(start, moveInfo, checker),
		MoveStyleType.Bishop => GetAvailableTilesOnDiagonals(start, moveInfo, checker),
		MoveStyleType.Knight => GetAvailableTilesInRange(start, moveInfo, range, checker, IsNotDiagonalOrStraight),
		MoveStyleType.Pawn	 => GetAvailableTilesOnVertical(start, moveInfo, checker),
		_					 => Enumerable.Empty<Vector3Int>(),
	};

}
