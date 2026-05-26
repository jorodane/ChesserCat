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

	//���� �߿��� ���!
	//���� ���� �� ���� �� ���Դ� ��
	[SerializeField] ControllerBase _controller;
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

	protected Vector3Int _currentTilePosition = Vector3Int.one * -1;
	public Vector3Int CurrentTilePosition { get => _currentTilePosition; set => _currentTilePosition = value; }

	protected TileBase _currentTileBase;
	public TileBase CurrentTileBase { get => _currentTileBase; set => _currentTileBase = value; }

	public void RegistrationFunctions()
	{
		AddAllModuleFromObject(gameObject); // ����� ���� �ٿ�����
		if (_controller) _controller.Possess(this); //��Ʈ�ѷ��� ������ �Ϸ� ��
	}

	public void UnregistrationFunctions()
	{
		RemoveAllModule(); //�� ������ ����� �����ϱ�!
	}

	//����� �����س���!
	//List : �߰�/���Ű� ���� <-> �޸� ȿ���� ����, ��ü ��ȯ�� ������
	//           ����                                    ����
	//Array: �߰�/���Ű� ��ư� <-> �޸� ȿ���� ���, ��ü ��ȯ�� ������
	//           ����                                    ����
	Dictionary<System.Type, CharacterModule> moduleDictionary = new();
	// �߰� / ���� / �˻�
	public void AddModule(System.Type wantType, CharacterModule wantModule)
	{
		if(moduleDictionary.TryAdd(wantType, wantModule))
		{//�߰��ϴ� ���� ���������ϱ�
			wantModule.OnRegistration(this); 
			//����ϴ� �͵� �ߵ�!
		}
	}
	public void AddAllModuleFromObject(GameObject target)
	{
		if (!target) return;

		foreach(CharacterModule currentModule in target.GetComponentsInChildren<CharacterModule>())
		{
			//           �� ģ���� ��з� Ÿ��,          �� ģ��
			AddModule(currentModule.RegistrationType, currentModule);
		}
	}
	public void RemoveModule(System.Type wantType) 
	{
		//                      ��� Ÿ���� ������
		if (moduleDictionary.ContainsKey(wantType))
		{
			moduleDictionary[wantType]?.OnUnregistration(this); //�� ���� ������ �ž�
			moduleDictionary.Remove(wantType); //�� ������ �����ϱ�!
		}
	}
	public void RemoveAllModule()
	{
		// A B C D E      A B C D E
		// 0              0
		// B C D E        F A B C D E
		//   1              1
		// B D E          G F A B C D E
		//     2              2
		//�ڷᱸ���� ���� ���� �� �ȿ� �ִ� ���빰�� �ٲٸ� ������ ����!
		foreach (CharacterModule currentModule in moduleDictionary.Values)
		{
			//             ������ �ߴٰ� ���س���
			currentModule.OnUnregistration(this);
		}
		//������ ���� ���ֱ�!
		moduleDictionary.Clear();
	}
	public GameObject GetHoveredObject() => gameObject;
	public T GetModule<T>() where T : CharacterModule
	{
		//������ �Ű������� �־��µ� �Ʒ����� �Ű������� ���� ����
		moduleDictionary.TryGetValue(typeof(T), out CharacterModule result);
		return result as T;
	}

	//                    ���ǵǴ�
	protected virtual void OnPossessed(ControllerBase newController){}
	public ControllerBase Possessed(ControllerBase from)
	{
		//���Ǹ� �Ϸ��� �ߴµ� ���� ��ȥ�� ����־����
		//��ȥ�� �ִ��� ���ǰ� ����
		//��ȥ�� ������ ƨ�ܳ´�!
		if (Controller) Unpossessed();
		_controller = from;
		OnPossessed(Controller); //�� ������ �����Ϸ� ����
		return Controller;
	}
	
	//          ȥ�� ������
	protected virtual void OnUnpossessed(ControllerBase oldController){}
	public void Unpossessed()
	{
		if(Controller) OnUnpossessed(Controller); //���� �����ǰ� ����
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
}
