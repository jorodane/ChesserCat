using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.FilePathAttribute;
using static UnityEngine.UI.Image;

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

    public Vector3Int Direction => nextTile - previousTile;
    public bool IsForwardDirection => Direction.DirectionCheck(oppositeDirection);
}

public struct TileCheckStruct
{
    public TileMoveStruct currentMoveInfo;
    public HashSet<Object> accepter;
    public bool isObjectPassed;
    public bool isStop;
    public bool result;
}

public struct TileInfo
{
	public GameObject objectOnTile;
	public CharacterBase characterOnTile;
	public ITilePlaceable placeableOnTile;
	public Vector3Int location;
	public TileBasement basement;
	public string basementVariation;
	public TileDecoration decoration;
	public string decorationVariation;
    public WallBasement wallBasement;
    public string wallBasementVariation;
    public WallDecoration wallDecoration;
    public string wallDecorationVariation;

    public TileInfo(Vector3Int wantLocation, 
        TileBasement wantBasement, string wantBasementVariation, 
        TileDecoration wantDecoration, string wantDecorationVariation,
        WallBasement wantWallBasement, string wantWallBasementVariation,
        WallDecoration wantWallDecoration, string wantWallDecorationVariation
        ) 
    {
        objectOnTile = null;
        characterOnTile = null;
		placeableOnTile = null;
        location = wantLocation;
		basement = wantBasement;
		basementVariation = wantBasementVariation;
		decoration = wantDecoration;
		decorationVariation = wantDecorationVariation;
        wallBasement = wantWallBasement;
        wallBasementVariation = wantWallBasementVariation;
        wallDecoration = wantWallDecoration;
        wallDecorationVariation = wantWallDecorationVariation;
    }

    public TileInfo(TileSaveData data)
    {
        objectOnTile = null;
        characterOnTile = null;
		placeableOnTile = null;
        location = data.location;
		basement = TileManager.GetBasement(data.basement);
		basementVariation = data.basementVariation;
		decoration = TileManager.GetDecoration(data.decoration);
		decorationVariation = data.decorationVariation;
        wallBasement = TileManager.GetWallBasement(data.wallBasement);
        wallBasementVariation = data.basementVariation;
        wallDecoration = TileManager.GetWallDecoration(data.wallDecoration);
        wallDecorationVariation = data.decorationVariation;
    }

