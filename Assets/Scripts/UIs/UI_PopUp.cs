using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class UI_PopUp : UIBase, 
	ISystemMessagePossible, IConfirmable
{
	[SerializeField] TextMeshProUGUI titleText;
	[SerializeField] TextMeshProUGUI contextText;
	[SerializeField] TextMeshProUGUI confirmText;
	[SerializeField] Button confirmButton;
	Action ConfirmAction;

	public override void Registration(UIManager manager)
	{
		base.Registration(manager);
		confirmButton.onClick.AddListener(Confirm);
	}

	public override void Unregistration(UIManager manager)
	{
		base.Unregistration(manager);
		confirmButton.onClick.RemoveListener(Confirm);
		ConfirmAction = null;
	}

	public void Confirm()
	{
		ConfirmAction?.Invoke();
	}

	public void SetConfirmAction(Action newAction)
	{
		ConfirmAction -= newAction;
		ConfirmAction += newAction;
	}

	public void SetSystemMessage(string title, string context, string confirm)
	{
		titleText?.SetText(title);
		contextText?.SetText(context);
		confirmText?.SetText(confirm);
	}
}
