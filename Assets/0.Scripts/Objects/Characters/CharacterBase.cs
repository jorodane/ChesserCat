using System.Collections.Generic;
using UnityEngine;

public delegate void HoverEvent(bool isHovered);
public delegate void SelectEvent(bool isSelected, ControllerBase from);

public delegate void MovementEvent(Vector3 move);
public delegate void LookAtEvent(Vector3 direction);
public delegate void DamageEvent(in DamageStruct info);
public delegate void RestoreEvent(in RestoreStruct info);
public delegate void NameChangeEvent(in string newName);

public class CharacterBase : MonoBehaviour, ISelectable, IFunctionable, ITilePlaceable
{
	public event HoverEvent OnHovered;
	public event SelectEvent OnSelected;

	public event MovementEvent	OnMovement;
	public void MovementNotify(Vector3 move) => OnMovement?.Invoke(move);

	public event LookAtEvent	OnLookAt;
	public void LookAtNotify(Vector3 direction) => OnLookAt?.Invoke(direction);

	public event DamageEvent	OnDamage;
	public void DamageNotify(in DamageStruct info) => OnDamage?.Invoke(info);

	public event RestoreEvent	OnRestore;
	public void RestoreNotify(in RestoreStruct info) => OnRestore?.Invoke(info);

	public event NameChangeEvent OnNameChanged;

	ControllerBase _controller;
	public ControllerBase Controller => _controller;

	protected Vector3 _lookRotation;
	public Vector3 LookRotation => _lookRotation;


	[SerializeField] string _displayName;
	public string DisplayName
	{
		get => _displayName;
		set
		{
			_displayName = value;
			OnNameChanged?.Invoke(value);
		}
	}


    protected TileBase _currentTileBase;
    public TileBase CurrentTileBase { get => _currentTileBase; set => _currentTileBase = value; }

    protected CharacterBase _masterCharacter;
    public CharacterBase MasterCharacter => _masterCharacter;

    protected List<CharacterBase> _pawns = new();
    public List<CharacterBase> Pawns => _pawns;

    protected string _pawnPrefabName = "SamplePiece_Pawn";
    public string PawnPrefabName => _pawnPrefabName;

	Vector3Int _oppositeDirection = Vector3Int.up;
    public Vector3Int OppositeDirection { get => _oppositeDirection; set => _oppositeDirection = value; }

    [SerializeField]protected Vector3Int _currentTilePosition = Vector3Int.one * -1;
    public Vector3Int CurrentTilePosition { get => _currentTilePosition; set => _currentTilePosition = value; }

    public void RegistrationFunctions()
	{
		AddAllModuleFromObject(gameObject);
	}

	public void UnregistrationFunctions()
	{
		RemoveAllModule();
	}

	Dictionary<System.Type, CharacterModule> moduleDictionary = new();
	public void AddModule(System.Type wantType, CharacterModule wantModule)
	{
		if(moduleDictionary.TryAdd(wantType, wantModule))
		{
			wantModule.OnRegistration(this); 
		}
	}
	public void AddAllModuleFromObject(GameObject target)
	{
		if (!target) return;

		foreach(CharacterModule currentModule in target.GetComponentsInChildren<CharacterModule>())
		{
			AddModule(currentModule.RegistrationType, currentModule);
		}
	}
	public void RemoveModule(System.Type wantType) 
	{
		if (moduleDictionary.ContainsKey(wantType))
		{
			moduleDictionary[wantType]?.OnUnregistration(this);
			moduleDictionary.Remove(wantType);
		}
	}
	public void RemoveAllModule()
	{
		foreach (CharacterModule currentModule in moduleDictionary.Values)
		{
			currentModule.OnUnregistration(this);
		}
		moduleDictionary.Clear();
	}
	public GameObject GetHoveredObject() => gameObject;
	public T GetModule<T>() where T : CharacterModule
	{
		moduleDictionary.TryGetValue(typeof(T), out CharacterModule result);
		return result as T;
	}

	protected virtual void OnPossessed(ControllerBase newController){}
	public ControllerBase Possessed(ControllerBase from)
	{
		if (Controller) Unpossessed();
		_controller = from;
		OnPossessed(Controller);
		return Controller;
	}

	protected virtual void OnUnpossessed(ControllerBase oldController){}
	public void Unpossessed()
	{
		if(Controller) OnUnpossessed(Controller);
		_controller = null;
	}
	public bool Unpossessed(ControllerBase oldController)
	{
		if (Controller != oldController) return false;
		Unpossessed();
		return true;
	}

	public void MouseHoverEnter()
	{
		OnHovered?.Invoke(true);
	}

	public void MouseHoverExit()
	{
		OnHovered?.Invoke(false);
	}

	public bool Select(ControllerBase from)
	{
		if(Controller != from) return false;
		OnSelected?.Invoke(true, from);
		return true;
	}

	public bool Unselect(ControllerBase from)
	{
		if(Controller != from) return false;
		OnSelected?.Invoke(false, from);
		return true;
	}

	public bool PlaceOnTile(TileInfo newInfo, TileBase newTile)
	{
		CurrentTileBase = newTile;
		CurrentTilePosition = newInfo.position;
		return true;
	}

	public bool RemoveFromTile(TileInfo oldInfo, TileBase oldTile)
	{
		CurrentTileBase = null;
		CurrentTilePosition = Vector3Int.one * -1;
		return true;
	}

    public void SetMaster(CharacterBase target)
    {
        _masterCharacter = target;
        _masterCharacter.Pawns.Add(this);
    }

    public GameObject SpawnPawn(ControllerBase TargetController)
    {
        GameObject Result = ObjectManager.CreateObject(PawnPrefabName);

        if (!Result) return Result;
        if (Result.TryGetComponent(out CharacterBase spawnedCharacter))
        {
            spawnedCharacter.SetMaster(this);
            TargetController.Possess(spawnedCharacter);
            TileManager.PlaceObjectOnTile(Result, CurrentTilePosition + OppositeDirection);
        }
        return Result;
    }
}