	public readonly TileEnterException EnterCheck()
	{
		TileEnterException		result = TileEnterException.Possible;
		if (!basement)			result |= TileEnterException.TileNotExist;
		else					result |= basement.EnterCheck();
		if (decoration)			result |= decoration.EnterCheck();
		if (characterOnTile)	result |= TileEnterException.AlreadyOwned;
		else if(objectOnTile)	result |= TileEnterException.Block_Low;

		return result;
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
public delegate void TileEnterCheck(ref TileCheckStruct tileChecker);
public delegate void TileOffsetChangeEvent(in Vector3 newOffset);
public delegate void BoardSizeChangeEvent(int width, int height);

public class TileManager : ManagerBase, ISavable<BoardSaveData>
{
    public readonly static Vector3    tileSize     = new (0.98f, 0.7f);
    public readonly static Vector2    boardPadding_LR = new (2.0f, 2.0f);
    public readonly static Vector2    boardPadding_UD = new (4.0f, 2.0f);

	public readonly static Vector3Int diagonal_RU = new (1, 1);
	public readonly static Vector3Int diagonal_RD = new (1, -1);
	public readonly static Vector3Int diagonal_LU = new (-1, 1);
	public readonly static Vector3Int diagonal_LD = new (-1, -1);

	public static event TileMoveEvent VisualTileExitEvent;
	public static event TileMoveEvent VisualTilePassEvent;
	public static event TileMoveEvent VisualTileEnterEvent;
    public static event TileHoverEvent TileHoverEvent;
    public static event TileOffsetChangeEvent OnTileOffsetChanged;
    public static event BoardSizeChangeEvent OnBoardSizeChanged;


	public static TileBasement defaultBasement;
	public static TileDecoration defaultDecoration;
	public static WallBasement defaultWallBasement;
	public static WallDecoration defaultWallDecoration;
    //public static event TileMoveEvent ActualTileMoveEvent;

    static Transform tileOffsetTransform;
	public static Vector3 TileOffsetValue => tileOffsetTransform ? tileOffsetTransform.position : Vector3.zero;

	static TileBase[,] tiles;
	static CharacterBase inputWaitTarget;
    static Vector3Int[] inputWaitMovePositions;
    static Vector3Int[] inputWaitAttackPositions;
	static Dictionary<string, TileBasement> tileBasementDictionary;
	static Dictionary<string, TileDecoration> tileDecorationDictionary;
	static Dictionary<string, WallBasement> wallBasementDictionary;
	static Dictionary<string, WallDecoration> wallDecorationDictionary;

    Vector3Int _tileHoverPosition;
    public static Vector3Int TileHoverPosition => GameManager.Tile?._tileHoverPosition ?? Vector3Int.zero;

    Vector3 boardCenterPosition;
    Rect boardRect;
	Vector2Int _boardSize;
	public static Vector2Int BoardSize => GameManager.Tile? GameManager.Tile._boardSize : Vector2Int.zero;

	List<GuideLine> guideLines = new();

    public BoardSaveData MakeSaveData()
    {
        List<TileSaveData> result = new();
		if (tiles is null) return new() { saveDataList = this.MakeCustomSaveData() };
        foreach (TileBase currentTile in tiles)
        {
            if (!currentTile) continue;
            result.Add(currentTile.MakeSaveData());
        }
        return new()
        {
            saveDataList = this.MakeCustomSaveData(),
            boardSize = GetRealBoardSize(),
            tileList = result.ToArray()
        };
    }

    public void LoadData(in BoardSaveData data)
    {
        ResetAll();
        CreateTileSet(data);
    }

    protected override IEnumerator OnConnected(GameManager newManager)
	{
		tileBasementDictionary = new();
		foreach (TileBasement currentBasement in DataManager.GetAllDataFromDictionary<TileBasement>())
		{
			if(!currentBasement) continue;
			if(tileBasementDictionary.TryAdd(currentBasement.name, currentBasement)) currentBasement.Initialize();
		}

		tileDecorationDictionary = new();
		foreach (TileDecoration currentDecoration in DataManager.GetAllDataFromDictionary<TileDecoration>())
		{
			if (!currentDecoration) continue;
			if(tileDecorationDictionary.TryAdd(currentDecoration.name, currentDecoration)) currentDecoration.Initialize();
		}

		wallBasementDictionary = new();
		foreach (WallBasement currentWall in DataManager.GetAllDataFromDictionary<WallBasement>())
		{
			if (!currentWall) continue;
			if (wallBasementDictionary.TryAdd(currentWall.name, currentWall)) currentWall.Initialize();
		}

		wallDecorationDictionary = new();
		foreach (WallDecoration currentWallDecoration in DataManager.GetAllDataFromDictionary<WallDecoration>())
		{
			if (!currentWallDecoration) continue;
			if (wallDecorationDictionary.TryAdd(currentWallDecoration.name, currentWallDecoration)) currentWallDecoration.Initialize();
		}

		defaultBasement			= GetBasement("Dirt");
		defaultDecoration		= null;
		defaultWallBasement		= null;
		defaultWallDecoration	= null;

		tileOffsetTransform = new GameObject("TileOffset").transform;
        OnTileOffsetChanged?.Invoke(TileOffsetValue);

		VisualTileExitEvent -= OnVisualTileExit;
		VisualTileExitEvent += OnVisualTileExit;
		VisualTilePassEvent -= OnVisualTilePass;
		VisualTilePassEvent += OnVisualTilePass;
		VisualTileEnterEvent -= OnVisualTileEnter;
		VisualTileEnterEvent += OnVisualTileEnter;
        InputManager.OnMouseMove -= UpdateMousePosition;
        InputManager.OnMouseMove += UpdateMousePosition;
		CameraManager.OnCameraPositionChanged -= UpdateCameraPosition;
		CameraManager.OnCameraPositionChanged += UpdateCameraPosition;
		yield return null;
	}


	protected override void OnDisconnected()
	{
		VisualTileExitEvent -= OnVisualTileExit;
        VisualTilePassEvent -= OnVisualTilePass;
		VisualTileEnterEvent -= OnVisualTileEnter;
        InputManager.OnMouseMove -= UpdateMousePosition;
		CameraManager.OnCameraPositionChanged -= UpdateCameraPosition;
        ResetAll();
	}

    public void ResetAll()
    {
        ResetGuideLine();
        ResetTileSet();
        EndInput();
    }

	private void UpdateMousePosition(Vector2 screenPosition, Vector3 worldPosition) => OnHoverTileChanged(worldPosition);

	void UpdateCameraPosition(Camera targetCamera, Vector3 newPosition) => OnHoverTileChanged(InputManager.CursorWorldPosition);

	void OnHoverTileChanged(Vector3 worldPosition)
	{
		Vector3Int currentHoverTile = GetTileCellPosition(worldPosition);
		if (currentHoverTile != _tileHoverPosition)
		{
			_tileHoverPosition = currentHoverTile;
			TileHoverEvent?.Invoke(_tileHoverPosition, GetTile(TileHoverPosition));
		}
	}

	public void CreateTileSet(in BoardSaveData data)
    {
        int LengthX = data.boardSize.x;
        int LengthY = data.boardSize.y;
        tiles = new TileBase[LengthX, LengthY];

        float tileHalfSizeX = tileSize.x * 0.5f;
        float tileHalfSizeY = tileSize.y * 0.5f;

        boardRect = Rect.zero;
        if(data.tileList.Length > 0)
        {
            TileSaveData initialTile = data.tileList[0];
            Vector3 initialTileLocation =  GetTileWorldPosition(initialTile.location);
            boardRect.xMin = initialTileLocation.x - tileHalfSizeX;
            boardRect.yMin = initialTileLocation.y - tileHalfSizeY;
            boardRect.xMax = initialTileLocation.x + tileHalfSizeX;
            boardRect.yMax = initialTileLocation.y + tileHalfSizeY;

            foreach (TileSaveData currentTile in data.tileList)
            {
                CreateTile(new TileInfo(currentTile));
                Vector3 instanceTileLocation = GetTileWorldPosition(currentTile.location);
                boardRect.xMin = Mathf.Min(boardRect.xMin, instanceTileLocation.x - tileHalfSizeX);
                boardRect.yMin = Mathf.Min(boardRect.yMin, instanceTileLocation.y - tileHalfSizeY);
                boardRect.xMax = Mathf.Max(boardRect.xMax, instanceTileLocation.x + tileHalfSizeX);
                boardRect.yMax = Mathf.Max(boardRect.yMax, instanceTileLocation.y + tileHalfSizeY);
            }
            boardRect.xMax += boardPadding_LR.y;
            boardRect.xMin -= boardPadding_LR.x;
            boardRect.yMax += boardPadding_UD.x;
            boardRect.yMin -= boardPadding_UD.y;
        }

        boardCenterPosition = boardRect.center;
		_boardSize.x = LengthX;
		_boardSize.y = LengthY;
		OnBoardSizeChanged?.Invoke(LengthX, LengthY);
		CameraManager.ClaimCameraSetting(boardRect);
		CameraManager.ClaimCameraReset();
	}

    public void ResetTileSet()
    {
        if (tiles is null) return;
        foreach (TileBase currentTile in tiles)
        {
            if (!currentTile) continue;
            currentTile.ResetAll();
            ObjectManager.DestroyObject(currentTile.gameObject);
        }
        tiles = null;
    }

    public void ResetTilePosition(bool value = true)
    {
        if (!tileOffsetTransform) return;
        tileOffsetTransform.position = boardCenterPosition;
        OnTileOffsetChanged?.Invoke(boardCenterPosition);
    }

    public static void RemoveTile(in Vector3Int wantLocation)
    {
        TileBase targetTile = GetTile(wantLocation);
        if (!targetTile) return;
        CharacterBase targetCharacter = targetTile.Info.characterOnTile;
        GameObject targetObject = targetTile.Info.objectOnTile;
        if(targetCharacter)
        {
            targetCharacter.MouseHoverExit();
            BattleManager.RemoveCharacterOnBattle(targetCharacter);
        }
        else if(targetObject)
        {
            ObjectManager.DestroyObject(targetObject);
        }
        targetTile.UnsetObject();
        tiles[wantLocation.x, wantLocation.y] = null;
        ObjectManager.DestroyObject(targetTile.gameObject);
    }

    public TileBase CreateTile(in TileInfo wantInfo)
	{
        int currentX = wantInfo.location.x;
        int currentY = wantInfo.location.y;
        if (!tiles.IsValidRange(currentX, currentY)) return null;
        if (tiles[currentX, currentY])
        {
            Debug.LogError($"Fail to Create Tile : ({currentX},{currentY}) Already Exist");
            return null;
        }

        TileBase result = null;
		GameObject instance = ObjectManager.CreateObject("Tile", tileOffsetTransform);
		if (instance)
		{
			result = instance.GetComponent<TileBase>();
			result.Set(wantInfo);
		}
		if (result)
		{
			tiles[wantInfo.location.x, wantInfo.location.y] = result;
		}
		return result;
	}

	public static void CreateTileWithBoardCalculation(in TileInfo data)
	{
		Vector3Int location = data.location;
		TileManager currentManager = GameManager.Tile;
		if (!currentManager) return;
		currentManager.BoardExpand(location);
		currentManager.CreateTile(in data);
	}

	public void BoardExpand(in Vector3Int newLocationLimit)
	{
		if (tiles is null) return;
		if (newLocationLimit.x < 0 || newLocationLimit.y < 0) return;
		int originLengthX = tiles.GetLength(0);
        int originLengthY = tiles.GetLength(1);
		int newLengthX = newLocationLimit.x + 1;
		int newLengthY = newLocationLimit.y + 1;
		if (newLengthX < originLengthX && newLengthY < originLengthY) return;
		newLengthX = Mathf.Max(newLengthX, originLengthX);
		newLengthY = Mathf.Max(newLengthY, originLengthY);

		TileBase[,] newTiles = new TileBase[newLengthX, newLengthY];
		for (int i = 0; i < originLengthX; i++)
		{
			for (int j = 0; j < originLengthY; j++)
			{
				newTiles[i, j] = tiles[i, j];
			}
		}
		tiles = newTiles;
		_boardSize.x = newLengthX;
		_boardSize.y = newLengthY;
		BoardExpandedSizeCheck(newLocationLimit);
	}

	public void BoardExpandedSizeCheck(in Vector3Int newLocation)
	{
		Vector3 worldLocation = GetTileWorldPosition(newLocation);
		float tileHalfSizeX = tileSize.x * 0.5f;
		float tileHalfSizeY = tileSize.y * 0.5f;
		Rect originRect = boardRect;
		boardRect.xMin = Mathf.Min(boardRect.xMin, worldLocation.x - tileHalfSizeX - boardPadding_LR.x);
		boardRect.yMin = Mathf.Min(boardRect.yMin, worldLocation.y - tileHalfSizeY - boardPadding_UD.y);
		boardRect.xMax = Mathf.Max(boardRect.xMax, worldLocation.x + tileHalfSizeX + boardPadding_LR.y);
		boardRect.yMax = Mathf.Max(boardRect.yMax, worldLocation.y + tileHalfSizeY + boardPadding_UD.x);

		OnBoardSizeChanged?.Invoke(_boardSize.x, _boardSize.y);
		CameraManager.ClaimCameraSetting(boardRect);
		CameraManager.ClaimCalculateCameraBound();
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

	public static bool RemoveObjectOnTile(Vector3Int wantPosition)
    {
        bool result = TryGetTile(wantPosition, out TileBase lastTile);
        if (result) lastTile.UnsetObject();
        return result;
    }

    public static bool RemoveObjectOnTile(GameObject target, Vector3Int wantPosition)
    {
        if (!target) return false;
        bool result = TryGetTile(wantPosition, out TileBase lastTile) && lastTile.Info.objectOnTile == target;
        if (result) lastTile.UnsetObject();
        return result;
    }

    public static bool PlaceObjectOnTile(GameObject target, Vector3Int wantPosition)
	{
        if (!TryGetTile(wantPosition, out TileBase targetTile)) return false;

        if (target.TryGetComponent(out ITilePlaceable asPlaceableObject))
		{
			if (TryGetTile(asPlaceableObject.CurrentTilePosition, out TileBase lastTile)) lastTile.UnsetObject();
		}
		return targetTile.SetObject(target);
	}

    public static bool PlaceObjectOnTile(GameObject target, Vector3Int wantPosition, Vector3Int originPosition)
    {
        if (!TryGetTile(wantPosition, out TileBase targetTile)) return false;

        if (target.TryGetComponent(out ITilePlaceable asPlaceableObject))
        {
            if (TryGetTile(originPosition, out TileBase lastTile)) lastTile.UnsetObject();
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

	public static IEnumerable<string> GetBasementNames(bool needEmptySlot)
	{
		if (tileBasementDictionary is null) yield break;
		if(needEmptySlot) yield return null;
		foreach (string currentValue in tileBasementDictionary.Keys.ToArray()) yield return currentValue;
	}
	public static IEnumerable<string> GetDecorationNames(bool needEmptySlot)
	{
		if (tileBasementDictionary is null) yield break;
		if(needEmptySlot) yield return null;
		foreach (string currentValue in tileDecorationDictionary?.Keys.ToArray()) yield return currentValue;
	}
    public static IEnumerable<string> GetWallBasementNames(bool needEmptySlot)
    {
        if (wallBasementDictionary is null) yield break;
        if (needEmptySlot) yield return null;
        foreach (string currentValue in wallBasementDictionary.Keys.ToArray()) yield return currentValue;
    }
    public static IEnumerable<string> GetWallDecorationNames(bool needEmptySlot)
    {
        if (wallBasementDictionary is null) yield break;
        if (needEmptySlot) yield return null;
        foreach (string currentValue in wallDecorationDictionary?.Keys.ToArray()) yield return currentValue;
    }

    public static TileBasement GetBasement(string basement)
	{
		if (tileBasementDictionary is null || string.IsNullOrEmpty(basement)) return defaultBasement;
		if (tileBasementDictionary.TryGetValue(basement, out TileBasement result)) return result;
		return defaultBasement;
	}

	public static TileDecoration GetDecoration(string decoration)
	{
		if (tileDecorationDictionary is null || string.IsNullOrEmpty(decoration)) return defaultDecoration;
		if (tileDecorationDictionary.TryGetValue(decoration, out TileDecoration result)) return result;
		return defaultDecoration;
	}

	public static WallBasement GetWallBasement(string wall)
	{
		if (wallBasementDictionary is null || string.IsNullOrEmpty(wall)) return defaultWallBasement;
		if (wallBasementDictionary.TryGetValue(wall, out WallBasement result)) return result;
		return defaultWallBasement;
	}

	public static WallDecoration GetWallDecoration(string decoration)
	{
		if (wallDecorationDictionary is null || string.IsNullOrEmpty(decoration)) return defaultWallDecoration;
		if (wallDecorationDictionary.TryGetValue(decoration, out WallDecoration result)) return result;
		return defaultWallDecoration;
	}


	public static Vector3Int GetRealBoardSize()
	{
		Vector3Int result = Vector3Int.zero;

		foreach(TileBase currentTile in tiles)
		{
			if (!currentTile) continue;

			result.x = Mathf.Max(result.x, currentTile.Info.location.x + 1);
			result.y = Mathf.Max(result.y, currentTile.Info.location.y + 1);
		}

		return result;
	}

	public static TileBase GetTileFromText(string text)
    {
        if (string.IsNullOrEmpty(text)) return null;
        if (!text.AsAlgebraicChessNotation(out Vector3Int position)) return null;
        if(!TryGetTile(position, out TileBase result)) return null;
        return result;
    }

    public static string GetTileHorizonText(int index) => index.ToAlphabet();
    public static string GetTileHorizonText_Lower(int index) => GetTileHorizonText(index).ToLower();
    public static string GetTileVerticalText(int index) => $"{1 + index}";

    public static string GetTileText(Vector3Int wantTile) => GetTileHorizonText_Lower(wantTile.x) + GetTileVerticalText(wantTile.y);

	public static Vector3Int GetTileCellPosition(Vector3 wantPosition)
	{
		wantPosition -= TileOffsetValue;
		return new Vector3Int(Mathf.RoundToInt(wantPosition.x / tileSize.x), Mathf.RoundToInt(wantPosition.y / tileSize.y));
	}

	public static Vector3 GetTileWorldPosition(in Vector3Int wantTile) => new (GetTileWorldPositionX(wantTile), GetTileWorldPositionY(wantTile));

    public static float GetTileWorldPositionX(in Vector3Int wantTile) => wantTile.x * tileSize.x + TileOffsetValue.x;
    public static float GetTileWorldPositionY(in Vector3Int wantTile) => wantTile.y * tileSize.y + TileOffsetValue.y;

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

    public static GameObject GetObjectOnTile(in string algebraicNotation)
    {
        if (algebraicNotation.AsAlgebraicChessNotation(out Vector3Int position)) return GetObjectOnTile(position);
        return null;
    }

    public static GameObject GetObjectOnTile(in Vector3Int wantTile)
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
			case MoveCheckType.Range:
				exception &= ~TileEnterException.Block_High;
				break;
			case MoveCheckType.Through:
			case MoveCheckType.Jump:
				exception &= ~TileEnterException.Block_Low;
				break;
		}
        return exception != TileEnterException.Possible;
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

    public static bool GetTileEnterable(in Vector3Int targetTile, in Vector3Int direction, out TileEnterException exception)
    {
        if (TryGetTileInfo(targetTile, out TileInfo targetTileInfo))
        {
            exception = targetTileInfo.EnterCheck();
        }
        else
        {
            exception = TileEnterException.TileNotExist;
        }

        return exception == TileEnterException.Possible;
    }

    public static TileEnterException GetTileEnterable(in TileMoveStruct moveInfo, in TileInfo targetTileInfo) => targetTileInfo.EnterCheck();

    public static CharacterBase GetWaitInputCharacter() => inputWaitTarget;
    public static bool IsWaitInput() => GetWaitInputCharacter() != null;
    public static bool IsAttackable(CharacterBase target)
    {
        if (!IsWaitInput()) return false;
        if (!target) return false;
        if (IsLegalAttack(target.CurrentTilePosition)) return true;
        return false;
    }
    public static bool HasLegalMove(in CharacterBase target) => target != null && target == inputWaitTarget && inputWaitMovePositions is not null && inputWaitMovePositions.Length > 0;
    public static bool IsLegalMove(in Vector3Int position) => inputWaitMovePositions is not null && inputWaitMovePositions.Contains(position);
    public static bool IsLegalMove(in CharacterBase target, in Vector3Int position) => HasLegalMove(target) && IsLegalMove(position);
    public static bool IsIllegalMove(in Vector3Int position) => !IsLegalMove(position);
    public static bool IsIllegalMove(in CharacterBase target, in Vector3Int position) => !IsLegalMove(target, position);
    public static bool HasLegalAttack(in CharacterBase target) => target != null && target == inputWaitTarget && inputWaitAttackPositions is not null && inputWaitAttackPositions.Length > 0;
    public static bool IsLegalAttack(in Vector3Int position) => inputWaitAttackPositions is not null && inputWaitAttackPositions.Contains(position);
    public static bool IsLegalAttack(in CharacterBase target, in Vector3Int position) => HasLegalAttack(target) && IsLegalAttack(position);
    public static bool IsIllegalAttack(in Vector3Int position) => !IsLegalAttack(position);
    public static bool IsIllegalAttack(in CharacterBase target, in Vector3Int position) => !IsLegalAttack(target, position);

    public static bool IsDiagonal(in Vector3Int direction) => Mathf.Abs(direction.x) == Mathf.Abs(direction.y);
	public static bool IsStraight(in Vector3Int direction) => direction.x == 0 || direction.y == 0;
	public static bool IsDiagonalOrStraight(in Vector3Int direction) => IsDiagonal(direction) || IsStraight(direction);
	public static bool IsNotDiagonalOrStraight(Vector3Int direction) => !(IsDiagonal(direction) || IsStraight(direction));
	public static int GetDistance(in Vector3Int diff) => Mathf.Max(Mathf.Abs(diff.x), Mathf.Abs(diff.y));
	public static int GetDistance(in Vector3Int start, in Vector3Int end) => GetDistance(end - start);
    public static int GetAttackDamage(CharacterBase target)
    {
        int result = 0;
        if (!IsWaitInput()) return result;
        if (!target) return result;
        if (IsLegalAttack(target.CurrentTilePosition))
        {
            result = inputWaitTarget.GetAttackDamage(target);
        }
        return result;
    }
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
        TileCheckStruct tileChecker = new()
        {
            result = true,
            accepter = new(),
            currentMoveInfo = moveInfo,
        };
        foreach (Vector3Int currentDirection in movementDelta)
        {
            tileChecker.currentMoveInfo.previousTile = current;
            tileChecker.currentMoveInfo.nextTile = tileChecker.currentMoveInfo.previousTile + currentDirection;
            ++tileChecker.currentMoveInfo.moveDistance;
            tileChecker.accepter.Clear();

            if (checker is null) yield return tileChecker.currentMoveInfo.nextTile;
            else
            {
                tileChecker.result = true;
                checker(ref tileChecker);
                if (tileChecker.result)   yield return tileChecker.currentMoveInfo.nextTile;
                if (tileChecker.isStop)   yield break;
            }
            current = tileChecker.currentMoveInfo.nextTile;
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


	public static IEnumerable<Vector3Int> GetAvailableTilesOnMainDiagonal(Vector3Int start, TileMoveStruct moveInfo, TileEnterCheck checker)  => GetAvailableTilesOnDirections(start, moveInfo, checker, diagonal_LD, diagonal_RU);
	public static IEnumerable<Vector3Int> GetAvailableTilesOnAntiDiagonal(Vector3Int start, TileMoveStruct moveInfo, TileEnterCheck checker)  => GetAvailableTilesOnDirections(start, moveInfo, checker, diagonal_LU, diagonal_RD);
	public static IEnumerable<Vector3Int> GetAvailableTilesOnDiagonals(Vector3Int start, TileMoveStruct moveInfo, TileEnterCheck checker)     => GetAvailableTilesOnDirections(start, moveInfo, checker, diagonal_LD, diagonal_LU, diagonal_RD, diagonal_RU);
	public static IEnumerable<Vector3Int> GetAvailableTilesOnVertical(Vector3Int start, TileMoveStruct moveInfo, TileEnterCheck checker)      => GetAvailableTilesOnDirections(start, moveInfo, checker, Vector3Int.up, Vector3Int.down);
	public static IEnumerable<Vector3Int> GetAvailableTilesOnHorizontal(Vector3Int start, TileMoveStruct moveInfo, TileEnterCheck checker)    => GetAvailableTilesOnDirections(start, moveInfo, checker, Vector3Int.left, Vector3Int.right);
	public static IEnumerable<Vector3Int> GetAvailableTilesOnCross(Vector3Int start, TileMoveStruct moveInfo, TileEnterCheck checker)         => GetAvailableTilesOnDirections(start, moveInfo, checker, Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right);
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
