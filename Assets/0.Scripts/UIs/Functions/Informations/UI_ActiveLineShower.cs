using UnityEngine;

public class UI_ActiveLineShower : OpenableUIBase
{
    UI_TurnLayout connectedTurn;

    public bool IsTurnConnected => connectedTurn;

    public void ConnectTurn(UI_TurnLayout wantTurn)
    {
        DisconnectTurn();
        connectedTurn = wantTurn;
        if(connectedTurn)
        {
            transform.localPosition = connectedTurn.transform.localPosition.y * Vector3.up;
        }
    }

    public void DisconnectTurn()
    {
        if (!connectedTurn) return;
        connectedTurn = null;
    }
}
