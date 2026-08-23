using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public delegate void SetCameraBoundEvent(Camera targetCamera, in Rect currentRect, ref Rect resultRect, in Vector3 currentInitialPosition, ref Vector3 resultInitialPosition);
public delegate void CameraPositionChangeEvent(Camera targetCamera, Vector3 newPosition);

public class CameraManager : ManagerBase
{
    public static SetCameraBoundEvent OnSetCameraBound;
	public static CameraPositionChangeEvent OnCameraPositionChanged;

	static Camera _mainCamera;
    public static Camera MainCamera
    {
        get => _mainCamera;
        private set
        {
            _mainCamera = value;
			if (!_mainCamera)
			{
				MainTransform = null;
				return;
			}
			else
			{
				MainTransform = _mainCamera.transform;
				mainCameraRect.position = MainTransform.position;
				UpdateMainCameraRectSize();
			}
        }
    }

	public static void UpdateMainCameraRectSize()
	{
		if(MainCamera) UpdateMainCameraRectSize(MainCamera.orthographicSize);
	}

	public static void UpdateMainCameraRectSizeWithoutNotify(float newSize)
	{
		newSize *= 2.0f;
		mainCameraRect.size = new(newSize * MainCamera.aspect, newSize);
	}

	public static void UpdateMainCameraRectSize(float newSize)
    {
		UpdateMainCameraRectSizeWithoutNotify(newSize);
		ClaimCalculateCameraBound();
		CameraInBound();
    }

    public static Transform MainTransform { get; private set; }

    public Vector3 cameraMoveDirection;
    public float cameraMoveSpeed = 10;

    public readonly static (int min, int max) defaultCameraSizeRange = (3, 8);
    public static (int min, int max) cameraSizeRange = defaultCameraSizeRange;

    public const float defaultCameraSize = 5;
    public static float cameraInitialSize = defaultCameraSize;

    public readonly static Vector3 defaultCameraOffset = Vector3.back * 10.0f;
    public readonly static Vector3 defaultCameraPosition = defaultCameraOffset;
    static Vector3 cameraInitialPositionOrigin;
    public static Vector3 cameraInitialPositionResult = defaultCameraPosition;

    static Rect cameraBoundOrigin;
    public static Rect cameraBoundResult = new(0, 0, 100, 150);
    static Rect mainCameraRect;

    protected override IEnumerator OnConnected(GameManager newManager)
    {
        MainCamera = Camera.main;
        CameraInBound();
        InputManager.OnCameraMove -= ClaimCameraMove;
        InputManager.OnCameraMove += ClaimCameraMove;
        InputManager.OnCameraZoom -= ClaimCameraZoom;
        InputManager.OnCameraZoom += ClaimCameraZoom;
        InputManager.OnCameraReset -= ClaimCameraReset;
        InputManager.OnCameraReset += ClaimCameraReset;

        GameManager.OnUpdateManager -= CameraMove;
        GameManager.OnUpdateManager += CameraMove;
        yield return null;
    }

    protected override void OnDisconnected()
    {
        InputManager.OnCameraMove -= ClaimCameraMove;
        InputManager.OnCameraZoom -= ClaimCameraZoom;
        InputManager.OnCameraReset -= ClaimCameraReset;

        GameManager.OnUpdateManager -= CameraMove;
    }

    void ClaimCameraMove(Vector2 value)
    {
        cameraMoveDirection = value.normalized;
    }

    void ClaimCameraZoom(float value)
    {
        if (!MainCamera) return;
        float originSize = MainCamera.orthographicSize;
        float result = Mathf.Clamp(originSize - value, cameraSizeRange.min, cameraSizeRange.max);
		if (MainCamera.orthographicSize == result) return;
        MainCamera.orthographicSize = result;
        UpdateMainCameraRectSize(result);
    }

