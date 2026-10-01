using System;
using TMPro;
using UnityEngine;

public class UI_StageInfo : UIBase
{
	[SerializeField] TextMeshProUGUI stageNameText;
	[SerializeField] TextMeshProUGUI stageDetailText;

	public override void Registration(UIManager manager)
	{
		OnSelectStage(null);
		base.Registration(manager);
		InputManager.OnSelectStage -= OnSelectStage;
		InputManager.OnSelectStage += OnSelectStage;
	}

	public override void Unregistration(UIManager manager)
	{
		base.Unregistration(manager);
		InputManager.OnSelectStage -= OnSelectStage;
	}

	void OnSelectStage(StageBase value)
	{
		if(value)
		{
			stageNameText.SetText(value.GetStageName());
			stageDetailText.SetText(value.GetStageDetail());
		}
		else
		{
			stageNameText.SetText(string.Empty);
			stageDetailText.SetText(string.Empty);
		}
	}
}
