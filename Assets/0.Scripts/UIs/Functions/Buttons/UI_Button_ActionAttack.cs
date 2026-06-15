using UnityEngine;

public class UI_Button_ActionAttack : UI_Button_PlayAction
{

    protected override void OnConnected(CharacterBase target)
    {
        base.OnConnected(target);
        SetActivatable(TileManager.HasLegalAttack(target));
    }

    public override bool SetActivatable(bool value)
    {
        gameObject.SetActive(value);
        return value;
    }

    protected override void OnActivated()
	{
		base.OnActivated();
		InputManager.ClaimCommandAttack(true);
	}
}
