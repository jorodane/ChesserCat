using UnityEngine;

public interface ISelectable
{
	public void MouseHoverEnter();
	public void MouseHoverExit();

	public bool Select(ControllerBase from);
	public bool Unselect(ControllerBase from);

	public GameObject GetHoveredObject();
}
