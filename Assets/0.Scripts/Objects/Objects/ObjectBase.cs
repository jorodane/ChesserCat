using Unity.VisualScripting;
using UnityEngine;

public class ObjectBase : MonoBehaviour, ITilePlaceable, ISavable<ObjectSaveData>
{
    public readonly static Vector3Int missingTilePosition = Vector3Int.one * -1024;

	[SerializeField] protected string _prefabName;

	protected TileBase _currentTileBase;
	public TileBase CurrentTileBase { get => _currentTileBase; set => _currentTileBase = value; }
	protected Vector3Int _currentTilePosition = missingTilePosition;

	public Vector3Int CurrentTilePosition { get => _currentTilePosition; set => _currentTilePosition = value; }

	protected Vector3Int? _startTilePosition;
	public Vector3Int? StartTilePosition { get => _startTilePosition; set => _startTilePosition = value; }

	public virtual void ResetAll()
	{
		_startTilePosition = null;
		CurrentTilePosition = missingTilePosition;
	}

	public virtual void OriginShifted(in Vector3Int shiftAmount)
	{
		CurrentTilePosition += shiftAmount;
		if (StartTilePosition is not null) StartTilePosition += shiftAmount;
	}

	public virtual bool PlaceOnTile(in TileInfo newInfo, TileBase newTile)
	{
		CurrentTileBase = newTile;
		CurrentTilePosition = newInfo.location;
		StartTilePosition ??= newInfo.location;
		return true;
	}

	public virtual bool RemoveFromTile(in TileInfo oldInfo, TileBase oldTile)
	{
		CurrentTileBase = null;
		CurrentTilePosition = missingTilePosition;
		OnRemoveFromTile(oldInfo, oldTile);
		return true;
	}

	public virtual void OnRemoveFromTile(in TileInfo oldInfo, TileBase oldTile)
	{
		ObjectManager.DestroyObject(gameObject);
	}

	public virtual void ResetPosition()
	{
		if (!CurrentTileBase) return;
		CurrentTileBase.SetObject(gameObject);
	}

	public ObjectSaveData MakeSaveData() => new()
	{
		saveDataList = this.MakeCustomSaveData(),
		prefabName = _prefabName,
	};

	public void LoadData(in ObjectSaveData data)
	{
		ResetAll();
		_prefabName = data.prefabName;
	}

	public static ObjectBase SpawnObjectWithData(in ObjectSaveData data)
	{
		if (string.IsNullOrEmpty(data.prefabName)) return null;
		GameObject instance = ObjectManager.CreateObject(data.prefabName);
		ObjectBase result = instance.GetComponent<ObjectBase>();
		result.LoadData(data);
		return result;
	}
}
