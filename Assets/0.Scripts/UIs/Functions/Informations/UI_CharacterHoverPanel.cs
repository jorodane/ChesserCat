using System;
using System.Collections.Generic;
using UnityEngine;

public class UI_CharacterHoverPanel : UIBase
{
    UI_CharacterHoverInfo mouseHoverInfo;
    List<UI_CharacterHoverInfo> currentHoverInfoList = new();

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

        BattleManager.OnTurnSimulated -= ShowTurnSimulation;
        BattleManager.OnTurnSimulated += ShowTurnSimulation;
    }

    //해제
    public override void Unregistration(UIManager manager)
    {
        base.Unregistration(manager);
        InputManager.OnMouseHover -= HoverInfoChange;
        InputManager.OnShowStatus -= ShowAllCharacters;
        UIManager.OnUIToggle -= MouseHoverToggleWithOther;
        BattleManager.OnTurnSimulated -= ShowTurnSimulation;
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
    }

    void ShowTurnSimulation(in TurnBaseInfo simulatedTurnInfo)
    {
        SetTurnResult(simulatedTurnInfo);
    }

    public void HideAllCharacters()
    {
        if (currentHoverInfoList is null) return;
        foreach(UI_CharacterHoverInfo currentInfo in currentHoverInfoList)
        {
            if (!currentInfo) continue;
            currentInfo.Unregistration(UIManager.instance);
            ObjectManager.DestroyObject(currentInfo.gameObject);
        }
        currentHoverInfoList.Clear();
        RecoverMouseHoverInfo();
    }

    public void SetCharacters(IEnumerable<CharacterBase> targets, bool isSimple)
    {
        currentHoverInfoList ??= new();
        foreach (CharacterBase current in targets)
        {
            if(current)
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
        HideAllCharacters();
        if (turn is null) return;
        currentHoverInfoList ??= new();
        Dictionary<CharacterBase, int> affectedCharacters = new();
        foreach (HealthDeltaData currentTuple in turn.GetHealthDelta())
        {
            CharacterBase currentCharacter = currentTuple.character;
            if (!currentCharacter) continue;
            if (affectedCharacters.ContainsKey(currentCharacter))
            {
                affectedCharacters[currentCharacter] += currentTuple.delta;
            }
            else
            {
                affectedCharacters[currentCharacter] = currentTuple.delta;
            }
        }

        foreach (KeyValuePair<CharacterBase, int> currentTuple in affectedCharacters)
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
