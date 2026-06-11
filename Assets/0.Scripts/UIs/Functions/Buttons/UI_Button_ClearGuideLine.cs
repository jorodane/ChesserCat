using UnityEngine;

public class UI_Button_ClearGuideLine : MonoBehaviour
{
    public void ClearGuideLine()
	{
		TileManager.ClaimResetGuideLine();
	}
}
