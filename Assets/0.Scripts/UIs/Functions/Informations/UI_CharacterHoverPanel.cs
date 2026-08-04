using System;
using System.Collections.Generic;
using UnityEngine;

public class UI_CharacterHoverPanel : UIBase
{
    UI_CharacterHoverInfo mouseHoverInfo;
    List<UI_CharacterHoverInfo> currentHoverInfoList = new();

    public override void Registration(UIManager manager)
    {
        base.Registration(manager);
        if(CreateHoverInfo(out mouseHoverInfo)) mouseHoverInfo.Close(false);

        InputManager.OnMouseHover -= HoverInfoChange;
        InputManager.OnMouseHover += HoverInfoChange;

        InputManager.OnShowStatus -= ShowAllCharacters;
        InputManager.OnShowStatus += ShowAllCharacters;
    }

    //해제
    public override void Unregistration(UIManager manager)
    {
        base.Unregistration(manager);
        InputManager.OnMouseHover -= HoverInfoChange;
        InputManager.OnShowStatus -= ShowAllCharacters;
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
        if (mouseHoverInfo && mouseHoverInfo.HasCharacter()) mouseHoverInfo.Open(false);
    }

    public void SetCharacters(IEnumerable<CharacterBase> targets, bool isSimple)
    {
        if (currentHoverInfoList is null) currentHoverInfoList = new();
        foreach (CharacterBase current in targets)
        {
            if(current)
            {
                if (!CreateHoverInfo(out UI_CharacterHoverInfo info)) return;
                SetCharacter(info, current, isSimple);
                currentHoverInfoList.Add(info);
            }
        }
        if (mouseHoverInfo && currentHoverInfoList.Count > 0) mouseHoverInfo.Close(false);
    }

    public void SetCharacter(UI_CharacterHoverInfo info, CharacterBase character, bool isSimple)
    {
        if (!info) return;
        if (character) info.OpenWithCharacter(character, isSimple);
        else info.Close(false);
    }

    void HoverInfoChange(GameObject newTarget, GameObject oldTarget)
    {
        if (!newTarget)
        {
            mouseHoverInfo.UnSetCharacter();
            mouseHoverInfo.Close(false);
            return;
        }
        SetCharacter(mouseHoverInfo, newTarget.GetComponent<CharacterBase>(), false);

        if (currentHoverInfoList is not null && currentHoverInfoList.Count > 0)
        {
            mouseHoverInfo.Close(false);
        }
    }
}
