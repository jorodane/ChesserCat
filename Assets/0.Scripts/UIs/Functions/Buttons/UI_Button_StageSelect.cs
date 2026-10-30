using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Button_StageSelect : MonoBehaviour
{
	[SerializeField] Button selectButton;
	string currentLabel;
	StageBase currentStage;
	StageCondition[] currentCondition;

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
