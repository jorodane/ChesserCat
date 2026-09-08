using UnityEngine;
using UnityEngine.UI;

public class UI_ObjectFollowUI : OpenableUIBase
{
	[SerializeField] protected Vector2 shiftedPosition;
	[SerializeField] protected Vector3 shiftedWorldPosition;

	protected Transform _target;
	public Transform Target => _target;

	protected virtual void OnEnable()
	{
		GameManager.OnUpdateUI -= MoveToTarget;
		GameManager.OnUpdateUI += MoveToTarget;
	}

	protected virtual void OnDisable()
	{
		GameManager.OnUpdateUI -= MoveToTarget;
	}

	public override void Registration(UIManager manager)
	{
		base.Registration(manager);

	}

	public override void Unregistration(UIManager manager)
	{
		base.Unregistration(manager);
		UnsetObject();
	}

	public void SetAsLastSibiling()
	{
		transform.SetAsLastSibling();
	}

	public virtual void OpenWithObject(GameObject newObject)
	{
		if (!newObject) { Close(false); return; }
		SetObject(newObject);
		LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
	}

	public override void Close(bool isActiveByKey)
	{
		if (!IsOpen) return;
		base.Close(isActiveByKey);
	}

	public void SetObject(GameObject newObject)
	{
		if (Target) UnsetObject();
		_target = newObject.transform;
		if (!Target) return;
		OnSetObject(newObject);
	}

	protected virtual void OnSetObject(GameObject newObject){}

	public void UnsetObject()
	{
		if(!Target) return;
		OnUnsetObject(Target.gameObject);
		_target = null;
	}
	protected virtual void OnUnsetObject(GameObject oldObject){}

	protected void MoveToTarget(float deltaTime)
	{
		if (!Target) return;
		transform.position = Camera.main.WorldToScreenPoint(Target.position + shiftedWorldPosition) + (Vector3)shiftedPosition;
	}

	protected void MoveToMouse(Vector2 screenPosition, Vector3 worldPosition)
	{
		transform.position = screenPosition + shiftedPosition;
	}
}