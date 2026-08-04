using System;
using UnityEngine;
using UnityEngine.UI;

public class UI_CharacterHoverInfo : OpenableUIBase
{
	[SerializeField] Vector2 shiftedPosition;

	[SerializeField] Vector2 detailedOffset;
	[SerializeField] Vector2 simplifiedOffset;

    [SerializeField] float detailedHPBarSize;
    [SerializeField] float SimplifiedHPBarSize;

	[SerializeField] UI_HPBar hpBar;
	[SerializeField] UI_TargetNameTag nameTag;
    [SerializeField] GameObject arrow;
	
	CharacterBase target;

    bool isSimplified = false;

    public override void Registration(UIManager manager)
    {
        base.Registration(manager);
        GameManager.OnUpdateUI -= MoveToTarget;
        GameManager.OnUpdateUI += MoveToTarget; 
        InputManager.OnMouseHover -= HoverInfoChange;
        InputManager.OnMouseHover += HoverInfoChange;

    }

    public override void Unregistration(UIManager manager)
    {
        base.Unregistration(manager);
        UnSetCharacter();
        GameManager.OnUpdateUI -= MoveToTarget;
        InputManager.OnMouseHover -= HoverInfoChange;
    }

    public bool SameAsClickInfo(CharacterBase targetCharacter)
	{
		return UIManager.ClaimCheckOpen(UIType.CharacterClickInfo, out IOpenable ClickInfo) && ClickInfo is ICharacterConnectable asCharacterConnector && asCharacterConnector.ConnectedCharacter == targetCharacter;
	}

    public bool HasCharacter() => target;

	public void OpenWithCharacter(CharacterBase asCharacter, bool isSimple)
	{
        if (!asCharacter) { Close(false); return; }
        SetCharacter(asCharacter);
        SetSimple(isSimple);
        transform.position = Camera.main.WorldToScreenPoint(target.transform.position) + (Vector3)shiftedPosition;
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
    }

    public override void Close(bool isActiveByKey)
    {
        if (!IsOpen) return;
        base.Close(isActiveByKey);
    }

    public void SetCharacter(CharacterBase asCharacter)
    {
        if(HasCharacter()) UnSetCharacter();
        target = asCharacter;
        if (!target) return;
        target.OnOuted -= OnCharacterOut;
        target.OnOuted += OnCharacterOut;
        hpBar.Connect(asCharacter);
        nameTag.Connect(asCharacter);
        OnCharacterOut(!target.IsAlive);
    }

    public void UnSetCharacter()
    {
        if (!HasCharacter()) return;
        target.OnOuted -= OnCharacterOut;
        hpBar.Disconnect(target);
        nameTag.Disconnect(target);
        target = null;

    }

    public void SetSimple(bool value)
    {
        isSimplified = value;
        if (isSimplified)
        {
            shiftedPosition = simplifiedOffset;
            arrow.SetActive(false);
            ShowName(InputManager.CursorHoverObject == target.gameObject);
            SetHPBarSize(SimplifiedHPBarSize);
        }
        else
        {
            shiftedPosition = detailedOffset;
            arrow.SetActive(true);
            ShowName(true);
            SetHPBarSize(detailedHPBarSize);
        }
    }

    public void ShowName(bool value)
    {
        nameTag.gameObject.SetActive(value);
    }

    public void SetHPBarSize(float value)
    {
        if (hpBar.transform is RectTransform hpRect)
        {
            Vector2 newSize = hpRect.sizeDelta;
            newSize.x = value;
            hpRect.sizeDelta = newSize;
        }
    }

    void HoverInfoChange(GameObject newTarget, GameObject oldTarget)
    {
        if (!HasCharacter()) return;
        if (isSimplified) ShowName(newTarget == target.gameObject);
    }

    void OnCharacterOut(bool isOuted)
    {
        gameObject.SetActive(!isOuted);
    }

    void MoveToTarget(float deltaTime)
    {
        if (!HasCharacter()) return;
        transform.position = Camera.main.WorldToScreenPoint(target.transform.position) + (Vector3)shiftedPosition;
    }

    void MoveToMouse(Vector2 screenPosition, Vector3 worldPosition)
	{
		transform.position = screenPosition + shiftedPosition;
	}
}