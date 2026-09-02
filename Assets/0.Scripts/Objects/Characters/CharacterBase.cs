using System.Collections.Generic;
using UnityEngine;

public struct PossibleActionInfo
{
	public Vector3Int location;
	public TileBase targetTile;
	public CharacterModule from;
	public string tag;

	public PossibleActionInfo(CharacterModule claimer, Vector3Int wantLocation, string wantTag)
	{
		from = claimer;
		location = wantLocation;
		tag = wantTag;
		targetTile = TileManager.GetTile(wantLocation);
	}
	public override readonly string ToString() => $"{from} : {tag} to {location}";
	public static implicit operator Vector3Int(PossibleActionInfo info) => info.location;
	public static implicit operator TileBase(PossibleActionInfo info) => info.targetTile;
}

public delegate IEnumerable<PossibleActionInfo> PossibleActionCheckEvent();
public delegate void HoverEvent(bool isHovered);
public delegate void SelectEvent(bool isSelected, ControllerBase from);

public delegate void MovementEvent(Vector3 move);
public delegate void LookAtEvent(Vector3 direction);
public delegate void OutEvent(bool isOuted);
public delegate void AnimationTriggertEvent(AnimationTriggerType wantType);
public delegate void DamageEvent(in DamageStruct info);
public delegate void RestoreEvent(in RestoreStruct info);
public delegate void NameChangeEvent(in string newName);

public partial class CharacterBase : MonoBehaviour, ISelectable, IFunctionable, ITilePlaceable, ISavable<CharacterSaveData>, IIdentificatable
{
    public event HoverEvent OnHovered;
    public event OutEvent OnOuted;
    public event SelectEvent OnSelected;

	public event PossibleActionCheckEvent OnPossibleActionCheck;

	public event MovementEvent OnMovement;
    public void MovementNotify(Vector3 move) => OnMovement?.Invoke(move);

    public event LookAtEvent OnLookAt;
    public void LookAtNotify(Vector3 direction) => OnLookAt?.Invoke(direction);

    public event DamageEvent OnDamage;
    public void DamageNotify(in DamageStruct info) => OnDamage?.Invoke(info);

    public event RestoreEvent OnRestore;
    public void RestoreNotify(in RestoreStruct info) => OnRestore?.Invoke(info);

    public event AnimationTriggertEvent OnAnimationTrigger;
    public void AnimationTriggerNotify(in AnimationTriggerType wantType) => OnAnimationTrigger?.Invoke(wantType);

    public event NameChangeEvent OnNameChanged;

    ControllerBase _controller;
    public ControllerBase Controller => _controller;

    protected Vector3 _lookRotation;
    public Vector3 LookRotation => _lookRotation;

    [SerializeField] string _displayInitial;
    public string DisplayInitial => _displayInitial;

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

    readonly Dictionary<System.Type, CharacterModule> moduleDictionary = new();

	CharacterPreset currentPreset;


	Vector3Int _oppositeDirection = Vector3Int.up;
    public Vector3Int OppositeDirection { get => _oppositeDirection; set => _oppositeDirection = value; }

    public readonly static Vector3Int missingTilePosition = Vector3Int.one * -1024;
	protected Vector3Int _currentTilePosition = missingTilePosition;

	public Vector3Int CurrentTilePosition { get => _currentTilePosition; set => _currentTilePosition = value; }

    protected Vector3Int? _startTilePosition;
    public Vector3Int? StartTilePosition { get => _startTilePosition; set => _startTilePosition = value; }

    [SerializeField] protected int baseDamage = 3;

	protected int id = -1;

    protected bool _isPawn;
    public bool IsPawn => _isPawn;

    public bool IsAlive
    {
        get
        {
            if (TryGetModule(out HitPointModule hpModule)) return hpModule.IsAlive;
            else return true;
        }
    }

    public bool IsDamaged
    {
        get
        {
            if (TryGetModule(out HitPointModule hpModule)) return hpModule.IsDamaged;
            else return true;
        }
    }

    public CharacterSaveData MakeSaveData() => new()
    {
		selfID = GetID(),
		controllerID = Controller ? Controller.GetID() : -1,
		masterID = MasterCharacter ? MasterCharacter.GetID() : -1,
		pawnIDList = Pawns.MakeCharacterIDArray(),
        isPawn = IsPawn,
        presetName = currentPreset.name,
        saveDataList = this.MakeCustomSaveData(),
        startPosition = StartTilePosition ?? CurrentTilePosition,
    };

    public void LoadData(in CharacterSaveData data)
    {
		ResetAll();
        _isPawn         = data.isPawn;
		_startTilePosition = data.startPosition;
		SetPreset(data.presetName);
		id				= data.selfID;
		ControllerBase ownerController = BattleManager.GetControllerFromID(data.controllerID);
		if (ownerController) ownerController.Possess(this);
		if(data.masterID >= 0) SetMaster(BattleManager.GetCharacterFromID(data.masterID));
		if(data.pawnIDList is not null)
		{
			foreach (int currentPawnID in data.pawnIDList)
			{
				CharacterBase currentPawn = BattleManager.GetCharacterFromID(currentPawnID);
				if (currentPawn) currentPawn.SetMaster(this);
			}
		}
		TileManager.PlaceObjectOnTile(gameObject, data.startPosition);
    }

	public void ResetAll()
	{
		_startTilePosition = null;
		CurrentTilePosition = missingTilePosition;
		_isPawn = false;
		id = -1;
		Unpossessed();
		UnsetMaster();
		Pawns.Clear();
	}

