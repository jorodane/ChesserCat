using UnityEngine;

public class UI_Button_ActionMove : UI_Button_PlayAction
{

    protected override void OnConnected(CharacterBase target)
    {
        base.OnConnected(target);
        SetActivatable(TileManager.HasLegalMove(target));
    }

    public override bool SetActivatable(bool value)
    {
        gameObject.SetActive(value);
        return value;
    }

	protected override void OnActivated()
	{
		base.OnActivated();
		InputManager.ClaimCommandMove(true);
	}
}
