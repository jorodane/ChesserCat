using System.Collections.Generic;
using UnityEngine;

public delegate void ControllerPossessEvent(CharacterBase target);
public delegate void ControllerUnPossessEvent(CharacterBase target);

public class ControllerBase : MonoBehaviour, ISavable<ControllerSaveData>, IIdentificatable
{
	public event ControllerPossessEvent OnControllerPossess;
	public event ControllerUnPossessEvent OnControllerUnPossess;

	List<CharacterBase> _characters = new();
    public List<CharacterBase> Characters => _characters;

    ISelectable selectedTarget;
    public ISelectable SelectTarget => selectedTarget;

    public CharacterBase SelectedCharacter => selectedTarget as CharacterBase;

    [SerializeField] string _prefabName;

	public int id;

	Vector3Int _oppositeDirection = Vector3Int.down;

	[SerializeField] protected Color _teamColor = Color.gray;
	public Color TeamColor => _teamColor;


	public ControllerSaveData MakeSaveData() => new()
    {
        saveDataList = this.MakeCustomSaveData(),
        oppositeDirection = _oppositeDirection,
		teamColor = TeamColor,
        prefabName = _prefabName,
    };

    public void LoadData(in ControllerSaveData data)
    {
        ResetAll();
        _oppositeDirection = data.oppositeDirection;
		_teamColor = data.teamColor;
        _prefabName = data.prefabName;
    }

    public virtual void ConstructCustomSaveData(Dictionary<string, string> result) { }

    public virtual void ResetAll()
    {
        DestroyAllCharacters();
    }

    protected virtual void OnPossess(CharacterBase newCharacter) { }
    public void Possess(CharacterBase target)
    {
        if (!target) return; //대상이 없습니다.
                             //        빙의된컨트롤러             빙의   내가 너에게 가겠다
        ControllerBase result = target.Possessed(this);
        //내가 당첨되었어! => 제대로 빙의가 된 거구나!
        if (result == this)
        {
            _characters.Add(target);
            OnPossess(target);
			OnControllerPossess?.Invoke(target);
		}
	}

    protected virtual void OnUnpossess(CharacterBase oldCharacter) { }
    public void Unpossess(CharacterBase target)
    {
        _characters.Remove(target);
        if (target.Controller == this)
        {
            target.Unpossessed();
            OnUnpossess(target);
			OnControllerUnPossess?.Invoke(target);
        }
    }

    public void DestroyCharacter(CharacterBase target)
    {
        if (!target) return;
        Unpossess(target);
        ObjectManager.DestroyObject(target.gameObject);
    }

    public void DestroyAllCharacters()
    {
        if (_characters == null) return;
        foreach (CharacterBase currentCharacter in _characters.ToArray()) DestroyCharacter(currentCharacter);
        _characters.Clear();
    }

	public virtual bool TurnRequested() => false;

    protected virtual void OnSelect(ISelectable newTarget) 
    { 
        BattleManager.ClaimCompletePlayTurn();
    }
    protected virtual void OnReselect(ISelectable newTarget) 
    { 
        BattleManager.ClaimCompletePlayTurn();
    }

    public void Select(ISelectable target)
    {
        if (selectedTarget == target)
        {
			if(selectedTarget is not null) OnReselect(target);
			return;
        }
        else if (selectedTarget is not null) Unselect(selectedTarget);
        if (target is null) return;
        if (target.Select(this))
        {
            selectedTarget = target;
            OnSelect(target);
        }
    }

    protected virtual void OnUnselect(ISelectable oldTarget) { }
    public void Unselect(ISelectable oldTarget)
    {
        if (selectedTarget is null) return;
        selectedTarget.Unselect(this);
        selectedTarget = null;
        OnUnselect(oldTarget);
    }
    public void UnselectCurrentCharacter(bool value)
    {
        if (!value) return;
        Unselect(SelectTarget);
    }

    public void ReselectCurrentCharacter(bool value)
    {
        if (!value) return;
        Select(SelectTarget);
    }

    public virtual void OpenCharacterClickInfo(CharacterBase target)
    {
        if (target)
        {
            if (!UIManager.ClaimCheckOpen(UIType.CharacterClickInfo, out IOpenable clickUI))
            {
                UIManager.ClaimOpenUI(UIType.CharacterClickInfo);
                if (clickUI is ICharacterConnectable asCharacterConnector) asCharacterConnector.Connect(target);
                if (clickUI is IControllerConnectable asControllerConnector) asControllerConnector.Connect(this);
            }
        }
    }
	public int GetID() => id;
	public virtual int SetID(int value) => id = value;

	public IEnumerable<CharacterBase> GetAllCharacters()
    {
        foreach (CharacterBase current in Characters) yield return current;
    }

    public bool CommandMoveToTile(Vector3Int destination)
    {
        if(TileManager.IsLegalMove(SelectedCharacter, destination))
        {
            //BattleManager.ClaimMove(this, SelectedCharacter, destination);
            return true;
        }
        return false;
    }

    public bool CommandAttackToTile(Vector3Int destination)
    {
        if (TileManager.IsLegalAttack(SelectedCharacter, destination))
        {
            //BattleManager.ClaimAttack(this, SelectedCharacter, destination);
            return true;
        }
        return false;
    }

    public void CommandMoveToDirection(Vector3 direction)
    {
        if (SelectedCharacter && SelectedCharacter.GetModule<MovementModule>() is IRunnable target) target.MoveToDirection(direction);
    }

    public void CommandMoveToDestination(Vector3 destination, float tolerance)
    {
        if (SelectedCharacter && SelectedCharacter.GetModule<MovementModule>() is IRunnable target) target.MoveToDestination(destination, tolerance);
    }

    public void CommandStop()
    {
        if (SelectedCharacter && SelectedCharacter.GetModule<MovementModule>() is IRunnable target) target.StopMovement();
    }
}