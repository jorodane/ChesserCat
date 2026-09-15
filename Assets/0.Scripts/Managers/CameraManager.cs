using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.EventSystems;

public delegate void SetCameraBoundEvent(Camera targetCamera, in Rect currentRect, ref Rect resultRect, in Vector3 currentInitialPosition, ref Vector3 resultInitialPosition);
public delegate void CameraPositionChangeEvent(Camera targetCamera, Vector3 newPosition);
public delegate void CameraLockEvent(CameraLockInfo info);
public delegate void CameraUnlockEvent(bool immediately);
public delegate void CameraReturnEvent(bool immediately);
public delegate void CameraLockChangeEvent(bool isLocked);

public enum CameraLockTargetType { Claimer, From, Tag }

[System.Serializable]
public struct CameraLockInfo
{
	[HideInInspector]
	public Transform lockTarget;

	public Vector3 lockPosition;
	public float zoomScale;
	public float lockDelay;
	public CameraLockTargetType lockTargetType;
	public string lockTargetTag;
	public bool isReturnToOrigin;

	public Vector3 GetLockPosition()
	{
		if (lockTarget)
		{
			lockPosition = lockTarget.position;
			lockPosition.z = CameraManager.cameraDistance;
		}
		return lockPosition;
	}

	public readonly GameObject LockTargetFinder(GameObject mainChatClaimer)
	{
		GameObject result;
		switch (lockTargetType)
		{
			case CameraLockTargetType.Claimer: result = mainChatClaimer; break;
			case CameraLockTargetType.Tag: result = BattleManager.GetObjectFromName(lockTargetTag); break;
			default: result = lockTarget ? lockTarget.gameObject : null; break;
		}
		return result;
	}
}


public class CameraManager : ManagerBase
{
    public static SetCameraBoundEvent OnSetCameraBound;
	public static CameraPositionChangeEvent OnCameraPositionChanged;

	public static CameraLockEvent OnCameraLocked;
	public static CameraUnlockEvent OnCameraUnlock;
	public static CameraReturnEvent OnCameraReturn;
	public static event CameraLockChangeEvent OnCameraLockChanged;
	public static void ClaimCameraLock(in CameraLockInfo info) => OnCameraLocked?.Invoke(info);
	public static void ClaimCameraUnlock(bool immediately) => OnCameraUnlock?.Invoke(immediately);
	public static void ClaimCameraReturn(bool immediately) => OnCameraReturn?.Invoke(immediately);


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

	CameraLockInfo? currentLock = null;
	IEnumerator currentLockCoroutine = null;
	Vector3? currentLockStartPosition = null;
	float? currentLockStartZoom = null;
	Vector3 currentLockEndPosition;
	float currentLockEndZoom;

	public static bool IsCameraLock
	{
		get
		{
			CameraManager instance = GameManager.Camera;
			if (!instance) return false;
			if (instance.currentLock is not null) return true;
			if (instance.currentLockCoroutine is not null) return true;
			return false;
		}
	}
	public const float cameraDistance = -10.0f;

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

    static Vector2 cameraBoundPadding;
    static Vector3 cameraCenterShifted;
    static Rect cameraBoundOrigin;
    public static Rect cameraBoundResult = new(0, 0, 100, 150);
    static Rect mainCameraRect;

    protected override IEnumerator OnConnected(GameManager newManager)
    {
        MainCamera = Camera.main;
        CameraInBound();
        OnCameraLocked -= CameraLock;
        OnCameraLocked += CameraLock;
		OnCameraUnlock -= CameraUnlock;
		OnCameraUnlock += CameraUnlock;
		OnCameraReturn -= CameraReturnToOrigin;
		OnCameraReturn += CameraReturnToOrigin;

        InputManager.OnCameraMove -= CameraMoveInput;
        InputManager.OnCameraMove += CameraMoveInput;
        InputManager.OnCameraZoom -= CameraZoomInput;
        InputManager.OnCameraZoom += CameraZoomInput;
        InputManager.OnCameraReset -= CameraResetInput;
        InputManager.OnCameraReset += CameraResetInput;

        GameManager.OnUpdateManager -= CameraMoveUpdateByInput;
        GameManager.OnUpdateManager += CameraMoveUpdateByInput;
        yield return null;
    }


