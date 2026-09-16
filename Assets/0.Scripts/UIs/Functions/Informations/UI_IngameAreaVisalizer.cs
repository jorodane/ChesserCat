using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public delegate void IngameAreaBlockEvent(string wantTag);

public class UI_IngameAreaVisalizer : UIBase, IOpenable
{
	public static event IngameAreaBlockEvent OnIngameAreaBlock;
	public static void ClaimIngameAreaBlock(string wantTag) => OnIngameAreaBlock?.Invoke(wantTag);
	public static event IngameAreaBlockEvent OnIngameAreaUnblock;
	public static void ClaimIngameAreaUnblock(string wantTag) => OnIngameAreaUnblock?.Invoke(wantTag);

	[SerializeField] GameObject analysisModeFilter;
    [SerializeField] Image cursorBlocker;
    RectTransform ingameRectTransform;
	List<string> blockTag = new();

    public bool IsOpen => blockTag.Count > 0;
    public bool IsNeedClose => IsOpen;
    public void Close(bool isActiveByKey)
    {
		if (isActiveByKey) BattleManager.ClaimAnalysisModeEnd();
	}
	public void Open(bool isActiveByKey){}

	public bool Toggle(bool isActiveByKey)
	{
		analysisModeFilter.SetActive(!analysisModeFilter.activeSelf);
		return analysisModeFilter.activeSelf;
	}

	public virtual void SetOpen(bool newOpen, bool isActiveByKey)
    {
        if (IsOpen == newOpen) return;
        if (newOpen) Open(isActiveByKey);
        else Close(isActiveByKey);
    }

	public void AddBlockTag(string newTag)
	{
		if (string.IsNullOrEmpty(newTag)) return;
		if(!blockTag.Contains(newTag)) blockTag.Add(newTag);
		cursorBlocker.enabled = IsOpen;
	}

	public void RemoveBlockTag(string oldTag)
	{
		blockTag.Remove(oldTag);
		cursorBlocker.enabled = IsOpen;
	}

    void OnEnable()
    {
        ingameRectTransform = transform as RectTransform;

		OnIngameAreaBlock -= AddBlockTag;
		OnIngameAreaBlock += AddBlockTag;
		OnIngameAreaUnblock -= RemoveBlockTag;
		OnIngameAreaUnblock += RemoveBlockTag;

		BattleManager.OnAnalysisModeChanged -= OnAnalysisModeChange;
        BattleManager.OnAnalysisModeChanged += OnAnalysisModeChange;
        BattleManager.OnAnimationModeChanged -= OnAnimationModeChange;
        BattleManager.OnAnimationModeChanged += OnAnimationModeChange;

		CameraManager.OnCameraLockChanged -= OnCameraLock;
		CameraManager.OnCameraLockChanged += OnCameraLock;
        CameraManager.OnSetCameraBound = CalculateCameraBoundary;
        CameraManager.CameraInBound();
    }

    void OnDisable()
    {
		OnIngameAreaBlock -= AddBlockTag;
		OnIngameAreaUnblock -= RemoveBlockTag;

		BattleManager.OnAnalysisModeChanged -= OnAnalysisModeChange;
        BattleManager.OnAnimationModeChanged -= OnAnimationModeChange;
		CameraManager.OnCameraLockChanged -= OnCameraLock;
        CameraManager.OnSetCameraBound = null;
	}

	void OnCameraLock(bool locked)
	{
		if(locked) AddBlockTag("CameraLock");
		else RemoveBlockTag("CameraLock");
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
		analysisModeFilter.SetActive(value);
		//if (value) AddBlockTag("AnalysisMode");
		//else RemoveBlockTag("AnalysisMode");
	}

    private void OnAnimationModeChange(bool value)
    {
		if (value) AddBlockTag("AnimationMode");
		else RemoveBlockTag("AnimationMode");
    }
}
