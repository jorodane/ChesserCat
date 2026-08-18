using System;
using TMPro;
using UnityEngine;

public class UI_TurnLayout : UIBase
{
    [SerializeField] TextMeshProUGUI indexText;
    [SerializeField] TextMeshProUGUI actionText;

    public void SetTurn(int wantIndex, in TurnBaseInfo wantTurnInfo)
    {
        indexText.text = (wantTurnInfo.turnIndex + 1).ToString();
        actionText.text = wantTurnInfo.turnContext;
    }
}
