using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class UI_CharacterHoverPanel : UIBase
{
    UI_CharacterHoverInfo mouseHoverInfo;
    List<UI_CharacterHoverInfo> currentHoverInfoList = new();
    Dictionary<CharacterBase, int> hpDeltaDictionary = new();

    bool isShowCommit = false;

    public bool IsShowing() => currentHoverInfoList is not null && currentHoverInfoList.Count > 0;

    public override void Registration(UIManager manager)
    {
        base.Registration(manager);
        if(CreateHoverInfo(out mouseHoverInfo)) mouseHoverInfo.Close(false);

        InputManager.OnMouseHover -= HoverInfoChange;
        InputManager.OnMouseHover += HoverInfoChange;

        InputManager.OnShowStatus -= ShowAllCharacters;
        InputManager.OnShowStatus += ShowAllCharacters;

        UIManager.OnUIToggle -= MouseHoverToggleWithOther;
        UIManager.OnUIToggle += MouseHoverToggleWithOther;

        BattleManager.OnTurnSimulated -= SetTurnResult;
        BattleManager.OnTurnSimulated += SetTurnResult;
    }

    //해제
    public override void Unregistration(UIManager manager)
    {
        base.Unregistration(manager);
        InputManager.OnMouseHover -= HoverInfoChange;
        InputManager.OnShowStatus -= ShowAllCharacters;
        UIManager.OnUIToggle -= MouseHoverToggleWithOther;
        BattleManager.OnTurnSimulated -= SetTurnResult;
    }

    public void RecoverMouseHoverInfo()
    {
        if (!mouseHoverInfo || !mouseHoverInfo.HasCharacter()) return;
        if (UIManager.ClaimCheckOpen(UIType.CharacterClickInfo)) return;
        if (IsShowing()) return;

        mouseHoverInfo.Open(false);
    }

    public void TryCloseMouseHoverInfo()
    {
        if (!mouseHoverInfo || !IsShowing()) return;
        mouseHoverInfo.Close(false);
    }

    void MouseHoverToggleWithOther(UIType targetType, bool isOpen)
    {
        switch(targetType)
        {
            case UIType.CharacterClickInfo:
                if (isOpen) mouseHoverInfo.Close(true);
                else RecoverMouseHoverInfo();
                break;
        }
    }

    public bool CreateHoverInfo(out UI_CharacterHoverInfo createdInfo)
    {
        createdInfo = null;
        GameObject instance = ObjectManager.CreateObject("CharacterHoverInfo", transform);
        if (!instance) return false;
        bool result = instance.TryGetComponent(out createdInfo);
        if(result) createdInfo.Registration(UIManager.instance);
        return result;
    }

    void ShowAllCharacters(bool value)
    {
        HideAllCharacters();
        if(value) SetCharacters(PlayerController.Instance.GetAllCharacters(), true);
        isShowCommit = value;
    }

    public void HideAllCharacters()
    {
        if (currentHoverInfoList is not null)
        {
            foreach (UI_CharacterHoverInfo currentInfo in currentHoverInfoList.ToArray())
            {
                if (!currentInfo) continue;
                if(!hpDeltaDictionary.ContainsKey(currentInfo.Target))
                {
                    currentInfo.Unregistration(UIManager.instance);
                    ObjectManager.DestroyObject(currentInfo.gameObject);
                    currentHoverInfoList.Remove(currentInfo);
                }
            }
            RecoverMouseHoverInfo();
        }
    }

    public void HideAllHealthDelta()
    {
        if(isShowCommit)
        {
            foreach (UI_CharacterHoverInfo currentInfo in currentHoverInfoList.ToArray())
            {
                if (!currentInfo) continue;
                if (!hpDeltaDictionary.ContainsKey(currentInfo.Target)) continue;

                currentInfo.SetHPBarDelta(0);
            }
        }
        else
        {
            foreach (UI_CharacterHoverInfo currentInfo in currentHoverInfoList.ToArray())
            {
                if (!currentInfo) continue;
                if (!hpDeltaDictionary.ContainsKey(currentInfo.Target)) continue;
                currentInfo.Unregistration(UIManager.instance);
                ObjectManager.DestroyObject(currentInfo.gameObject);
                currentHoverInfoList.Remove(currentInfo);
            }
        }
        hpDeltaDictionary.Clear();
    }

    public void SetCharacters(IEnumerable<CharacterBase> targets, bool isSimple)
    {
        currentHoverInfoList ??= new();
        foreach (CharacterBase current in targets)
        {
            if (hpDeltaDictionary.ContainsKey(current)) continue;

            if (current)
            {
                if (!CreateHoverInfo(out UI_CharacterHoverInfo info)) return;
                SetCharacter(info, current, isSimple);
                info.SetHPBarDelta(0);
                currentHoverInfoList.Add(info);
            }
        }
        TryCloseMouseHoverInfo();
    }

    public void SetTurnResult(in TurnBaseInfo turn)
    {
        if (!isShowCommit) HideAllCharacters();
        if (turn is not null)
        {
            SetHealthDeltas(turn.GetHealthDelta());
        }
        else
        {
            HideAllHealthDelta();
        }
    }

    public void SetHealthDeltas(IEnumerable<HealthDeltaData> values)
    {
        HideAllHealthDelta();
        currentHoverInfoList ??= new();
        foreach (HealthDeltaData currentTuple in values)
        {
            CharacterBase currentCharacter = currentTuple.character;
            if (!currentCharacter) continue;
            if (hpDeltaDictionary.ContainsKey(currentCharacter))
            {
                hpDeltaDictionary[currentCharacter] += currentTuple.delta;
            }
            else
            {
                hpDeltaDictionary[currentCharacter] = currentTuple.delta;
            }
        }

        foreach (KeyValuePair<CharacterBase, int> currentTuple in hpDeltaDictionary)
        { 
            CharacterBase currentCharacter = currentTuple.Key;
            int currentDelta = currentTuple.Value;
            if (!CreateHoverInfo(out UI_CharacterHoverInfo info)) return;
            SetCharacter(info, currentCharacter, true);
            info.SetHPBarDelta(currentDelta);
            currentHoverInfoList.Add(info);
        }

        TryCloseMouseHoverInfo();
    }

    public void SetCharacter(UI_CharacterHoverInfo info, CharacterBase character, bool isSimple)
    {
        if (!info) return;
        if (character) info.OpenWithCharacter(character, isSimple);
        else info.Close(false);
    }

    void HoverInfoChange(GameObject newTarget, GameObject oldTarget)
    {
        if (!mouseHoverInfo) return;
        if (newTarget)
        {
            SetCharacter(mouseHoverInfo, newTarget.GetComponent<CharacterBase>(), false);
            mouseHoverInfo.SetHPBarDelta(0);
            TryCloseMouseHoverInfo();
        }
        else
        {
            mouseHoverInfo.UnSetCharacter();
            mouseHoverInfo.Close(false);
        }
    }
}
