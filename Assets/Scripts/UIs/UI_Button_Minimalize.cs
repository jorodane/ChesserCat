using UnityEngine;
using UnityEngine.UI;

public class UI_Button_Minimalize : MonoBehaviour
{
	[SerializeField] GameObject target;

    public void Toggle()
	{
		if(target)
		{
			target.SetActive(!target.activeSelf);
		}
	}
}
