using UnityEngine;
using UnityEngine.EventSystems;

public class UI_PullUpWindow : MonoBehaviour, IPointerDownHandler
{
	public void OnPointerDown(PointerEventData eventData)
	{
		//나의 "대립 중인 형제들" 중에서 맨 마지막 순번
		//         => 맨 마지막이 이기잖아요?
		//        Sibling
		//막내가 되도록 한다.
		transform.SetAsLastSibling();
	}
}
