using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//팝업이 일어나는 "이벤트"가 발생할 것이다
//델리게이트는 => 스킬을 무한히 배울 수 있는 친구!
//A스킬을 쓰면 => 슬라임
//B스킬을 쓰면 => 고블린
//A스킬과 B스킬을 가르쳐놨다 => 실행을 시키면 결과는 => 고블린 슬라임 => X
//맨 마지막 결과만 알려줘요!
//실행하면 고블린이나온다!
public delegate void PopUpEvent(string title, string context, string confirm);
public delegate void UIToggleEvent<KeyType>(KeyType targetType, bool isOpen);

public class UIManager : ManagerBase
{
	public static UIManager instance => GameManager.UI;

	public static event PopUpEvent OnPopUp;
	public static event UIToggleEvent<UIType> OnUIToggle;
	public static event UIToggleEvent<ScreenType> OnScreenToggle;

	readonly KeyValuePair<ScreenType, string>[] globalScreenArray =
	{
		new (ScreenType.Title , "TitleScreen"),
		new (ScreenType.Battle, "BattleScreen"),
		new (ScreenType.Stage,	"StageScreen"),
	};


	Canvas _mainCanvas;
	public Canvas MainCanvas => _mainCanvas;
	public static Canvas GetMainCanvas() => instance?.MainCanvas;

	UIBase _movableScreen;
	RectTransform _overlayTransform;
	RectTransform _switcherTransform;
	RectTransform _createdTransform;
	RectTransform _changerTransform;

	GraphicRaycaster _raycaster;
	public GraphicRaycaster Raycaster => _raycaster;

	UIContainer<UIType, UIBase> commonModule;
	UIContainer<ScreenType, UI_ScreenBase> screenModule;

	Dictionary<ScreenChangeType, UI_ScreenChanger> screenChangerDictionary = new();

	Rect _uiBoundary;
	public static Rect UIBoundary => instance?._uiBoundary ?? Rect.zero;

	ScreenType _currentScreenType = ScreenType.None;
	public static ScreenType CurrentScreen => instance?._currentScreenType ?? ScreenType.None;

	UI_ScreenChanger currentScreenChanger;

	float _uiScale = 1.0f;
	public static float UIScale => instance?._uiScale ?? 1.0f;

	public IEnumerator Initialize(GameManager newManager)
	{
		//GameObject.FindGameObjectWithTag("MainCanvas");
		SetMainCanvas(GetComponentInChildren<Canvas>());
		commonModule = new(this);
		screenModule = new(this);
		screenModule.SetUI(GetComponentInChildren<UI_LoadingScreen>(), ScreenType.Loading);
		yield return null;
	}

	public RectTransform CreateFullScreen(string wantName)
	{
		GameObject instance = new GameObject(wantName);
		RectTransform result = instance.AddComponent<RectTransform>();
		//메인 캔버스에 넣고
		result.SetParent(MainCanvas.transform);
		//맨 위로 올려주기!
		result.SetAsFirstSibling();
		//anchor를 stretch - stretch로 만들고
		result.anchorMin = Vector3.zero;
		result.anchorMax = Vector3.one;
		//여백을 0,0,0,0
		result.offsetMin = Vector3.zero;
		result.offsetMax = Vector3.zero;
		//크기를 1로
		result.localScale = Vector3.one;

		return result;
	}

