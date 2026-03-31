using Unity.VisualScripting;
using UnityEngine;

public class MouseFollower : MonoBehaviour, IFunctionable
{
	void Start()
	{
		RegistrationFunctions();
	}
	void OnDestroy()
	{
		UnregistrationFunctions();
	}

	public void RegistrationFunctions()
	{
		//마우스 움직임이 발생했을 때에 할 일에 => 마우스 따라가기를 넣기!
		//클릭할 때 기능을 추가했다!
		InputManager.OnMouseLeftDown += CreateToMouse;
		InputManager.OnMouseRightDown += DestroyOnMouse;
	}

	//사람이 언제 죽는지 아나?
	//사람들에게서 잊혀졌을 때다
	public void UnregistrationFunctions()
	{
		InputManager.OnMouseLeftDown -= CreateToMouse;
		InputManager.OnMouseRightDown -= DestroyOnMouse;
	}


	void DestroyOnMouse(Vector2 screenPosition, Vector3 worldPosition)
	{
		ObjectManager.DestroyObject(GameManager.Instance.Input.GetGameObjectUnderCursor());
	}

	void CreateToMouse(Vector2 screenPosition, Vector3 worldPosition)
	{
		//저희가.. 로딩해놓은 거 있잖아요!
		GameObject inst = ObjectManager.CreateObject("NemoMan", worldPosition);
	}

	void MoveToMouse(Vector2 screenPosition, Vector3 worldPosition)
	{
		DestroyOnMouse(screenPosition, worldPosition);
	}
}
