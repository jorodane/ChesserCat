using System;
using UnityEngine;



public class PlayerController : ControllerBase
{

	static PlayerController _instance;
	public static PlayerController Instance => _instance;

    GuideLine dragGuide;

	int lastSelected = -1;

	Vector3Int clickedTilePosition;
	Vector3Int lastHoveredTilePosition;

    [SerializeField] Color legalMoveColor;
    [SerializeField] Color legalAttackColor;
    [SerializeField] Color illegalMoveColor;

    bool isDragSelect = false;

	public override void RegistrationFunctions()
	{
		base.RegistrationFunctions();
        RegistrationInputs();
		if (!Instance) _instance = this;
        GameObject guidelineInstance = ObjectManager.CreateObject("GuideLine");
        if (guidelineInstance)
        {
            dragGuide = guidelineInstance.GetComponent<GuideLine>();
            dragGuide.SetInvisible();
            dragGuide.SetColor(Color.cyan);
        }
    }

    public override void UnregistrationFunctions()
	{
		base.UnregistrationFunctions();
        UnregistrationInputs();
        BattleManager.RemovePlayerOnBattle(this);
    }

    void RegistrationInputs()
    {
        InputManager.OnMouseLeftButton -= SelectUnderCursor;
        InputManager.OnMouseLeftButton += SelectUnderCursor;
        InputManager.OnMouseRightButton -= GuideUnderCursor;
        InputManager.OnMouseRightButton += GuideUnderCursor;
        InputManager.OnMouseMove -= CheckTileUnderCursor;
        InputManager.OnMouseMove += CheckTileUnderCursor;
        InputManager.OnCommandResetGuide -= GuideReset;
        InputManager.OnCommandResetGuide += GuideReset;
        InputManager.OnSelectByNumber -= SelectByNumber;
        InputManager.OnSelectByNumber += SelectByNumber;
        InputManager.OnSelectByCharacter -= SelectByCharacter;
        InputManager.OnSelectByCharacter += SelectByCharacter;
        InputManager.OnSelectNext -= SelectNext;
        InputManager.OnSelectNext += SelectNext;
        InputManager.OnSelectPrev -= SelectPrev;
        InputManager.OnSelectPrev += SelectPrev;
        InputManager.OnCommandMove -= CommandMoveInput;
        InputManager.OnCommandMove += CommandMoveInput;
        InputManager.OnCommandAttack -= CommandAttackInput;
        InputManager.OnCommandAttack += CommandAttackInput;
        InputManager.OnCommandCancel -= CommandCancel;
        InputManager.OnCommandCancel += CommandCancel;
        InputManager.OnCommandInfo -= CommandInfo;
        InputManager.OnCommandInfo += CommandInfo;
    }

    void UnregistrationInputs()
    {
        InputManager.OnMouseLeftButton -= SelectUnderCursor;
        InputManager.OnMouseRightButton -= GuideUnderCursor;
        InputManager.OnMouseMove -= CheckTileUnderCursor;
        InputManager.OnCommandResetGuide -= GuideReset;
        InputManager.OnSelectByNumber -= SelectByNumber;
        InputManager.OnSelectNext -= SelectNext;
        InputManager.OnSelectPrev -= SelectPrev;
        InputManager.OnCommandMove -= CommandMoveInput;
        InputManager.OnCommandAttack -= CommandAttackInput;
        InputManager.OnCommandCancel -= CommandCancel;
        InputManager.OnCommandInfo -= CommandInfo;
    }

    public override void ResetAll()
    {
        Unselect(SelectTarget);
        base.ResetAll();
    }


    private void SelectPrev(bool value)
	{
		if(lastSelected < 0) SelectByNumber(0);
		else SelectByNumber((lastSelected - 1 + Characters.Count) % Characters.Count);
	}

	private void SelectNext(bool value)
	{
		if(lastSelected < 0) SelectByNumber(0);
		else SelectByNumber((lastSelected + 1) % Characters.Count);
	}

