using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

//이벤트!
//"마우스가 클릭되는 이벤트"라는 상황이 발생했다고 해봅시다!
//마우스가 클릭되었다고 하는 것은 어떤 정보가 필요할까요?
//플레이어가 받을 정보는 무엇일까?
//어디가 눌렸는지? 
//                   4도류, 제비반환
//      대리자 => 너에게 내 기술을 전수해주마.
//                기능의 모양은 정해져 있습니다!
//      대리를 뛸 수 있다는 건 => 능력이 아주 좋다 => 가르쳐준 건 모두 한번에 씁니다!
// 플레이어가 할 일 대리 뛰어주고, 열려있는 창이 있다면 그 친구의 기능도 수행해주고
// 내가 신호 주면 연결되어 있는 모든 애들이 한 번에 뛰쳐나와서 일을 수행하고 간다!
public delegate void MouseMoveEvent(Vector2 screenPosition, Vector3 worldPosition);
public delegate void MouseButtonEvent(bool value, Vector2 screenPosition, Vector3 worldPosition);
public delegate void ButtonEvent(bool value);
public delegate void VectorEvent(Vector2 value);
public delegate void AxisEvent(float value);

//인풋 매니저는 PlayerInput없이 일을 할 수 있을까?
//할 수 없습니다.
//특정한 클래스는 특정한 컴포넌트와 함께 사용해야 한다!
//특정 클래스가 다른 클래스를 Dependence 의존하는 경우!
//다른 클래스가 필요해요! Require
//대상 변수나 클래스 위쪽에다가 [이렇게] 내용을 넣는 것을 Attribute : 속성
[RequireComponent(typeof(PlayerInput))]
public class InputManager : ManagerBase
{
	//delegate : 대리자 => 기술을 전수해놓고 기술을 시전하는 친구
	//                                     ------------------누구 명령?
	//                     대폭발 기술 전수 우리집에 있었어요. 내가 안시켰는데, 다른 사람이 시전
	//                    전수는 누구나 가능하지만 시전은 누가 할지 정할 수 있습니다!
	//                                                 "나만"
	//그냥 대리자는 누구나 등록하고 시전할 수 있지만
	//event 대리자는 누구나 등록하고 나만 시전할 수 있음!
	public static event MouseButtonEvent	OnMouseLeftButton;
	public static event MouseButtonEvent	OnMouseRightButton;
	public static event MouseMoveEvent		OnMouseMove;
	public static event ButtonEvent			OnCancel;
	public static event ButtonEvent			OnShowStatus;
	public static event VectorEvent			OnMove;

	PlayerInput targetInput;
	Dictionary<string, InputAction> actionDictionary = new();
	List<RaycastResult> cursorHitList = new();

	Vector2 cursorScreenPosition;
	Vector3 cursorWorldPosition;

	public bool is2D = true;

	protected override IEnumerator OnConnected(GameManager newManager)
	{
		//나랑 (무조건 죽을 때까지) 함께 있는 PlayerInput을 가져오고 싶다.
		targetInput = GetComponent<PlayerInput>();

		LoadAllActions();
		InitializeAllActions();

		//저희가 만약에 액션들을 다 불러온 상태에서 액션을 추가해줘야 한다!
		//ESC를 누른다!
		//취소, 창이 닫힌다거나, Pause
		//글면 이 작업들을 InputManager가 하는게 맞나?
		//스킬 취소는.. 누가 하는 걸까?
		//그렇다면 InputManager가 Player를 알면 되겠구나!
		//InputManager는 키가 눌렸다는 것을 전세계에 알리고
		//Player는 화들짝 놀라서 스킬을 취소함
		//Event => Subscribers
		//                                          원래 있었으면 빼고 아니면 말고니까
		//											추가할 때마다 빼고 넣으면
		//											무조건 개수는 한 개가된다!
		GameManager.OnUpdateManager -= UpdateEvent; //뺄 건데, 없으면 말고
		GameManager.OnUpdateManager += UpdateEvent;
		yield return null;
	}

	protected override void OnDisconnected()
	{
		GameManager.OnUpdateManager -= UpdateEvent;
	}

	public void UpdateEvent(float deltaTime)
	{
		RefreshGameObjectUnderCursor();
	}

	void RefreshGameObjectUnderCursor()
	{
		cursorHitList.Clear();
		if (is2D)
		{
			GameManager.Instance.Camera.GetRaycastResult2D(cursorScreenPosition, cursorHitList);
		}
		else
		{
			GameManager.Instance.Camera.GetRaycastResult3D(cursorScreenPosition, cursorHitList);
		}
	}

