using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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
public delegate void MouseHoverEvent(GameObject newTarget, GameObject oldTarget);
public delegate void ButtonEvent(bool value);
public delegate void VectorEvent(Vector2 value);
public delegate void CharacterEvent(CharacterBase value);
public delegate void AxisEvent(float value);
public delegate void NumberEvent(int value);

//인풋 매니저는 PlayerInput없이 일을 할 수 있을까?
//할 수 없습니다.
//특정한 클래스는 특정한 컴포넌트와 함께 사용해야 한다!
//특정 클래스가 다른 클래스를 Dependence 의존하는 경우!
//다른 클래스가 필요해요! Require
//대상 변수나 클래스 위쪽에다가 [이렇게] 내용을 넣는 것을 Attribute : 속성
[RequireComponent(typeof(PlayerInput))]
public class InputManager : ManagerBase
{
	public const int SelectableMaxIndex = 8;

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
	public static event MouseHoverEvent		OnMouseHover;

	public static event CharacterEvent			OnCharacterSelect;
	public static void ClaimCharacter(CharacterBase value) => OnCharacterSelect?.Invoke(value);

	public static event ButtonEvent			OnConfirm;
    public static void ClaimConfirm(bool value) => OnConfirm?.Invoke(value);

	public static event ButtonEvent			OnCancel;
	public static void ClaimCancel(bool value) => OnCancel?.Invoke(value);

	public static event ButtonEvent			OnCommandAttack;
	public static void ClaimCommandAttack(bool value) => OnCommandAttack?.Invoke(value);

	public static event ButtonEvent			OnCommandInfo;
	public static void ClaimCommandInfo(bool value) => OnCommandInfo?.Invoke(value);

	public static event ButtonEvent			OnCommandMove;
	public static void ClaimCommandMove(bool value) => OnCommandMove?.Invoke(value);

	public static event ButtonEvent			OnCommandCancel;
	public static void ClaimCommandCancel(bool value) => OnCommandCancel?.Invoke(value);

	public static event ButtonEvent			OnCommandClearGuide;
	public static void ClaimCommandClearGuide(bool value) => OnCommandClearGuide?.Invoke(value);

	public static event ButtonEvent			OnShowStatus;
	public static void ClaimShowStatus(bool value) => OnShowStatus?.Invoke(value);

	public static event ButtonEvent			OnSelectPrev;
	public static void ClaimSelectPrev(bool value) => OnSelectPrev?.Invoke(value);

	public static event ButtonEvent			OnSelectNext;
	public static void ClaimSelectNext(bool value) => OnSelectNext?.Invoke(value);

    public static event VectorEvent			OnTileMove;
	public static event ButtonEvent			OnResetTilePosition;
    public static void ClaimResetTilePosition(bool value) => OnResetTilePosition?.Invoke(value);

    public static event NumberEvent			OnSelectByNumber;
	public static void ClaimSelectByNumber(int value) => OnSelectByNumber?.Invoke(value);


	public static event Action				OnAnyKey;

	static Vector2				_cursorScreenPosition;
	public static Vector2		CursorScreenPosition => _cursorScreenPosition;

	static Vector3				_cursorWorldPosition;
	public static Vector3		CursorWorldPosition => _cursorWorldPosition;

	static ISelectable			_cursorHoverSelectable;
	public static ISelectable	CursorHoverSelectable => _cursorHoverSelectable;

	static GameObject			_cursorHoverObjectReal;
	public static GameObject	CursorHoverObjectReal => _cursorHoverObjectReal;

    static bool					_isCursorHoverOnUI;
	public static bool			IsCursorHoverOnUI => _isCursorHoverOnUI;

