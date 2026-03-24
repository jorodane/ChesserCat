using Unity.VisualScripting;
using UnityEngine;

public class MouseFollower : MonoBehaviour
{

    void Start()
    {
		//마우스 움직임이 발생했을 때에 할 일에 => 마우스 따라가기를 넣기!
		InputManager.OnMouseMove += MoveToMouse;
    }

	void MoveToMouse(Vector2 screenPosition, Vector3 worldPosition)
	{
		transform.position = worldPosition;
	}
}
