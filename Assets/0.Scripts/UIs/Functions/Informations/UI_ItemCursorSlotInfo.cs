using System;
using UnityEngine;

public class UI_ItemCursorSlotInfo : UI_ItemSlotInfo
{
    //일반 슬롯과 다른점
    //1.마우스를 따라다닌다.
    //2.등록되는 시점이 그냥 시작했을 때

    public override void Registration(UIManager manager)
    {
        base.Registration(manager);
        ConnectSlot(Inventory.cursorSlot); //시작하면 커서에 연결됨!
        InputManager.OnMouseMove -= MoveToMouse;
        InputManager.OnMouseMove += MoveToMouse;
        InputManager.OnMouseLeftButton -= LeftButton;
        InputManager.OnMouseLeftButton += LeftButton;
        InputManager.OnMouseRightButton -= RightButton;
        InputManager.OnMouseRightButton += RightButton;
    }

    public override void Unregistration(UIManager manager)
    {
        base.Unregistration(manager);
        DisconnectSlot(); //끝나면 연결도 끝남!
        InputManager.OnMouseMove -= MoveToMouse;
        InputManager.OnMouseLeftButton -= LeftButton;
        InputManager.OnMouseRightButton -= RightButton;
    }

    void LeftButton(bool value, Vector2 screenPosition, Vector3 worldPosition)
    {
        if (!value) return;
        GameObject currentHover = InputManager.CursorHoverObject;
        if (!currentHover) return;

        if(currentHover.TryGetComponent(out UI_ItemSlotInfo currentSlotInfo))
        {
            ConnectedSlot?.LeftClick(currentSlotInfo.ConnectedSlot);
        }
    }

    void RightButton(bool value, Vector2 screenPosition, Vector3 worldPosition)
    {
        if (!value) return;
        GameObject currentHover = InputManager.CursorHoverObject;
        if (!currentHover) return;

        if (currentHover.TryGetComponent(out UI_ItemSlotInfo currentSlotInfo))
        {
            ConnectedSlot?.RightClick(currentSlotInfo.ConnectedSlot);
        }
    }

    void MoveToMouse(Vector2 screenPosition, Vector3 worldPosition)
    {
        transform.position = screenPosition;
    }
}
