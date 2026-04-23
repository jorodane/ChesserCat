using UnityEngine;

public class UI_ResignConfirmWindow : OpenableUIBase
{
    public void Confirm() => Debug.Log("Resigned");
	public void Cancel()  => UIManager.ClaimCloseUI(UIType.Resign);

}
