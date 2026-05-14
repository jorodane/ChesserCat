using UnityEngine;
using TMPro;
public class UI_VersionText : MonoBehaviour
{
	[SerializeField] TextMeshProUGUI targetText;
    void Start()
    {
		targetText.SetText($"V.{Application.version}");
    }
}
