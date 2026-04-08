using UnityEngine;

public class UI_MenuWindow : UIBase, IOpenable
{
	public bool IsOpen => gameObject.activeSelf;
	public void Open() => gameObject.SetActive(true);
	public void Close() => gameObject.SetActive(false);
	public void Toggle() => gameObject.SetActive(!IsOpen);

}
