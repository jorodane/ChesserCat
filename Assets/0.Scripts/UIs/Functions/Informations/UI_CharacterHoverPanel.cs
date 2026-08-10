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

        BattleManager.OnTurnPlayed -= SetTurnPlay;
        BattleManager.OnTurnPlayed += SetTurnPlay;
    }

    //해제
    public override void Unregistration(UIManager manager)
    {
        base.Unregistration(manager);
        InputManager.OnMouseHover -= HoverInfoChange;
        InputManager.OnShowStatus -= ShowAllCharacters;
        UIManager.OnUIToggle -= MouseHoverToggleWithOther;
        BattleManager.OnTurnSimulated -= SetTurnResult;

        BattleManager.OnTurnPlayed -= SetTurnPlay;
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

    public UI_CharacterHoverInfo GetHoverInfo(CharacterBase targetCharacter)
    {
        if(targetCharacter == null || currentHoverInfoList is null || currentHoverInfoList.Count == 0) return null;
        return currentHoverInfoList.Find(currentInfo => currentInfo.Target == targetCharacter);
    }

    public UI_CharacterHoverInfo GetOrCreateHoverInfo(CharacterBase targetCharacter, bool addToList, bool isSimple)
    {
        UI_CharacterHoverInfo result = GetHoverInfo(targetCharacter);
        if (!result && CreateHoverInfo(out result))
        {
            result.SetCharacter(targetCharacter);
            if(addToList) currentHoverInfoList.Add(result);
        }
        result.SetSimple(isSimple);
        return result;
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
        }
        RecoverMouseHoverInfo();
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
                UI_CharacterHoverInfo info = GetOrCreateHoverInfo(current, true, true);
                if (!info) return;
                info.transform.SetAsLastSibling();
                info.SetHPBarDelta(0);
            }
        }
        TryCloseMouseHoverInfo();
    }

    public void SetTurnResult(in TurnBaseInfo turn)
    {
        if (!isShowCommit) HideAllCharacters();
        if (turn is not null)
        {
            SetHealthDeltas(turn.GetHealthDelta(), true);
        }
        else
        {
            HideAllHealthDelta();
        }
    }

    public void SetTurnPlay(in TurnBaseInfo turn)
    {
        if (!isShowCommit) HideAllCharacters();
        if (turn is not null)
        {
            SetHealthDeltas(turn.GetHealthDelta(), false);
        }
        else
        {
            HideAllHealthDelta();
        }
    }

    public void SetHealthDeltas(IEnumerable<HealthDeltaData> values, bool showDelta)
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
                hpDeltaDictionary.Add(currentCharacter, currentTuple.delta);
            }
        }

        foreach (KeyValuePair<CharacterBase, int> currentTuple in hpDeltaDictionary)
        { 
            CharacterBase currentCharacter = currentTuple.Key;
            int currentDelta = showDelta ? currentTuple.Value : 0;
            UI_CharacterHoverInfo info = GetOrCreateHoverInfo(currentCharacter, true, true);
            if (!info) return;
            info.transform.SetAsLastSibling();
            info.SetHPBarDelta(currentDelta);
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

        CharacterBase newCharacter = newTarget ? newTarget.GetComponent<CharacterBase>() : null;
        if (newCharacter)
        {
            SetCharacter(mouseHoverInfo, newCharacter, false);
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
