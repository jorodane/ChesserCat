using UnityEngine;

public class CharacterBase : MonoBehaviour
{
	//가장 중요한 기능!
	//말을 했을 때 말을 잘 들어먹는 것
	ControllerBase _controller;
	public ControllerBase Controller => _controller;

	public virtual string DisplayName => "character";

	//                    빙의되다
	protected virtual void OnPossessed(ControllerBase newController){}
	public ControllerBase Possessed(ControllerBase from)
	{
		//빙의를 하려고 했는데 원래 영혼이 들어있었어요
		//영혼이 있더라도 빙의가 가능
		//영혼이 있으면 튕겨냈다!
		if (Controller) Unpossessed();
		_controller = from;
		OnPossessed(Controller);
		return Controller;
	}

	
	//          혼이 나가다
	protected virtual void OnUnpossessed(ControllerBase oldController){}
	public void Unpossessed()
	{
		if(Controller) OnUnpossessed(Controller);
		_controller = null;
	}

	public bool Unpossessed(ControllerBase oldController)
	{
		if (Controller != oldController) return false;
		Unpossessed();
		return true;
	}

}
