using UnityEngine;

public partial class ChessMovementModule
{
    public void TileChecker_OnlyForward(ref TileCheckStruct tileChecker)
    {
        if (tileChecker.currentMoveInfo.IsForwardDirection) return;
        tileChecker.result = false;
        tileChecker.isStop = true;
    }

    public void TileChecker_Enterable(ref TileCheckStruct tileChecker)
    {
		if (!TileManager.GetTileEnterable(tileChecker.currentMoveInfo, out TileInfo targetTileInfo, out TileEnterException exception))
		{
			if (TileManager.IsValidExceptionOnPass(tileChecker.currentMoveInfo.moveType, exception))
			{
				tileChecker.isStop = true;
			}
			if (TileManager.IsValidExceptionOnEnter(tileChecker.currentMoveInfo.moveType, exception))
			{
				tileChecker.result = false;
				return;
			}
		}
        if (!tileChecker.isObjectPassed) tileChecker.isObjectPassed = targetTileInfo.characterOnTile != null || targetTileInfo.objectOnTile != null;
		tileChecker.accepter.Add(this);
		tileChecker.result &= true;
	}

    public void TileChecker_AttackableTile(ref TileCheckStruct tileChecker)
    {
        if (!TileManager.GetTileEnterable(tileChecker.currentMoveInfo, out _, out TileEnterException exception))
        {
			if (TileManager.IsValidExceptionOnPass(tileChecker.currentMoveInfo.moveType, exception))
			{
				tileChecker.isStop = true;
			}
			if (TileManager.IsValidExceptionOnEnter(tileChecker.currentMoveInfo.moveType, exception))
			{
				tileChecker.result &= GetIsAttackable(TileManager.GetObjectOnTile(tileChecker.currentMoveInfo.nextTile));
				return;
			}
		}
		tileChecker.accepter.Add(this);
		tileChecker.result &= true;
	}

	public void TileChecker_AttackableTargetValid(ref TileCheckStruct tileChecker)
	{
		TileBase targetTile = TileManager.GetTile(tileChecker.currentMoveInfo.nextTile);
		if (!targetTile)
		{
			tileChecker.result = false;
			return;
		}
		TileInfo targetTileInfo = targetTile.Info;
		GameObject attackTarget = targetTileInfo.objectOnTile;

		if (targetTileInfo.characterOnTile)
		{
			tileChecker.result &= GetIsAttackable(targetTileInfo.characterOnTile);
		}
		else if (targetTileInfo.nonCharacterOnTile)
		{
			tileChecker.result &= GetIsAttackable(attackTarget);
		}
		else
		{
			tileChecker.result = false;
		}

		if (tileChecker.result) tileChecker.accepter.Add(this);
	}

	public void TileChecker_MoveDistance(ref TileCheckStruct tileChecker)
    {
        if (!tileChecker.result) return;
        if (MovableDistance > 0 && tileChecker.currentMoveInfo.moveDistance > MovableDistance)
        {
            tileChecker.result = false;
            tileChecker.isStop = true;
        }
    }

    public void TileChecker_AttackDistance(ref TileCheckStruct tileChecker)
    {
        if (!tileChecker.result) return;
        if (AttackableDistance > 0 && tileChecker.currentMoveInfo.moveDistance > AttackableDistance)
        {
            tileChecker.result = false;
            tileChecker.isStop = true;
        }
    }
}
