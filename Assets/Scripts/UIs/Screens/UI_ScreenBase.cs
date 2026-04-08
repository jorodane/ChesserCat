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

public class UI_ScreenBase : UIBase
{
	[SerializeField] UIClaim[] requiredUI;

	public override void Registration(UIManager manager)
	{
		base.Registration(manager);
		foreach (UIClaim currentClaim in requiredUI)
		{
			currentClaim.Execute();
		}
	}
}