	public GameObject GetGameObjectUnderCursor()
	{
		//마우스에 닿은것의 개수가 0이라면 => 없으니까 돌아가라
		if(cursorHitList.Count == 0) return null;
		return cursorHitList[0].gameObject; //일단 지금은 임시로 첫 번째 오브젝트 돌려주기!
	}

	void LoadAllActions()
	{
		//여러분들이 저번에 게임 만들 때에 "앞으로"가는 키가 뭐였죠?
		//"Forward" -> [D]키로 만들었습니다. : D키로 이동하고 싶지 않으면요?
		//키 변경이 가능하던가요?
		//유저가 키 변경을 할 수 있게 하려면? "Forward"가 뭔지는 알아야
		//=> Forward의 버튼을 알 수 있음
		//OnMouseLeftButtonDown이라는 함수를 만들면 그냥 됐었는데
		//그걸 안 쓰는 이유는 뭘까? => 직접 연결하기 위해!
		//MouseLeftButtonDown이라는 이름의 액션을 만들었죠
		//유니티는 OnMouseLeftButtonDown이라고 하는 이름의 함수를
		//제 스크립트에서 "찾아서" 실시간으로 "실행할 수 있는 기능"을 불러와야 합니다
		//유니티보고 찾으라는 게 아니라 내가 직접 꽂아줄 거
		foreach (InputAction currentAction in targetInput.actions)
		{ 
			actionDictionary.TryAdd(currentAction.name, currentAction);
		}
	}

	void InitializeAllActions()
	{
		if (actionDictionary == null || actionDictionary.Count == 0) return;

		InitializeAction("CursorPositionChanged",(context) => CursorPositionChanged(GetVector2Value(context)));
		InitializeAction("Move",				 (context) => OnMove?.Invoke(GetVector2Value(context)));

		InitializeAction("MouseLeftButtonDown",  (context) => OnMouseLeftButton ?.Invoke(true,  cursorScreenPosition, cursorWorldPosition));
		InitializeAction("MouseRightButtonDown", (context) => OnMouseRightButton?.Invoke(true,  cursorScreenPosition, cursorWorldPosition));
		InitializeAction("MouseLeftButtonUp",	 (context) => OnMouseLeftButton ?.Invoke(false, cursorScreenPosition, cursorWorldPosition));
		InitializeAction("MouseRightButtonUp",	 (context) => OnMouseRightButton?.Invoke(false, cursorScreenPosition, cursorWorldPosition));

		InitializeAction("Cancel",				 (context) => OnCancel			?.Invoke(true));
		InitializeAction("ShowStatusButtonDown", (context) => OnShowStatus		?.Invoke(true));
		InitializeAction("ShowStatusButtonUp",	 (context) => OnShowStatus		?.Invoke(false));
	}

	void InitializeAction(string actionName, Action<InputAction.CallbackContext> actionMethod)
	{
		if (actionDictionary == null) return;
		if (actionDictionary.TryGetValue(actionName, out InputAction cursorPositionChange))
		{
			//커서위치변경액션의 발동에다가 CursorPositionChanged함수를 추가!
			//이것도 같이 해줘!
			cursorPositionChange.performed += actionMethod;
		}
	}

	T GetInputValue<T>(InputAction.CallbackContext context) where T : struct
	{
		if (context.valueType != typeof(T)) return default;
		return context.ReadValue<T>();
	}

	Vector2 GetVector2Value(InputAction.CallbackContext context) => GetInputValue<Vector2>(context);

	void CursorPositionChanged(Vector2 screenPosition)
	{
		//마우스의 화면상 실제 픽셀 위치
		//화면상 x축으로 1픽셀을 움직이면
		//유니티에서 "1칸"은 1m
		//화면 => 세상
		//필요한 것이 무엇일까? => 기준점이 되는 좌표
		//화면의 왼쪽 끝은 세상의 어디일까?
		//카메라가 필요하다
		//카메라를 기준으로 세상을 본다!
		//절두체
		Vector3 worldPosition;

		if(is2D)
		{
			worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);
			worldPosition.z = 0;
		}
		else
		{
			worldPosition = Vector3.zero;
		}

		//음.. 위치를 잘 찾아왔군. 내놓아
		cursorScreenPosition = screenPosition;
		cursorWorldPosition = worldPosition;



		//대리자는 모든 스킬을 한 번에 사용할 수 있는 친구 => 사기캐
		//....배운 스킬이 없으면?
		OnMouseMove?.Invoke(screenPosition, worldPosition);
	}
}
