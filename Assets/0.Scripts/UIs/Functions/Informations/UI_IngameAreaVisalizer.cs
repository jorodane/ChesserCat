using System;
using UnityEngine;
using UnityEngine.UI;

public class UI_IngameAreaVisalizer : UIBase, IOpenable
{
    [SerializeField] Image analysisModeFilter;
    [SerializeField] Image cursorBlocker;
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
        TileManager.SetTileOffsetVisual(Camera.main.ScreenToWorldPoint(transform.position));
        BattleManager.OnAnalysisModeChange -= OnAnalysisModeChange;
        BattleManager.OnAnalysisModeChange += OnAnalysisModeChange;
        BattleManager.OnAnimationModeChange -= OnAnimationModeChange;
        BattleManager.OnAnimationModeChange += OnAnimationModeChange;
    }

    void OnDisable()
    {
        BattleManager.OnAnalysisModeChange -= OnAnalysisModeChange;
        BattleManager.OnAnimationModeChange -= OnAnimationModeChange;
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
