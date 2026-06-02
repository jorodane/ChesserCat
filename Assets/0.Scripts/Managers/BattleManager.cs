using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct TurnInfo
{


}

public class BattleManager : ManagerBase
{
    static List<ControllerBase> players;

	protected override IEnumerator OnConnected(GameManager newManager)
	{
        players = new List<ControllerBase>();
		yield return null;
	}

	protected override void OnDisconnected()
	{

	}

    public static void AddPlayerOnBattle(ControllerBase newPlayer)
    {
        if(newPlayer && !players.Contains(newPlayer))
        {
            players.Add(newPlayer);
        }
    }
    public static void RemovePlayerOnBattle(ControllerBase newPlayer)
    {
        players.Remove(newPlayer);
    }
}