    public static void ClaimCameraSetting(Rect wantBoundary, Vector2 wantCameraInitialPosition, (int min, int max) wantCameraSizeRange, float wantCameraInitialSize = defaultCameraSize)
    {
		MainCamera.orthographicSize = cameraInitialSize = wantCameraInitialSize;
        cameraInitialPositionOrigin = (Vector3)wantCameraInitialPosition + defaultCameraOffset;
		cameraBoundOrigin = wantBoundary;
        cameraSizeRange = wantCameraSizeRange;

		ClaimCameraReset(false);
    }
    public static void ClaimCameraSetting(Rect wantBoundary, Vector2 wantCameraInitialPosition, float wantCameraInitialSize = defaultCameraSize) => ClaimCameraSetting(wantBoundary, wantCameraInitialPosition, defaultCameraSizeRange, wantCameraInitialSize);
    public static void ClaimCameraSetting(Rect wantBoundary) => ClaimCameraSetting(wantBoundary, wantBoundary.center, defaultCameraSizeRange);

    public static void ClaimCameraReset(bool value = false)
    {
        if (!MainCamera) return;
		MainCamera.orthographicSize = cameraInitialSize;
		UpdateMainCameraRectSizeWithoutNotify(cameraInitialSize);
		ClaimCalculateCameraBound();
		CameraMoveTo(cameraInitialPositionResult);
    }

	public static void ClaimCalculateCameraBound()
	{
		cameraBoundResult = cameraBoundOrigin;
		cameraInitialPositionResult = cameraInitialPositionOrigin;
		OnSetCameraBound?.Invoke(MainCamera, cameraBoundOrigin, ref cameraBoundResult, cameraInitialPositionOrigin, ref cameraInitialPositionResult);
	}

	void CameraMove(float deltaTime)
    {
        if (cameraMoveDirection.sqrMagnitude < float.Epsilon || !MainTransform) return;
        Vector3 cameraDelta = deltaTime * cameraMoveSpeed * cameraMoveDirection;
        Vector3 resultPosition = MainTransform.position + cameraDelta;
        CameraMoveTo(resultPosition);
    }

    public static void CameraMoveTo(Vector3 wantPosition)
    {
        if (!MainTransform) return;
        mainCameraRect.center = wantPosition;
        wantPosition += (Vector3)mainCameraRect.InversedAABB(cameraBoundResult);
        mainCameraRect.center = MainTransform.position = wantPosition;
		OnCameraPositionChanged?.Invoke(MainCamera, wantPosition);

	}

    public static void CameraInBound()
    {
        if (!MainTransform) return;
        mainCameraRect.center = MainTransform.position;
        mainCameraRect.center = MainTransform.position += (Vector3)mainCameraRect.InversedAABB(cameraBoundResult);
		OnCameraPositionChanged?.Invoke(MainCamera, MainTransform.position);
	}


    public static void GetRaycastResult(Vector2 screenPosition, List<RaycastResult> outResult)
    {
        EventSystem currentEvent = EventSystem.current;
        if (!currentEvent) return;

        //현재 이벤트 시스템에서 무언가를 가져와줘야 함!
        PointerEventData eventData = new(currentEvent);
        eventData.position = screenPosition;
        //결과물은 왜 여러개가 나오나요?
        //뚫고 가야 하는 이유!
        //오버워치 => 아나 아마리 : 공격이 히트스캔 => 레이캐스트를 해서 맞은 대상에게 공격
        //                         앞에 아군 => 힐
        //                         앞에 적군 => 딜
        //앞에 풀피인 탱커가 알짱거리고 있어요! => 딜 넣고 싶음
        //풀피일 때에는 탱커 힐을 무시하고 뒤에 있는 적군에게 딜을 넣을 수 있어야 함!
        //공격 눌러놓고 NPC랑 몬스터랑 겹쳐있으면 => 몬스터를 때린다!
        //마우스 클릭하다가 갑자기 상대 위쪽으로 이펙트가 겹쳐서 이펙트가 클릭되면=>??
        currentEvent.RaycastAll(eventData, outResult);
    }

    public static Vector3 GetScreenPosition(Vector3 worldPosition) => MainCamera.WorldToScreenPoint(worldPosition);
    public static Vector3 GetWorldPosition(Vector3 screenPosition) => MainCamera.ScreenToWorldPoint(screenPosition);
}