	protected override void OnDisconnected()
    {
        OnCameraLocked -= CameraLock;
		OnCameraUnlock -= CameraUnlock;
		OnCameraReturn -= CameraReturnToOrigin;

		InputManager.OnCameraMove -= CameraMoveInput;
        InputManager.OnCameraZoom -= CameraZoomInput;
        InputManager.OnCameraReset -= CameraResetInput;

        GameManager.OnUpdateManager -= CameraMoveUpdateByInput;
	}

    void CameraMoveInput(Vector2 value)
    {
        cameraMoveDirection = value.normalized;
    }

    void CameraMoveUpdateByInput(float deltaTime)
    {
        if (IsCameraLock) return;
        if (cameraMoveDirection.sqrMagnitude < float.Epsilon || !MainTransform) return;
        Vector3 cameraDelta = deltaTime * cameraMoveSpeed * cameraMoveDirection;
        Vector3 resultPosition = MainTransform.position + cameraDelta;
        SetCameraPosition(resultPosition);
    }

    void CameraZoomInput(float wantSize)
    {
        if (IsCameraLock) return;
        CameraZoom(wantSize, InputManager.CursorWorldPosition);
    }
    public void CameraResetInput(bool byKey)
	{
        if (IsCameraLock) return;
		ClaimCameraReset();
    }

    float CameraPrepareZoom(float wantSize, in Vector3 pivot, out Vector3 cameraPositionResult)
	{
		cameraPositionResult = GetCameraPosition();
		if (!MainCamera) return wantSize;
		float originSize = MainCamera.orthographicSize;
		float result = Mathf.Clamp(originSize - wantSize, cameraSizeRange.min, cameraSizeRange.max);
		if (MainCamera.orthographicSize == result) return result;

		Vector3 origin = cameraPositionResult;
		Vector3 offset = origin - pivot;
		float ratio = result / originSize;
		cameraPositionResult = pivot + offset * ratio;
		UpdateMainCameraRectSize(result);
		return result;
	}


	void CameraZoom(float wantSize, in Vector3 pivot)
    {
        float result = CameraPrepareZoom(wantSize, pivot, out Vector3 cameraPositionResult);
        MainCamera.orthographicSize = result;
		SetCameraPosition(cameraPositionResult);
	}

	private void CameraUnlock(bool immediately)
	{
		if(currentLock.HasValue) currentLock = null;
		CameraReturnToOrigin(immediately);
		OnCameraLockChanged?.Invoke(false);
	}

	private void CameraReturnToOrigin(bool immediately)
	{
		if (currentLockCoroutine is not null) StopCoroutine(currentLockCoroutine);
		if(immediately)
		{
			UnlockNow();
		}
		else
		{
			currentLockCoroutine = UnlockSmooth(0.2f);
			StartCoroutine(currentLockCoroutine);
			OnCameraLockChanged?.Invoke(false);
		}
	}

	void CameraLock(CameraLockInfo info)
    {
		currentLock = info;
		Vector3 LockStartPosition = GetCameraPosition();

		currentLockStartPosition ??= LockStartPosition;
		currentLockStartZoom ??= GetCameraZoom();

		currentLockEndZoom = Math.Max(1.0f, info.zoomScale);
		currentLockEndPosition = info.GetLockPosition() + cameraCenterShifted * (currentLockEndZoom / cameraInitialSize);
		currentLockEndPosition.z = cameraDistance;
		if(currentLockCoroutine is not null) StopCoroutine(currentLockCoroutine);
		OnCameraLockChanged?.Invoke(true);
		currentLockCoroutine = LockSmooth(currentLockEndPosition, currentLockEndZoom, info.lockDelay, 0.0f);
		StartCoroutine(currentLockCoroutine);
	}