	protected override IEnumerator OnConnected(GameManager newManager)
	{
		_createdTransform = CreateFullScreen("CreatedUI");
		_movableScreen = screenModule?.CreateUI(ScreenType.Movable, "MovableScreen", MainCanvas?.transform);

		_switcherTransform = CreateFullScreen("ScreenSwitcher");

		foreach (var currentPair in globalScreenArray)
		{
			UIBase created = screenModule?.CreateUI(currentPair.Key, currentPair.Value, _switcherTransform);
			if (created is IOpenable asOpenable) asOpenable.Close(false);
		}

		_changerTransform = CreateFullScreen("ScreenChangers");
		_changerTransform.SetAsLastSibling();

		_overlayTransform = CreateFullScreen("OverlayTransform");
		_overlayTransform.SetAsLastSibling();


		for (ScreenChangeType currentChanger = (ScreenChangeType)1;  //int i = 0;   
			currentChanger < ScreenChangeType._Length;              //i < 3;
			currentChanger++)                                       //i++
		{
			//enum 이름을 가지고 파일 이름으로 취급할 것!
			GameObject instance = ObjectManager.CreateObject(currentChanger.ToString(), _changerTransform);
			//만든 대상에게서 스크린 체인저 기능을 가져오기!
			if (instance?.TryGetComponent(out UI_ScreenChanger asChanger) ?? false)
			{
				//가져와졌으면 딕셔너리에 추가하기!
				screenChangerDictionary.Add(currentChanger, asChanger);
			}
			//끄고 갑시다!
			instance?.SetActive(false);
		}

		yield return null;
	}

	protected override void OnDisconnected()
	{
		//싹 다 나가!
		commonModule.UnSetAllUI();
		screenModule.UnSetAllUI();
		commonModule = null;
		screenModule = null;
	}

	protected void SetMainCanvas(Canvas newCanvas)
	{
		_mainCanvas = newCanvas;
		if (MainCanvas)
		{
			_raycaster = MainCanvas.GetComponent<GraphicRaycaster>();

			if (MainCanvas.transform is RectTransform mainRectTransform)
			{
				LayoutRebuilder.ForceRebuildLayoutImmediate(mainRectTransform);
				_uiScale = mainRectTransform.lossyScale.x;
				_uiBoundary = mainRectTransform.rect;
				//_uiBoundary.size *= _uiScale;
				//_uiBoundary.position *= _uiScale / 1.0f;
			}
		}
		else
		{
			_raycaster = null;
		}
	}

	public UIBase	CreateOverlay(UIType wantType, string wantName)				=> instance?.commonModule?.CreateUI(wantType, wantName, _overlayTransform, MainCanvas.transform, _movableScreen);
	public static UIBase	ClaimOverlay(UIType wantType, string wantName)		=> instance?.CreateOverlay(wantType, wantName);
	public UIBase	CreateUI(UIType wantType, string wantName)					=> instance?.commonModule?.CreateUI(wantType, wantName, _createdTransform, MainCanvas.transform, _movableScreen);
	public static UIBase	ClaimCreateUI(UIType wantType, string wantName)		=> instance?.CreateUI(wantType, wantName);
	public static UIBase	ClaimCreateUI(string wantName, Transform parent)	=> instance?.commonModule?.CreateUI(wantName, parent);
	public UIBase	CreateUI(string wantName)									=> instance?.commonModule?.CreateUI(wantName, _createdTransform, MainCanvas.transform, _movableScreen);
	public static UIBase	ClaimCreateUI(string wantName)						=> instance?.CreateUI(wantName);
	public static void		ClaimUnsetUI(UIBase wantUI)							=> instance?.commonModule?.UnsetUI(wantUI);
	public static void		ClaimUnsetUI(GameObject wantObject)					=> ClaimUnsetUI(wantObject?.GetComponent<UIBase>());
	public static UIBase	ClaimSetUI(UIBase wantUI)							=> instance?.commonModule?.SetUI(wantUI);
	public static UIBase	ClaimSetUI(GameObject wantObject)					=> ClaimSetUI(wantObject?.GetComponent<UIBase>());
	public static UIBase	ClaimSetUI(UIBase wantUI, UIType wantType)			=> instance?.commonModule?.SetUI(wantUI, wantType);
	public static UIBase	ClaimGetUI(UIType wantType)							=> instance?.commonModule?.GetUI(wantType);
	public static bool		ClaimCheckOpen(UIType wantType)						=> instance?.commonModule?.IsOpen(wantType) ?? false;
	public static bool		ClaimCheckOpen(UIType wantType, out IOpenable resultOpenable)
	{
		resultOpenable = default;
		return instance?.commonModule?.IsOpen(wantType, out resultOpenable) ?? false;
	}
	public static bool ClaimCloseUI(params UIType[] wantTypes)				=> instance?.commonModule?.CloseUI(OnUIToggle, wantTypes) ?? false;
	public static UIBase ClaimOpenUI(UIType wantType, bool isActiveByKey = false) => instance?.commonModule?.OpenUI(wantType, OnUIToggle, isActiveByKey);
	public static UIBase ClaimCloseUI(UIType wantType, bool isActiveByKey = false) => instance?.commonModule?.CloseUI(wantType, OnUIToggle, isActiveByKey);
	public static UIBase ClaimToggleUI(UIType wantType, bool isActiveByKey = false) => instance?.commonModule?.ToggleUI(wantType, OnUIToggle, isActiveByKey);

