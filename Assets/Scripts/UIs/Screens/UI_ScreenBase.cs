using System;
using UnityEngine;

[Serializable]
public struct UIClaim
{
	public string prefabName;
	public UIType uiType;
	public bool	  isOpen;

	public UIBase Execute()
	{
		//UI 만들어 줘! => 예외가 있을 수 있음!
		//                이미... 있는데?
		UIBase result = UIManager.ClaimGetUI(uiType);
		//찾은게 없다!                 만들어!
		if (!result) result = UIManager.ClaimCreateUI(uiType, prefabName);
		//만든게 없다!    없네..
		if (!result) return result;
		
		//대상이 오픈 가능한 친구라면
		if(result is IOpenable openTarget)
		{
			if(isOpen) openTarget.Open();
			else openTarget.Close();
		}

		return result;
	}
}

public class UI_ScreenBase : OpenableUIBase
{
	[SerializeField] UIClaim[] requiredUI;
	[SerializeField] protected UIType[] closeWithScreen;

	public override void Registration(UIManager manager)
	{
		base.Registration(manager);
		if (requiredUI is null) return;

		foreach (UIClaim currentClaim in requiredUI)
		{
			currentClaim.Execute();
		}
	}

	public override void Close()
	{
		base.Close();

		if(closeWithScreen != null)
		{
			foreach(UIType currentUI in closeWithScreen) UIManager.ClaimCloseUI(currentUI);
		}
	}

	public virtual bool CloseInnerUI() => UIManager.ClaimCloseUI(closeWithScreen);
}
