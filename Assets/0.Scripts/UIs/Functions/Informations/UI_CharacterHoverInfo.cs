using System;
using UnityEngine;

public class UI_CharacterHoverInfo : UI_CharacterFollowUI
{
	[SerializeField] Vector2 detailedOffset;
	[SerializeField] Vector2 simplifiedOffset;

	[SerializeField] UI_HPBar hpBar;
	[SerializeField] UI_TargetNameTag nameTag;
    [SerializeField] GameObject arrow;

    bool isSimplified = false;

    public override void Registration(UIManager manager)
    {
        base.Registration(manager);
		SetHPBarDelta(0);
        InputManager.OnMouseHover -= HoverInfoChange;
        InputManager.OnMouseHover += HoverInfoChange;

        hpBar.OnAnimated -= SetAsLastSibiling;
        hpBar.OnAnimated += SetAsLastSibiling;
    }

    public override void Unregistration(UIManager manager)
    {
        base.Unregistration(manager);
        InputManager.OnMouseHover -= HoverInfoChange;
        hpBar.OnAnimated -= SetAsLastSibiling;
    }

    public bool SameAsClickInfo(CharacterBase targetCharacter)
	{
		return UIManager.ClaimCheckOpen(UIType.CharacterClickInfo, out IOpenable ClickInfo) && ClickInfo is ICharacterConnectable asCharacterConnector && asCharacterConnector.ConnectedCharacter == targetCharacter;
	}

	public void OpenWithCharacter(CharacterBase asCharacter, bool isSimple)
	{
        SetSimple(isSimple);
		base.OpenWithObject(asCharacter.gameObject);
    }

	protected override void OnSetCharacter(CharacterBase asCharacter)
	{
		base.OnSetCharacter(asCharacter);
		hpBar.Connect(asCharacter);
		nameTag.Connect(asCharacter);
	}

	protected override void OnUnSetCharacter(CharacterBase asCharacter)
	{
		base.OnUnSetCharacter(asCharacter);
		hpBar.Disconnect(asCharacter);
		nameTag.Disconnect(asCharacter);
	}

    public void SetSimple(bool value)
    {
        isSimplified = value;
        if (isSimplified)
        {
            shiftedPosition = simplifiedOffset;
            arrow.SetActive(false);
            ShowName(InputManager.CursorHoverObject == TargetCharacter.gameObject);
            hpBar.SetSimple(true);
        }
        else
        {
            shiftedPosition = detailedOffset;
            arrow.SetActive(true);
            ShowName(true);
            hpBar.SetSimple(false);
        }
    }

    public void ShowName(bool value)
    {
		if (hpBar.CurrentHP + hpBar.CurrentDelta <= 0) value = false;
        nameTag.gameObject.SetActive(value);
    }

    public void SetHPBarDelta(int value)
    {
        if (!hpBar) return;
        hpBar.SetDelta(value);
    }

    public void AddHPBarDelta(int value)
    {
        if (!hpBar) return;
        hpBar.AddDelta(value);
    }

    void HoverInfoChange(GameObject newTarget, GameObject oldTarget)
    {
        if (!HasValidCharacter()) return;
        if (isSimplified) ShowName(newTarget == TargetCharacter.gameObject);
    }
}