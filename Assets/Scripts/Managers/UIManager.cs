using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UIType
{
	None, Loading, Title,
	_Length
}

public class UIManager : ManagerBase
{
	Canvas _mainCanvas;
	public Canvas MainCanvas => _mainCanvas;

	//어떤 창을 열어주세요!
	//         이 타입  어떤 오브젝트!
	Dictionary<UIType, UIBase> uiDictionary = new();

	protected override IEnumerator OnConnected(GameManager newManager)
	{
		_mainCanvas = GetComponentInChildren<Canvas>();
		//GameObject.FindGameObjectWithTag("MainCanvas");
		yield return null;
	}

	protected override void OnDisconnected()
	{

	}

	public UIBase SetUI(UIType wantType, UIBase wantUI)
	{
		//Set UI를 하려고 하는데 문제가 무엇일까!
		//InventoryType, InventoryInstance
		if (wantUI == null) return null; // 승상께서 나를 더 필요로 하시지 않는구나

		//어? 뭐야? 이미 Inventory는 있는데? 너는 누구냐! => 서생원
		//일단 문전박대 => 프로그래밍에서는요? 똑같은 기능을 하는 친구면
		//음.. 너가 원본인 건 무슨 상관인데?
		//뒤이어서 들어온 친구는 치워버리겠다!
		if (uiDictionary.TryGetValue(wantType, out UIBase origin)) return origin;

		//두 가지의 시련을 모두 통과하다니. 너는 등록될 수 있는 자격을 갖추었다.
		uiDictionary.Add(wantType, wantUI);
		return wantUI;
	}

	public UIBase GetUI(UIType wantType)
	{
		if (uiDictionary.TryGetValue(wantType, out UIBase result)) return result; //있으면 result반환
		else return null; //없으면 null
	}

	public UIBase OpenUI(UIType wantType)
	{
		//Result가 누군지 전혀 모름!  리스코프 치환 원칙
		//IOpenable이면 열게 해준다! 세부 요소는 모르겠는데, 상위 요소만으로 실행하기
		UIBase result = GetUI(wantType);
		//이게 "열 수 있는"인 건 어떻게 확인할까요?
		//IOpenable인지 체크해보면 열 수 있는지 알 수 있습니다.
		//IOpenable로서 활동 할 수 있으면 IOpenable
		//result는 IOpenable인 opener인가?
		if(result is IOpenable asOpenable) asOpenable.Open();

		//아랫줄이랑 같은 의미예요!
		//IOpenable opener = result as IOpenable;
		//if(opener != null) opener.Open();
		return result;
	}

	public UIBase CloseUI(UIType wantType)
	{
		UIBase result = GetUI(wantType);
		if(result is IOpenable asOpenable) asOpenable.Close();
		return result;
	}

	public UIBase ToggleUI(UIType wantType)
	{
		UIBase result = GetUI(wantType);
		if(result is IOpenable asOpenable) asOpenable.Toggle();
		return result;
	}
}
