using UnityEngine;

public class UI_Button_OpenUI : MonoBehaviour
{
	[SerializeField] UIType wantType;
	[SerializeField] bool openOnTop = true;

    public void Open()
	{
		UIBase opened = null;
		opened = UIManager.ClaimOpenUI(wantType);

		if(openOnTop && opened)
		{
			opened.transform.SetAsLastSibling();
		}
	}

	public void Toggle()
	{
		UIBase opened = null;
		opened = UIManager.ClaimToggleUI(wantType);
		if (openOnTop && opened)
		{
			opened.transform.SetAsLastSibling();
		}
	}

	public void Close()
	{
		UIManager.ClaimCloseUI(wantType);
	}
}
