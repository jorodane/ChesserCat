using System;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

public class UI_TurnShower : UIBase
{
    [SerializeField] UI_ActiveLineShower activeLineShower;
    [SerializeField] string turnLayoutPrefab;

    public override void Registration(UIManager manager)
    {
        base.Registration(manager);
        if (activeLineShower) activeLineShower.Close(false);
        BattleManager.OnTurnAdded -= OnTurnAdded;
        BattleManager.OnTurnAdded += OnTurnAdded;
        BattleManager.OnTurnReset -= OnTurnReset;
        BattleManager.OnTurnReset += OnTurnReset;
        BattleManager.OnTurnIndexChanged -= OnTurnIndexChanged;
        BattleManager.OnTurnIndexChanged += OnTurnIndexChanged;
    }

    public override void Unregistration(UIManager manager)
    {
        base.Unregistration(manager);
        BattleManager.OnTurnAdded -= OnTurnAdded;
        BattleManager.OnTurnReset -= OnTurnReset;
        BattleManager.OnTurnIndexChanged -= OnTurnIndexChanged;
    }

    void OnTurnReset()
    {
        while(transform.childCount > 0)
        {
            Transform currentChild = transform.GetChild(0);
            currentChild.SetParent(null);
            ObjectManager.DestroyObject(currentChild.gameObject);
        }
    }

    void OnTurnAdded(int wantIndex, in TurnBaseInfo wantTurnInfo)
    {
        GameObject instance = ObjectManager.CreateObject(turnLayoutPrefab, transform);
        if (!instance) return;

        if(instance.TryGetComponent(out UI_TurnLayout asLayout))
        {
            asLayout.SetTurn(wantIndex, wantTurnInfo);
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
    }

    void OnTurnIndexChanged(int newIndex)
    {
        if (!activeLineShower) return;
        int childIndex = newIndex + 1;
        if (0 < childIndex && childIndex < transform.childCount)
        {
            Transform targetTurnObject = transform.GetChild(childIndex);
            if (targetTurnObject) activeLineShower.ConnectTurn(targetTurnObject.GetComponent<UI_TurnLayout>());
            else activeLineShower.DisconnectTurn();
        }
        else activeLineShower.DisconnectTurn();
        if (activeLineShower.IsTurnConnected) activeLineShower.Open(false);
        else activeLineShower.Close(false);
    }
}