	IEnumerator CameraLock(Vector3 cameraPositionResult, float zoomResult, float wantTime, float stayTime)
	{
		Vector3 LockStartPosition = GetCameraPosition();

		currentLockStartPosition ??= LockStartPosition;
		currentLockStartZoom ??= GetCameraZoom();

		currentLockEndZoom = Math.Max(1.0f, zoomResult);
		currentLockEndPosition = cameraPositionResult + cameraCenterShifted * (currentLockEndZoom / cameraInitialSize);
		currentLockEndPosition.z = cameraDistance;
		if (currentLockCoroutine is not null) StopCoroutine(currentLockCoroutine);
		currentLockCoroutine = LockSmooth(currentLockEndPosition, currentLockEndZoom, wantTime, stayTime);
		OnCameraLockChanged?.Invoke(true);
		yield return currentLockCoroutine;
		currentLockCoroutine = null;
	}

	public void UnlockNow()
	{
		SetCameraPosition_Internal(currentLockStartPosition ?? GetCameraPosition());
		MainCamera.orthographicSize = currentLockStartZoom ?? GetCameraZoom();
		if(currentLockCoroutine is not null) StopCoroutine(currentLockCoroutine);
		currentLockCoroutine = null;
		currentLockStartPosition = null;
		currentLockStartZoom = null;
		currentLock = null;
		OnCameraLockChanged?.Invoke(false);
	}

	public static IEnumerator ClaimUnlockSmooth(float wantTime) => GameManager.Camera?.UnlockSmooth(wantTime);
	public static IEnumerator ClaimLockSmooth(Vector3 cameraPositionResult, float zoomResult, float wantTime, float stayTime) => GameManager.Camera?.CameraLock(cameraPositionResult, zoomResult, wantTime, stayTime);

	public IEnumerator UnlockSmooth(float wantTime)
	{
		Vector3 cameraPositionStart = GetCameraPosition();
		Vector3 cameraPositionEnd = currentLockStartPosition ?? cameraPositionStart;
		float zoomOrigin = GetCameraZoom();
		float zoomResult = currentLockStartZoom ?? zoomOrigin;
		if (wantTime > 0)
		{
			float startTime = Time.time;
			float endTime = startTime + wantTime;
			while (Time.time < endTime)
			{
				float percent = (Time.time - startTime) / wantTime;
				SetCameraPosition_Internal(Vector3.Lerp(cameraPositionStart, cameraPositionEnd, percent));
				MainCamera.orthographicSize = Mathf.Lerp(zoomOrigin, zoomResult, percent);
				yield return null;
			}
		}

		currentLockStartPosition = null;
		currentLockStartZoom = null;
		OnCameraLockChanged?.Invoke(false);
		OnSmoothCompleted(cameraPositionEnd, zoomResult);
	}




	public IEnumerator LockSmooth(Vector3 cameraPositionResult, float zoomResult, float wantTime, float stayTime)
	{
		float zoomOrigin = GetCameraZoom();
		if (wantTime > 0)
		{
			Vector3 cameraPositionStart = GetCameraPosition();
			float startTime = Time.time;
			float endTime = startTime + wantTime;
			while (Time.time < endTime)
			{
				float percent = (Time.time - startTime) / wantTime;
				SetCameraPosition_Internal(Vector3.Lerp(cameraPositionStart, cameraPositionResult, percent));
				MainCamera.orthographicSize = Mathf.Lerp(zoomOrigin, zoomResult, percent);
				yield return null;
			}
		}
		yield return new WaitForSeconds(stayTime);
		OnCameraLockChanged?.Invoke(true);
		OnSmoothCompleted(cameraPositionResult, zoomResult);
	}