	PlayerInput targetInput;
	Dictionary<string, InputAction> actionDictionary = new();
	List<RaycastResult> cursorHitList = new();

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
		RefreshGameObjectUnderCursor(_cursorScreenPosition);
	}

	void RefreshGameObjectUnderCursor(Vector2 screenPosition)
	{
		cursorHitList.Clear();
		CameraManager.GetRaycastResult(screenPosition, cursorHitList);

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
		try
		{
			worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);
		}
		catch (Exception e)
		{
			worldPosition = Vector3.zero;
			UIManager.ClaimErrorMessage(e.Message);
		}
		GameObject firstObject = null;

		//마우스에 닿을 수 있는 물체는 뭐가 있을까?
		//UI 2D 3D
		//맨 첫번째에 있는 친구가 보통 UI
		//제일 첫 번째에 있는 친구가 GraphicRaycaster에 의해서 선별된 경우
		//첫 번째 친구가 UI구나!
		//                                             element : UI 그래픽 요소
		_isCursorHoverOnUI = cursorHitList.Count > 0 && cursorHitList[0].module is GraphicRaycaster;

		if (_isCursorHoverOnUI)
		{
			firstObject = cursorHitList[0].gameObject;
		}
		if (GameManager.is2D)
		{
			worldPosition.z = 0;
			//order in layer는 2byte 자료형
			//-32768 ~ 32767 만 가능하기 때문에
			//layer를 100000배 해버리면
			//layer가 1일 때에 67232 ~ 132767이기 때문에
			//밑 레이어가 위 레이어에 숫자로 이길 가능성이 없다!
			float GetValue(RaycastResult target)
			{
				return target.sortingOrder + target.sortingLayer * 100000;
			}
			RaycastResult nearest = cursorHitList.GetMaximum<RaycastResult>(GetValue);
			firstObject = nearest.gameObject; //오브젝트 꺼내오고
		}
		else
		{
			//함수의 내부에서 함수를 만들기!
			//람다 : 이름 없는 메소드
			//람다랑 똑같은데 이름이 있을 뿐!
			float GetDistance(RaycastResult target)
			{
				return target.distance;
			}
			//가장 가까운 대상을 찾고
			RaycastResult nearest = cursorHitList.GetMinimum<RaycastResult>(GetDistance);
			firstObject = nearest.gameObject; //오브젝트 꺼내오고
			worldPosition = nearest.worldPosition; //위치 꺼내오고
		}

		//변수 서로 바꾸기 문제
		//  A  B        A  B  C
		//  1  3        1  3
		//  3  3  ??    1  3  1
		//              3  3  1
		//              3  1  1
		GameObject lastHoverObject = _cursorHoverObjectReal;
		ISelectable lastHoverSelectable = _cursorHoverSelectable;

		//음.. 위치를 잘 찾아왔군. 내놓아
		_cursorScreenPosition = screenPosition;
		_cursorWorldPosition = worldPosition;
		_cursorHoverObjectReal = firstObject;
		_cursorHoverSelectable = firstObject?.GetComponent<ISelectable>();
		if (_cursorHoverSelectable is not null)
		{
			_cursorHoverObjectReal = _cursorHoverSelectable.GetHoveredObject();
			firstObject = _cursorHoverObjectReal;
			if (_cursorHoverObjectReal)
			{
				_cursorHoverSelectable = _cursorHoverObjectReal.GetComponent<ISelectable>() ?? _cursorHoverSelectable;
			}
		}
		else _cursorHoverObjectReal = firstObject;

		//커서가 올라갔던 오브젝트가 1등 오브젝트랑 다르다!
		if (lastHoverObject != _cursorHoverObjectReal)
		{
			lastHoverSelectable?.MouseHoverExit();
			_cursorHoverSelectable?.MouseHoverEnter();
			//마우스 호버 변경됨!    이번 1등        원래 1등
			OnMouseHover?.Invoke(_cursorHoverObjectReal, lastHoverObject);
		}
	}

	public GameObject GetGameObjectUnderCursor()
	{
		//마우스에 닿은것의 개수가 0이라면 => 없으니까 돌아가라
		if (cursorHitList.Count == 0) return null;
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

		InitializeAction("CursorPositionChanged", (context) => CursorPositionChanged(GetVector2Value(context)));



		InitializeAction("MouseLeftButton"		, (context) => OnMouseLeftButton ?.Invoke(true,  _cursorScreenPosition, _cursorWorldPosition)
												, (context) => OnMouseLeftButton ?.Invoke(false, _cursorScreenPosition, _cursorWorldPosition));

		InitializeAction("MouseRightButton"		, (context) => OnMouseRightButton?.Invoke(true,  _cursorScreenPosition, _cursorWorldPosition)
												, (context) => OnMouseRightButton?.Invoke(false, _cursorScreenPosition, _cursorWorldPosition));

		InitializeAction("ShowStatus"			, (context) => ClaimShowStatus		  (true)
												, (context) => ClaimShowStatus		  (false));
																					  
		InitializeAction("CommandAttack"		, (context) => ClaimCommandAttack	  (true));
		InitializeAction("CommandInfo"			, (context) => ClaimCommandInfo		  (true));
		InitializeAction("CommandMove"			, (context) => ClaimCommandMove		  (true));
		InitializeAction("CommandCancel"		, (context) => ClaimCommandCancel	  (true));
		InitializeAction("CommandClearGuide"	, (context) => ClaimCommandClearGuide (true));
																					  
		InitializeAction("Cancel"				, (context) => ClaimCancel			  (true));
		InitializeAction("Confirm"				, (context) => ClaimConfirm			  (true));
																					  
		InitializeAction("SelectPrev"			, (context) => ClaimSelectPrev		  (true));
		InitializeAction("SelectNext"			, (context) => ClaimSelectNext		  (true));

        InitializeAction("TileMove"             , (context) => OnTileMove?.Invoke(GetVector2Value(context))
                                                , (context) => OnTileMove?.Invoke(Vector2.zero));
        InitializeAction("ResetTilePosition"    , (context) => ClaimResetTilePosition (true));

		for (int i = 0; i < SelectableMaxIndex; i++)
		{
			int currentNumber = i;
			InitializeAction($"Select{i:00}"	, (context) => ClaimSelectByNumber(currentNumber));
		}

		InitializeAction("AnyKey"				, (context) => OnAnyKey			?.Invoke());
	}

	void InitializeAction(string actionName, Action<InputAction.CallbackContext> actionMethod, Action<InputAction.CallbackContext> cancelMethod = null)
	{
		if (actionDictionary == null) return;
		if (actionDictionary.TryGetValue(actionName, out InputAction currentInput))
		{
			//												발동할 때 할 일
			if(actionMethod is not null) currentInput.performed += actionMethod;
			//												취소될 때 할 일
			if(cancelMethod is not null) currentInput.canceled  += cancelMethod;
			//       키가 눌렸을 때
			//currentInput.started
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
		RefreshGameObjectUnderCursor(screenPosition); //새로고침 한 번 때려주고!
		//대리자는 모든 스킬을 한 번에 사용할 수 있는 친구 => 사기캐
		//....배운 스킬이 없으면?
		OnMouseMove?.Invoke(_cursorScreenPosition, _cursorWorldPosition);
	}
}
