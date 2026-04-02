using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum UIType
{
	None, Loading, Title, Movable,
	_Length
}

//팝업이 일어나는 "이벤트"가 발생할 것이다
//델리게이트는 => 스킬을 무한히 배울 수 있는 친구!
//A스킬을 쓰면 => 슬라임
//B스킬을 쓰면 => 고블린
//A스킬과 B스킬을 가르쳐놨다 => 실행을 시키면 결과는 => 고블린 슬라임 => X
//맨 마지막 결과만 알려줘요!
//실행하면 고블린이나온다!
public delegate void PopUpEvent(string title, string context, string confirm);

public class UIManager : ManagerBase
{
	public static event PopUpEvent OnPopUp;

	Canvas _mainCanvas;
	public Canvas MainCanvas => _mainCanvas;

	GraphicRaycaster _raycaster;
	public GraphicRaycaster Raycaster => _raycaster;

	//어떤 창을 열어주세요!
	//         이 타입  어떤 오브젝트!
	Dictionary<UIType, UIBase> uiDictionary = new();

	public IEnumerator Initialize(GameManager newManager)
	{
		//GameObject.FindGameObjectWithTag("MainCanvas");
		SetMainCanvas(GetComponentInChildren<Canvas>());
		SetUI(UIType.Loading, GetComponentInChildren<UI_LoadingScreen>());
		yield return null;
	}

	protected override IEnumerator OnConnected(GameManager newManager)
	{
		UIBase movableUI = CreateUI(UIType.Movable, "MovableScreen");
		yield return null;
	}

	protected override void OnDisconnected()
	{
		//싹 다 나가!
		UnSetAllUI();
	}

	protected void SetMainCanvas(Canvas newCanvas)
	{
		_mainCanvas = newCanvas;
		if (_mainCanvas)
		{
			_raycaster = _mainCanvas.GetComponent<GraphicRaycaster>();
		}
		else
		{
			_raycaster = null;
		}
	}

	protected UIBase CreateUI(UIType wantType, string wantName)
	{
		GameObject instance = ObjectManager.CreateObject(wantName, _mainCanvas.transform);
		UIBase result = instance?.GetComponent<UIBase>();
		return SetUI(wantType, result);
	}

	protected void UnSetAllUI() // 싹 다 해고야
	{
		foreach(UIBase ui in uiDictionary.Values) //애들 전부 돌면서
		{
			UnsetUI(ui);//나가라고 해주기!
						//여기에서 나가라고 할 때마다 Dictionary에서 빼려고 하시는 분들이 있어요!
						//안되는 이유!
						//uiDictionary.Remove(wantType);
						//제거를 하는 경우 uiDictionary의 모양이 달라져서 모두를 돌다가...?
						//A,B,C,D,E,F
						//0 1 2 3 4 5 : 6명

						//A,B,C,D,E,F
						//0
						//B,C,D,E,F

						//B,C,D,E,F
						//  1
						//B, ,D,E,F

						//B,D,E,F
						//    2
						//B,D, ,F

						//B,D,F
						//      3
		}
		//다 나갔으니까 직원 명부를 버려버림!
		uiDictionary.Clear();
	}
	protected void UnsetUI(UIType wantType) //담당 공무원의 부서랑 직책만 알고 있는 경우
	{
		//그 직원을 찾아야 함
		//담당 공무원의 이름을 알고 있는 경우로 이동하시오.
		if(uiDictionary.TryGetValue(wantType, out UIBase found))
		{
			//처리하고
			UnsetUI(found);
			//너 해고야.
			uiDictionary.Remove(wantType);
		}
	}
	protected void UnsetUI(UIBase wantUI) //담당 공무원의 이름을 알고 있는 경우
	{
		if(!wantUI) return;

		wantUI.Unregistration(this);
	}
	public static void ClaimUnsetUI(UIBase wantUI)					=> GameManager.Instance?.UI?.UnsetUI(wantUI);
	public static void ClaimUnsetUI(GameObject wantObject)			=> ClaimUnsetUI(wantObject?.GetComponent<UIBase>());

	protected UIBase SetUI(UIBase wantUI)
	{
		wantUI?.Registration(this);
		return wantUI;
	}
	protected UIBase SetUI(UIType wantType, UIBase wantUI)
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
		//등록 완!
		return SetUI(wantUI);
	}
	public static UIBase ClaimSetUI(UIBase wantUI)					=> GameManager.Instance?.UI?.SetUI(wantUI);
	public static UIBase ClaimSetUI(GameObject wantObject)			=> ClaimSetUI(wantObject?.GetComponent<UIBase>());
	public static UIBase ClaimSetUI(UIType wantType, UIBase wantUI)	=> GameManager.Instance?.UI?.SetUI(wantType, wantUI);

	protected UIBase GetUI(UIType wantType)
	{
		if (uiDictionary.TryGetValue(wantType, out UIBase result)) return result; //있으면 result반환
		else return null; //없으면 null
	}
	public static UIBase ClaimGetUI(UIType wantType)				=> GameManager.Instance?.UI?.GetUI(wantType);

	protected UIBase OpenUI(UIType wantType)
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
	public static UIBase ClaimOpenUI(UIType wantType)				=> GameManager.Instance?.UI?.OpenUI(wantType);

	protected UIBase CloseUI(UIType wantType)
	{
		UIBase result = GetUI(wantType);
		//             자료형    이름   =>  변수 생성
		if(result is IOpenable asOpenable) asOpenable.Close();
		return result;
	}
	public static UIBase ClaimCloseUI(UIType wantType)				=> GameManager.Instance?.UI?.CloseUI(wantType);

	protected UIBase ToggleUI(UIType wantType)
	{
		UIBase result = GetUI(wantType);
		if(result is IOpenable asOpenable) asOpenable.Toggle();
		return result;
	}
	public static UIBase ClaimToggleUI(UIType wantType)				=> GameManager.Instance?.UI?.ToggleUI(wantType);

	public static void ClaimPopUp(string title, string context, string confirm)
	{
		OnPopUp?.Invoke(title, context, confirm);
	}
	public static void ClaimErrorMessage(string context)
	{
		OnPopUp?.Invoke("Error", context, "Confirm");
	}
}
