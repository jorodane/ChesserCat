using UnityEngine;

public class UI_TurnShower : UIBase
{
    [SerializeField] string turnLayoutPrefab;

    public override void Registration(UIManager manager)
    {
        base.Registration(manager);
        BattleManager.OnTurnAdded -= OnTurnAdded;
        BattleManager.OnTurnAdded += OnTurnAdded;
    }

    public override void Unregistration(UIManager manager)
    {
        base.Unregistration(manager);
        BattleManager.OnTurnAdded -= OnTurnAdded;
    }

    void OnTurnAdded(int wantIndex, in TurnBaseInfo wantTurnInfo)
    {
        GameObject instance = ObjectManager.CreateObject(turnLayoutPrefab, transform);
        if (!instance) return;

        if(instance.TryGetComponent(out UI_TurnLayout asLayout))
        {
            asLayout.SetTurn(wantIndex, wantTurnInfo);
        }
    }
}