	public static UIBase ClaimGetScreen(ScreenType wantType) => instance?.screenModule?.GetUI(wantType);

	protected UI_ScreenBase OpenScreen(ScreenType wantType)
	{
		screenModule?.CloseUI(CurrentScreen, OnScreenToggle);         //원래 있던 거 닫고
		_currentScreenType = wantType;  //이게 내 새로운 타입이다!
		return screenModule?.OpenUI(wantType, OnScreenToggle);        //그리고 열기
	}

	public static UI_ScreenBase ClaimOpenScreen(ScreenType wantType) => instance?.OpenScreen(wantType);


	protected void OpenScreen(ScreenType wantScreen, ScreenChangeType changeType, System.Action EventOnScreenCovered)
	{
		EventOnScreenCovered = (() => OpenScreen(wantScreen)) + EventOnScreenCovered;
		//                                  lambda : 프로젝트 내에서 한 번만 사용할 함수!
		ClaimScreenChangeEffect(changeType, EventOnScreenCovered);
	}

	public static void ClaimOpenScreen(ScreenType wantScreen, ScreenChangeType changeType, System.Action EventOnScreenCovered = null) 
		=> instance?.OpenScreen(wantScreen, changeType, EventOnScreenCovered);

	protected void ScreenChangeEffectStart(ScreenChangeType wantType, System.Action endFunction = null)
	{
		//현재 스크린이 변경 중이면 끝내기!
		if (currentScreenChanger) return;

		//일단 스크린 체인저를 가져오기!
		if(screenChangerDictionary.TryGetValue(wantType, out UI_ScreenChanger result))
		{
			if (!result)
			{
				endFunction?.Invoke(); //내용물이 없으니까 여기서도 함수 해주고 끝내기
				return;
			}
			//켠다!
			result.gameObject.SetActive(true);
			//애니메이션도 해라~! 끝나면 이걸 해줘!
			result.ChangeStart(endFunction);
			currentScreenChanger = result;
		}
		else //스크린 체인저가 없어.. 그냥 함수 실행하고 땡 쳐야겠다!
		{
			endFunction?.Invoke();
		}
	}
	public static void ClaimScreenChangeEffectStart(ScreenChangeType wantType, System.Action endFunction = null) 
		=> instance?.ScreenChangeEffectStart(wantType, endFunction);


	public static void ClaimScreenChangeEffect(ScreenChangeType wantType, System.Action endFunction = null)
		=> instance?.ScreenChangeEffectStart(wantType, endFunction + ClaimScreenChangeEffectEnd);


	protected void ScreenChangeEffectEnd()
	{
		if (!currentScreenChanger) return;
		GameObject targetObject = currentScreenChanger.gameObject;
		currentScreenChanger.ChangeEnd(() => targetObject.SetActive(false));
		currentScreenChanger = null;
	}
	public static void ClaimScreenChangeEffectEnd()								=> instance?.ScreenChangeEffectEnd();


	public static void ClaimPopUp(string title, string context, string confirm)
	{
		OnPopUp?.Invoke(title, context, confirm);
	}
	public static void ClaimErrorMessage(string context)
	{
		OnPopUp?.Invoke("Error", context, "Confirm");
	}
}
