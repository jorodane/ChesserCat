using System;
using UnityEngine;
using UnityEngine.UI;

public class UI_IngameAreaVisalizer : UIBase, IOpenable
{
    [SerializeField] Image analysisModeFilter;
    [SerializeField] Image cursorBlocker;
    RectTransform ingameRectTransform;
    public bool IsOpen => analysisModeFilter.enabled;
    public bool IsNeedClose => IsOpen;
    public void Close(bool isActiveByKey)
    {
        if(isActiveByKey) BattleManager.ClaimAnalysisModeEnd();
        else analysisModeFilter.enabled = false;
    }
    public void Open(bool isActiveByKey) => analysisModeFilter.enabled = true;
    public bool Toggle(bool isActiveByKey) => analysisModeFilter.enabled = !analysisModeFilter.enabled;
    public virtual void SetOpen(bool newOpen, bool isActiveByKey)
    {
        if (IsOpen == newOpen) return;
        if (newOpen) Open(isActiveByKey);
        else Close(isActiveByKey);
    }

    void OnEnable()
    {
        ingameRectTransform = transform as RectTransform;
        BattleManager.OnAnalysisModeChange -= OnAnalysisModeChange;
        BattleManager.OnAnalysisModeChange += OnAnalysisModeChange;
        BattleManager.OnAnimationModeChange -= OnAnimationModeChange;
        BattleManager.OnAnimationModeChange += OnAnimationModeChange;

        CameraManager.OnSetCameraBound = CalculateCameraBoundary;
        CameraManager.CameraInBound();
    }

    void OnDisable()
    {
        BattleManager.OnAnalysisModeChange -= OnAnalysisModeChange;
        BattleManager.OnAnimationModeChange -= OnAnimationModeChange;
        CameraManager.OnSetCameraBound = null;
    }

    void CalculateCameraBoundary(Camera targetCamera, in Rect currentRect, ref Rect resultRect, in Vector3 currentInitialPosition, ref Vector3 resultInitialPosition)
    {
        Vector3[] ingameWorldCorners = new Vector3[4];
        ingameRectTransform.GetWorldCorners(ingameWorldCorners);

        if (UIManager.GetMainCanvas()?.transform is not RectTransform mainCanvasRect) return;
        Vector3[] mainCanvasWorldCorners = new Vector3[4];
        mainCanvasRect.GetWorldCorners(mainCanvasWorldCorners);
        Vector2 screenMin = RectTransformUtility.WorldToScreenPoint(targetCamera, mainCanvasWorldCorners[0]);
        Vector2 screenMax = RectTransformUtility.WorldToScreenPoint(targetCamera, mainCanvasWorldCorners[2]);
        int screenWidth = Screen.width;
        int screenHeight = Screen.height;
        Rect visibleRect = Rect.MinMaxRect
        (
            screenMin.x / screenWidth,
            screenMin.y / screenHeight,
            screenMax.x / screenWidth,
            screenMax.y / screenHeight
        );
        float entireWidth = currentRect.width / visibleRect.width;
        float entireHeight = currentRect.height / visibleRect.height;
        resultRect.xMin = currentRect.xMin - entireWidth * visibleRect.xMin;
        resultRect.xMax = currentRect.xMax + entireWidth * (1f - visibleRect.xMax);
        resultRect.yMin = currentRect.yMin - entireHeight * visibleRect.yMin;
        resultRect.yMax = currentRect.yMax + entireHeight * (1f - visibleRect.yMax);
        Vector2 centerOffset = resultRect.center - currentRect.center;
        resultInitialPosition = currentInitialPosition + new Vector3(centerOffset.x, centerOffset.y, 0f);
    }

    void OnAnalysisModeChange(bool value)
    {
        if (value) Open(false);
        else Close(false);
    }

    private void OnAnimationModeChange(bool value)
    {
        cursorBlocker.enabled = value;
    }
}
