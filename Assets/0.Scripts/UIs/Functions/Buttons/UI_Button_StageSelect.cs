using UnityEngine;
using UnityEngine.EventSystems;

public class UI_Button_StageSelect : MonoBehaviour
{
	public void SelectStage(StageBase stage)
	{
		InputManager.ClaimSelectStage(stage);
		EventSystem focusHolder = EventSystem.current;
		if (focusHolder) focusHolder.SetSelectedGameObject(null);
	}
}
