using System;
using TMPro;
using UnityEngine;

public class UI_StageInfo : UIBase
{
	[SerializeField] TextMeshProUGUI stageNameText;
	[SerializeField] TextMeshProUGUI stageDetailText;
	[SerializeField] TextMeshProUGUI stageEngageText;


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
			if (stageNameText)	 stageNameText.SetText(value.GetStageName());
			if (stageDetailText) stageDetailText.SetText(value.GetStageDetail());
			if (stageEngageText) stageEngageText.SetText(value.GetStageEngageText());

        }
		else
		{
			if (stageNameText)	 stageNameText.SetText(string.Empty);
			if (stageDetailText) stageDetailText.SetText(string.Empty);
            if (stageEngageText) stageEngageText.SetText(string.Empty);
		}
	}
}
