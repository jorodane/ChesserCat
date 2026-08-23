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
		float screenWidth = Screen.width;
		float screenHeight = Screen.height;
		float screenToWorld = targetCamera.orthographicSize * 2.0f / targetCamera.pixelHeight;

		Vector3[] ingameCorners = new Vector3[4];
		ingameRectTransform.GetWorldCorners(ingameCorners);

		float left = ingameCorners[0].x;
		float right = screenWidth - ingameCorners[2].x;
		float down = ingameCorners[0].y;
		float top = screenHeight - ingameCorners[2].y;

		resultRect.xMin -= left * screenToWorld;
        resultRect.xMax += right * screenToWorld;
        resultRect.yMin -= down * screenToWorld;
        resultRect.yMax += top * screenToWorld;

		resultInitialPosition += (Vector3)(resultRect.center - currentRect.center);
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