	void OnSmoothCompleted(in Vector3 cameraPositionResult, float zoomResult)
	{
		SetCameraPosition_Internal(cameraPositionResult);
		MainCamera.orthographicSize = zoomResult;
		if(currentLockCoroutine is not null) StopCoroutine(currentLockCoroutine);
		currentLockCoroutine = null;
	}

	public static void ClaimCameraSetting(Rect wantBoundary, Vector2 wantCameraInitialPosition, (int min, int max) wantCameraSizeRange, float wantCameraInitialSize = defaultCameraSize)
    {
		MainCamera.orthographicSize = cameraInitialSize = wantCameraInitialSize;
        cameraInitialPositionOrigin = (Vector3)wantCameraInitialPosition + defaultCameraOffset;
		cameraBoundOrigin = wantBoundary;
        cameraSizeRange = wantCameraSizeRange;
    }
	public static void ClaimCameraSetting(Rect wantBoundary, Vector2 wantCameraInitialPosition, float wantCameraInitialSize = defaultCameraSize)
	{
		MainCamera.orthographicSize = cameraInitialSize = wantCameraInitialSize;
		cameraInitialPositionOrigin = (Vector3)wantCameraInitialPosition + defaultCameraOffset;
		cameraBoundOrigin = wantBoundary;
		cameraSizeRange = defaultCameraSizeRange;
	}

	public static void ClaimCameraSetting(Rect wantBoundary)
	{
        cameraInitialPositionOrigin = (Vector3)wantBoundary.center + defaultCameraOffset;
		cameraBoundOrigin = wantBoundary;
        cameraSizeRange = defaultCameraSizeRange;
	}

    public static void ClaimCameraReset()
    {
        if (!MainCamera) return;
		MainCamera.orthographicSize = cameraInitialSize;
		UpdateMainCameraRectSizeWithoutNotify(cameraInitialSize);
		ClaimCalculateCameraBound();
		CameraMoveTo(cameraInitialPositionOrigin);
    }

	public static void ClaimCalculateCameraBound()
	{
		cameraBoundResult = cameraBoundOrigin;
		cameraInitialPositionResult = cameraInitialPositionOrigin;
		OnSetCameraBound?.Invoke(MainCamera, cameraBoundOrigin, ref cameraBoundResult, cameraInitialPositionOrigin, ref cameraInitialPositionResult);
		cameraBoundPadding = cameraBoundResult.size - cameraBoundOrigin.size;
		cameraCenterShifted = cameraInitialPositionResult - cameraInitialPositionOrigin;
	}

	protected static Vector3 GetCameraPosition()
	{
		if(!MainCamera) return Vector3.zero;
		return MainCamera.transform.position;
	}

	protected static float GetCameraZoom()
	{
		if (MainCamera) return MainCamera.orthographicSize;
		return cameraInitialSize;
	}

	protected static Vector3 GetCameraClampedPosition(Vector3 wantPosition)
	{
		if (!MainTransform) return wantPosition;
		wantPosition.z = cameraDistance;
		mainCameraRect.center = wantPosition;
		Vector3 blockDistance = mainCameraRect.InversedAABB(cameraBoundResult);
		return wantPosition + blockDistance;
	}

	protected static void SetCameraPosition_Internal(Vector3 wantPosition)
	{
		mainCameraRect.center = MainTransform.position = wantPosition;
		OnCameraPositionChanged?.Invoke(MainCamera, wantPosition);
	}
	protected static void SetCameraPosition(Vector3 wantPosition)
	{
		SetCameraPosition_Internal(GetCameraClampedPosition(wantPosition));
	}
	public static void CameraMoveTo(Vector3 wantPosition) => SetCameraPosition(wantPosition + cameraCenterShifted);

	public static void CameraMove(Vector3 direction) => SetCameraPosition(MainTransform.position + direction);

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