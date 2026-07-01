using System;
using UnityEngine;
using UnityEngine.UI;

public class UI_IngameAreaVisalizer : UIBase, IOpenable
{
    [SerializeField] Image coverImage;
    public bool IsOpen => coverImage.enabled;
    public bool IsNeedClose => IsOpen;
    public void Close(bool isActiveByKey)
    {
        if(isActiveByKey) BattleManager.ClaimAnalasysModeEnd();
        else coverImage.enabled = false;
    }
    public void Open(bool isActiveByKey) => coverImage.enabled = true;
    public void Toggle(bool isActiveByKey) => coverImage.enabled = !coverImage.enabled;

    void OnEnable()
    {
        TileManager.SetTileOffsetVisual(Camera.main.ScreenToWorldPoint(transform.position));
        BattleManager.OnAnalysisModeChange -= OnAnalysisModeChange;
        BattleManager.OnAnalysisModeChange += OnAnalysisModeChange;
    }

    void OnDisable()
    {
        BattleManager.OnAnalysisModeChange -= OnAnalysisModeChange;
    }

    void OnAnalysisModeChange(bool value)
    {
        if (value) Open(false);
        else Close(false);
    }
}
