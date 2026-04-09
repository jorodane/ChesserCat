using UnityEngine;

public class UI_Button_OpenUI : MonoBehaviour
{
	[SerializeField] UIType wantType;
	[SerializeField] bool wantToggle;

    public void Open()
	{
		if(wantToggle)	UIManager.ClaimToggleUI(wantType);
		else			UIManager.ClaimOpenUI(wantType);
	}
}
