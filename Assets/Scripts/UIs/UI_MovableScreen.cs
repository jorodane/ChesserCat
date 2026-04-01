using UnityEngine;

public class UI_MovableScreen : UIBase
{
	Vector3 popupPosition = Vector3.zero;
	Vector3 popupShift = new(20.0f, -20.0f);

	protected override GameObject OnSetChild(GameObject newChild)
	{
		newChild.transform.localPosition = popupPosition;
		popupPosition += popupShift;
		return base.OnSetChild(newChild);
	}

	protected override void OnUnsetChild(GameObject oldChild)
	{
		base.OnUnsetChild(oldChild);
	}
}
