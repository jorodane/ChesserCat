using UnityEngine;

public class UI_Button_OpenScreen : MonoBehaviour
{
	[SerializeField] UIType wantType;
	[SerializeField] ScreenChangeType changeType;

    public void Open()
	{
		UIManager.ClaimOpenScreen(wantType, changeType);
	}
}