	void SelectByNumber(int value)
	{
		if (GameManager.IsPaused) return;
		if (!Characters.IsValidRange(value))
		{
			UnselectCurrentCharacter(true);
		}
		else
		{
            CharacterBase selectTarget = Characters[value];
            
            if (selectTarget && (SelectedCharacter == selectTarget))  selectTarget = selectTarget.Pawns[0];

            Select(selectTarget);
            OpenCharacterClickInfo(selectTarget);
        }
		lastSelected = value;
	}

    private void SelectByCharacter(CharacterBase value)
    {
        if (GameManager.IsPaused) return;
        Select(value);
        lastSelected = -1;
    }

    void CheckTileUnderCursor(Vector2 screenPosition, Vector3 worldPosition)
    {
        Vector3Int tilePosition = TileManager.GetTileCellPosition(worldPosition);
        if(tilePosition != lastHoveredTilePosition)
        {
            SetTileCursor(lastHoveredTilePosition, tilePosition);
            lastHoveredTilePosition = tilePosition;
        }
    }

    void SetTileCursor(Vector3Int lastTile, Vector3Int currentTile)
    {
        if (UIManager.ClaimCheckOpen(UIType.CharacterClickInfo)) return;
        bool isLegalAttack = TileManager.IsLegalAttack(SelectedCharacter, currentTile);
        bool isLegalMove = TileManager.IsLegalMove(SelectedCharacter, currentTile);
        if (SelectedCharacter)
        {
            Vector3Int startTile = SelectedCharacter.CurrentTilePosition;

            if (dragGuide)
            {
                dragGuide.Set(startTile, currentTile);
                Color dragColor = isLegalAttack ? legalAttackColor : isLegalMove ? legalMoveColor : illegalMoveColor;
                dragGuide.SetColor(dragColor);
            }

            if (isLegalAttack)
            {
                BattleManager.ClaimTurnSimulation(TurnActionBuilder.MakeTurnInfo_Attack(this, SelectedCharacter, startTile, currentTile));
            }
            else if (isLegalMove)
            {
                BattleManager.ClaimTurnSimulation(TurnActionBuilder.MakeTurnInfo_Move(this, SelectedCharacter, startTile, currentTile));
            }
            else
            {
                BattleManager.ClaimTurnSimulationReset();
            }
        }
        else if(dragGuide)
        {
            dragGuide.SetInvisible();
        }
    }

    void SelectUnderCursor(bool value, Vector2 screenPosition, Vector3 worldPosition)
	{
        if (InputManager.IsCursorHoverOnUI) return;
        Vector3Int tilePosition = TileManager.GetTileCellPosition(worldPosition);
        if (value)
        {
            if(SelectedCharacter)
            {
                if (SelectedCharacter.CurrentTilePosition == tilePosition)
                {
                    UIManager.ClaimCloseUI(UIType.CharacterClickInfo);
                }
                else if (!UIManager.ClaimCheckOpen(UIType.CharacterClickInfo) && CommandSimulationConfirm())
                {
                    Unselect(SelectedCharacter);
                }
                else if (InputManager.CursorHoverSelectable is not null)
                {
                    Select(InputManager.CursorHoverSelectable);
                    SetDragGuideActivate(true);
                }
            }
            else
            {
                Select(InputManager.CursorHoverSelectable);
                SetDragGuideActivate(true);
            }
            //if(UIManager.ClaimCheckOpen(UIType.CharacterClickInfo))
        }
        else
        {
            if(SelectedCharacter && SelectedCharacter.CurrentTilePosition == tilePosition)
            {
                if(isDragSelect) OpenCharacterClickInfo(SelectedCharacter);
                else Unselect(SelectedCharacter);
            }
            else
            {
                if (!UIManager.ClaimCheckOpen(UIType.CharacterClickInfo)) CommandSimulationConfirm();
                Unselect(SelectedCharacter);
            }
            SetDragGuideActivate(false);
        }
    }

