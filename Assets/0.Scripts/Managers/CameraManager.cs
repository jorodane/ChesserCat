using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraManager : ManagerBase
{
    static Camera _mainCamera;
    public static Camera MainCamera
    {
        get => _mainCamera;
        private set
        {
            _mainCamera = value;
            MainTransform = _mainCamera.transform;
            mainCameraRect.position = MainTransform.position;
            UpdateMainCameraRectSize(_mainCamera.orthographicSize);
        }
    }

    public static void UpdateMainCameraRectSize(float newSize)
    {
        newSize *= 2.0f;
        mainCameraRect.size = new(newSize * MainCamera.aspect, newSize);
    }

    public static Transform MainTransform { get; private set; }

    public Vector3 cameraMoveDirection;
    public float cameraMoveSpeed = 10;

    public float cameraInitialSize = 5;
    public (int min, int max) cameraSizeRange = (3, 8);
    public Vector3 cameraInitialPosition = Vector3.back * 10.0f;
    public Rect cameraBound = new(0, 0, 100, 150);
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
        MainCamera.orthographicSize = result;
        UpdateMainCameraRectSize(result);
        CameraInBound();
    }

    void ClaimCameraReset(bool value)
    {
        if (!MainCamera) return;
        MainCamera.orthographicSize = cameraInitialSize;
        CameraMoveTo(cameraInitialPosition);
    }

    void CameraMove(float deltaTime)
    {
        if (cameraMoveDirection.sqrMagnitude < float.Epsilon || !MainTransform) return;
        Vector3 cameraDelta = deltaTime * cameraMoveSpeed * cameraMoveDirection;
        Vector3 resultPosition = MainTransform.position + cameraDelta;
        CameraMoveTo(resultPosition);
    }

    public void CameraMoveTo(Vector3 wantPosition)
    {
        mainCameraRect.center = wantPosition;
        wantPosition += (Vector3)mainCameraRect.InversedAABB(cameraBound);
        mainCameraRect.position = MainTransform.position = wantPosition;
    }

    public void CameraInBound()
    {
        mainCameraRect.center = MainTransform.position;
        mainCameraRect.position = MainTransform.position += (Vector3)mainCameraRect.InversedAABB(cameraBound);
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
    public static Vector3 GetWorldPosition(Vector3 screenPosition) => MainCamera.WorldToScreenPoint(screenPosition);
}