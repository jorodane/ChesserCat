using UnityEngine;

public class UIBase : MonoBehaviour
{
	public virtual void Registration(UIManager manager)
	{

	}

	public virtual void Unregistration(UIManager manager) 
	{
	
	}

	public GameObject SetChild(GameObject newChild)
	{
		if (!newChild) return null;
		//오브젝트의 자식을 추가하는 방법!
		//애기한테         너의 부모가    나란다
		newChild.transform.SetParent(transform);

		return OnSetChild(newChild);
	}

	protected virtual GameObject OnSetChild(GameObject newChild)
	{
		return newChild;
	}

	public void UnsetChild(GameObject oldChild)
	{
		if (!oldChild) return;
		//얘가 나를 부모라고 생각하고 있다면
		if (oldChild.transform.parent == transform)
		{
			//너의 부모는 없어. 라고 말해주기
			oldChild.transform.SetParent(null);
		}
		OnUnsetChild(oldChild);
	}

	protected virtual void OnUnsetChild(GameObject oldChild)
	{

	}
}