    void SetDragGuideActivate(bool value)
    {
        if (!dragGuide) return;
        isDragSelect = value;
        if (!isDragSelect)
        {
            dragGuide.SetInvisible();
            BattleManager.ClaimTurnSimulationReset();
        }
    }


    void GuideUnderCursor(bool value, Vector2 screenPosition, Vector3 worldPosition)
	{
		if (value)
		{
			clickedTilePosition = TileManager.GetTileCellPosition(worldPosition);
		}
		else 
		{
			TileManager.ClaimCreateGuideLine(clickedTilePosition, TileManager.GetTileCellPosition(worldPosition));
		}
	}

	void GuideReset(bool value)
	{
		TileManager.ClaimResetGuideLine();
	}


	public virtual void CommandInfo(bool value)
	{
		if (!UIManager.ClaimCheckOpen(UIType.CharacterClickInfo)) return;
		UIManager.ClaimCloseUI(UIType.CharacterClickInfo);
	}

	public virtual void CommandAttackInput(bool value)
	{
		if (!UIManager.ClaimCheckOpen(UIType.CharacterClickInfo)) return;
		UIManager.ClaimCloseUI(UIType.CharacterClickInfo);
		TileManager.StartCharacterAttackInput(SelectedCharacter);
	}

	public virtual void CommandMoveInput(bool value)
	{
		if (!UIManager.ClaimCheckOpen(UIType.CharacterClickInfo)) return;
		UIManager.ClaimCloseUI(UIType.CharacterClickInfo);
		TileManager.SetCharacterMoveInput(SelectedCharacter);
	}

    public virtual bool CommandSimulationConfirm()
    {
        return BattleManager.ClaimTurnSimulationConfirm() != TurnResult.Failed;
    }

    public virtual void CommandCancel(bool value)
	{
        TileManager.EndInput();
        SetDragGuideActivate(false);
        if (UIManager.ClaimCheckOpen(UIType.CharacterClickInfo)) Unselect(SelectTarget);
        else OpenCharacterClickInfo(SelectedCharacter);
    }

    protected override void OnSelect(ISelectable newTarget)
	{
		base.OnSelect(newTarget);
        TileManager.SetCharacterInput(SelectedCharacter);
	}

	protected override void OnReselect(ISelectable newTarget)
	{
		base.OnReselect(newTarget);
        SetDragGuideActivate(false);
        TileManager.SetCharacterInput(SelectedCharacter);
        OpenCharacterClickInfo(SelectedCharacter);
    }

	protected override void OnUnselect(ISelectable oldTarget)
	{
		base.OnUnselect(oldTarget);
		TileManager.EndInput();
        SetDragGuideActivate(false);
        UIManager.ClaimCloseUI(UIType.CharacterClickInfo);
	}

    public virtual void OnCommandCanceled()
    {
        if (UIManager.ClaimCheckOpen(UIType.CharacterClickInfo))
        {
            UnselectCurrentCharacter(true);
        }
        else
        {
            ReselectCurrentCharacter(true);
        }
    }

    public override void OpenCharacterClickInfo(CharacterBase target)
    {
        base.OpenCharacterClickInfo(target);
        SetDragGuideActivate(false);
    }


    public void MoveToMousePosition(bool value, Vector2 screenPosition, Vector3 worldPosition)
	{
		if(value) CommandMoveToDestination(worldPosition, 0.0f);
	}

	public void MoveToDirection(Vector2 value)
	{
		CommandMoveToDirection(value);
	}

	public GameObject SpawnPiece(string wantName, Vector3Int wantPosition, Vector3Int wantOpposite)
	{
		GameObject Result = ObjectManager.CreateObject(wantName);

		if (!Result) return Result;
        if (Result.TryGetComponent(out CharacterBase spawnedCharacter))
        {
            Possess(spawnedCharacter);
            TileManager.PlaceObjectOnTile(Result, wantPosition);
            spawnedCharacter.OppositeDirection = wantOpposite;
            spawnedCharacter.SpawnPawn(this);
        }
		return Result;
	}
}