    public void ConstructCustomSaveData(Dictionary<string, string> result) 
    { 
        foreach(ISavable current in GetModules<ISavable>()) current.ConstructCustomSaveData(result);
    }

    public void RegistrationFunctions()
	{
		AddAllModuleFromObject(gameObject);
	}

	public void UnregistrationFunctions()
	{
		RemoveAllModule();
	}

	public void SetPreset(string wantPresetName)
	{
		SetPreset(DataManager.LoadDataFile<CharacterPreset>(wantPresetName));
	}

	void SetPreset(CharacterPreset newPreset)
	{
		currentPreset = newPreset;
		if(currentPreset) ApplySetting(currentPreset.GetSetting(IsPawn));
	}

	void ApplySetting(in CharacterBaseSetting setting)
	{
		DisplayName = setting.displayName;
		_displayInitial = setting.initial;
		foreach(CharacterModule currentModule in GetModules())
		{
			currentModule.ApplySetting(setting);
		}
	}

	public IEnumerable<PossibleActionInfo> GetPossibleActions()
	{
		if(OnPossibleActionCheck is not null)
		{
			foreach (PossibleActionInfo currentAction in OnPossibleActionCheck.Invoke())
			{
				yield return currentAction;
			}
		}
	}

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
			CharacterModule targetModule = moduleDictionary[wantType];
			if (!targetModule) return;
			targetModule.OnUnregistration(this);
			moduleDictionary.Remove(wantType);
		}
	}
	public void RemoveAllModule()
	{
		foreach (CharacterModule currentModule in moduleDictionary.Values) currentModule.OnUnregistration(this);
		moduleDictionary.Clear();
	}
	public GameObject GetHoveredObject() => gameObject;

	public Sprite GetIcon() => currentPreset ? currentPreset.GetSetting(IsPawn).icon : null;

	public IEnumerable<CharacterModule> GetModules()
	{
		if (moduleDictionary is null) yield break;
		foreach (CharacterModule current in moduleDictionary.Values) yield return current;
	}

	public IEnumerable<T> GetModules<T>()
    {
        if(moduleDictionary is null) yield break;
        foreach (CharacterModule current in moduleDictionary.Values)
        {
            if (current is T asT) yield return asT;
        }
    }

    public bool TryGetModule<T>(out T result) where T : CharacterModule
    {
        moduleDictionary.TryGetValue(typeof(T), out CharacterModule finder);
        result = finder as T;
        return result;
    }

    public T GetModule<T>() where T : CharacterModule
	{
		moduleDictionary.TryGetValue(typeof(T), out CharacterModule result);
		return result as T;
	}

	public int SetID(int newID) => id = newID; 
    public int GetID() => id;

	protected virtual void OnPossessed(ControllerBase newController) { }
	public ControllerBase Possessed(ControllerBase from)
	{
		if (Controller) Unpossessed();
		_controller = from;
		OnPossessed(Controller);
		foreach (CharacterModule currentModule in GetModules()) currentModule.OnPossessed(Controller);
		return Controller;
	}

	protected virtual void OnUnpossessed(ControllerBase oldController){}
	public void Unpossessed()
	{
		if (!Controller) return;

		OnUnpossessed(Controller);
		_controller = null;
		foreach (CharacterModule currentModule in GetModules()) currentModule.OnPossessed(null);
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
        if (!IsAlive) return false;
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

	public bool PlaceOnTile(in TileInfo newInfo, TileBase newTile)
	{
		CurrentTileBase = newTile;
		CurrentTilePosition = newInfo.location;
        StartTilePosition ??= newInfo.location;
		return true;
	}

	public bool RemoveFromTile(in TileInfo oldInfo, TileBase oldTile)
	{
		CurrentTileBase = null;
		CurrentTilePosition = missingTilePosition;
		return true;
	}

    public void SetMaster(CharacterBase target)
    {
        if (!target) return;
		if (_masterCharacter == target) return;
		if(_masterCharacter) UnsetMaster();
        _masterCharacter = target;
        OppositeDirection = MasterCharacter.OppositeDirection;
        _masterCharacter.Pawns.Add(this);
    }

	public void UnsetMaster()
	{
		if (!_masterCharacter) return;
		OppositeDirection = Vector3Int.up;
		_masterCharacter.Pawns.Remove(this);
		_masterCharacter = null;
	}

    public GameObject SpawnPawn(ControllerBase TargetController)
    {
        GameObject Result = ObjectManager.CreateObject("CharacterBase");

        if (!Result) return Result;
        if (Result.TryGetComponent(out CharacterBase spawnedCharacter))
        {
			spawnedCharacter._isPawn = true;
			spawnedCharacter.SetPreset(currentPreset);
            spawnedCharacter.SetMaster(this);
			TargetController.Possess(spawnedCharacter);
            TileManager.PlaceObjectOnTile(Result, CurrentTilePosition + OppositeDirection);
        }
        return Result;
    }

    public void ResetPosition()
    {
        if (!CurrentTileBase) return;
        CurrentTileBase.SetObject(gameObject);
    }

    public void AnimationReset()
    {
        AnimationTriggerNotify(AnimationTriggerType.Reset);
    }

    public void VisualizeOut()
    {
        gameObject.SetActive(false);
        
        OnOuted?.Invoke(true);
    }

    public void UnVisualizeOut(Vector3Int returnLocation)
    {
        gameObject.SetActive(true);
        
        OnOuted?.Invoke(false);
    }

    public int GetAttackDamage(CharacterBase target)
    {
        return baseDamage;
    }
}
