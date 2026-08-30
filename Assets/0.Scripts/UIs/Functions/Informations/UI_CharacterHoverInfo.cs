using System;
using UnityEngine;
using UnityEngine.UI;

public class UI_CharacterHoverInfo : OpenableUIBase
{
	[SerializeField] Vector2 shiftedPosition;

	[SerializeField] Vector2 detailedOffset;
	[SerializeField] Vector2 simplifiedOffset;

	[SerializeField] UI_HPBar hpBar;
	[SerializeField] UI_TargetNameTag nameTag;
    [SerializeField] GameObject arrow;
	
	CharacterBase _target;
    public CharacterBase Target => _target;

    bool isSimplified = false;

    public override void Registration(UIManager manager)
    {
        base.Registration(manager);
        GameManager.OnUpdateUI -= MoveToTarget;
        GameManager.OnUpdateUI += MoveToTarget; 
        InputManager.OnMouseHover -= HoverInfoChange;
        InputManager.OnMouseHover += HoverInfoChange;

        hpBar.OnAnimated -= SetAsLastSibiling;
        hpBar.OnAnimated += SetAsLastSibiling;
    }

    void SetAsLastSibiling()
    {
        transform.SetAsLastSibling();
    }

    public override void Unregistration(UIManager manager)
    {
        base.Unregistration(manager);
        UnSetCharacter();
        GameManager.OnUpdateUI -= MoveToTarget;
        InputManager.OnMouseHover -= HoverInfoChange;
        hpBar.OnAnimated -= SetAsLastSibiling;
    }

    public bool SameAsClickInfo(CharacterBase targetCharacter)
	{
		return UIManager.ClaimCheckOpen(UIType.CharacterClickInfo, out IOpenable ClickInfo) && ClickInfo is ICharacterConnectable asCharacterConnector && asCharacterConnector.ConnectedCharacter == targetCharacter;
	}

    public bool HasCharacter() => _target && _target.IsAlive;

	public void OpenWithCharacter(CharacterBase asCharacter, bool isSimple)
	{
        if (!asCharacter) { Close(false); return; }
        SetCharacter(asCharacter);
        SetSimple(isSimple);
        //transform.position = Camera.main.WorldToScreenPoint(target.transform.position) + (Vector3)shiftedPosition;
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
    }

    public override void Close(bool isActiveByKey)
    {
        if (!IsOpen) return;
        base.Close(isActiveByKey);
    }

    public void SetCharacter(CharacterBase asCharacter)
    {
        if(Target) UnSetCharacter();
        _target = asCharacter;
        if (!Target) return;
        _target.OnOuted -= OnCharacterOut;
        _target.OnOuted += OnCharacterOut;
        hpBar.Connect(asCharacter);
        nameTag.Connect(asCharacter);
        OnCharacterOut(!_target.IsAlive);
    }

    public void UnSetCharacter()
    {
        CharacterBase origin = _target;
        _target = null;

        if (!origin) return;
        origin.OnOuted -= OnCharacterOut;
        hpBar.Disconnect(origin);
        nameTag.Disconnect(origin);
        OnCharacterOut(true);
    }

    public void SetSimple(bool value)
    {
        isSimplified = value;
        if (isSimplified)
        {
            shiftedPosition = simplifiedOffset;
            arrow.SetActive(false);
            ShowName(InputManager.CursorHoverObject == _target.gameObject);
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
        if (!HasCharacter()) return;
        if (isSimplified) ShowName(newTarget == _target.gameObject);
    }

    void OnCharacterOut(bool isOuted)
    {
        if (!Target) isOuted = true;
        gameObject.SetActive(!isOuted);
    }

    void MoveToTarget(float deltaTime)
    {
        if (!_target) return;
        transform.position = Camera.main.WorldToScreenPoint(_target.transform.position) + (Vector3)shiftedPosition;
    }

    void MoveToMouse(Vector2 screenPosition, Vector3 worldPosition)
	{
		transform.position = screenPosition + shiftedPosition;
	}
}