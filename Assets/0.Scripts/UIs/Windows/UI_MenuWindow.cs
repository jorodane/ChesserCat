using UnityEngine;

public class UI_MenuWindow : OpenableUIBase
{
	public override void Open(bool isActiveByKey)
	{
		base.Open(isActiveByKey);
		GameManager.Pause();
	}

	public override void Close(bool isActiveByKey)
	{
		base.Close(isActiveByKey);
		GameManager.Unpause();
	}
}
