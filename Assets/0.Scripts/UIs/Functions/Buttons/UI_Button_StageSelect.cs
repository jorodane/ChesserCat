using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Button_StageSelect : MonoBehaviour
{
	[SerializeField] Button selectButton;
    [SerializeField] GameObject selectMarker;
	string currentLabel;
	StageBase currentStage;
	StageCondition[] currentCondition;

    void OnEnable()
    {
        InputManager.OnSelectStage -= OnStageChanged;
        InputManager.OnSelectStage += OnStageChanged;
    }

    void OnDisable()
    {
        InputManager.OnSelectStage -= OnStageChanged;
    }

    void OnStageChanged(StageBase value)
    {
        if (!selectMarker) return;
        selectMarker.SetActive(value == currentStage);
    }

    public void SetStage(StageBase wantStage)
	{
		currentStage = wantStage;
    }

	public void SetLabel(string wantLabel)
	{
        currentLabel = wantLabel;
    }

	public void SetCondition(in StageCondition[] wantCondition)
	{
        currentCondition = wantCondition;
        SetSelectable(CheckCondition());
    }

	public void SetSelectable(bool wantSelectable)
	{
		//selectButton.interactable = wantSelectable;
		gameObject.SetActive(wantSelectable);
    }

	public bool CheckCondition()
	{
		if (currentCondition is null || currentCondition.Length <= 0) return true;

		foreach (StageCondition condition in currentCondition)
		{
			if (!condition.Check()) return false;
		}
		return true;
	}

	public void SelectStage()
	{
		if (!currentStage) return;
		SelectStage(currentStage);
    }
	public void SelectStage(StageBase wantStage)
	{
		InputManager.ClaimSelectStage(wantStage);
		EventSystem focusHolder = EventSystem.current;
		if (focusHolder) focusHolder.SetSelectedGameObject(null);
	}
}
