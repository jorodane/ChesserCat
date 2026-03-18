using UnityEngine;

public class UI_LoadingScreen : UIBase, IOpenable
{
	//프로퍼티를 만들 때에 항상 원본이 되는 변수를 만들어줬는데
	//get;set;만 있는 경우에는 그냥 변수처럼 쓸 수 있어요!
	//set만 protected인 변수처럼 활용!
	public bool IsOpen => gameObject.activeSelf;
	public void Close() => gameObject.SetActive(false);
	public void Open() => gameObject.SetActive(true);
	public void Toggle() => gameObject.SetActive(!IsOpen);
}
