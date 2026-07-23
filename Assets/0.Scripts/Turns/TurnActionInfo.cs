using System;
using System.Collections;
using UnityEngine;


[Serializable]
public abstract class TurnActionInfo
{
    public abstract void GoNext();
    public abstract void GoPrev();
    public abstract IEnumerator Play();

    public CharacterBase SetCharacter(in CharacterBase targetCharacter, out int targetCharacterID)
    {
        if(targetCharacter)
        {
            targetCharacterID = targetCharacter.GetID();
            return targetCharacter;
        }
        else
        {
            targetCharacterID = -1;
            return null;
        }
    }
}

[Serializable]
public class TurnActionInfo_Move : TurnActionInfo
{
    public Vector3Int startLocation;
    public Vector3Int actionLocation;
    public CharacterBase effectedCharacter;
    public int effectedCharacterID;

    public override string ToString() => $"{effectedCharacter?.DisplayInitial}{TileManager.GetTileText(actionLocation)}";

    public Vector3Int GetLocation(in CharacterBase targetCharacter, in Vector3Int defaultValue)
    {
        if (targetCharacter) return targetCharacter.CurrentTilePosition;
        return defaultValue;
    }

    public TurnActionInfo_Move(Vector3Int currentLocation, Vector3Int wantLocation, CharacterBase wantCharacter)
    {
        effectedCharacter = SetCharacter(wantCharacter, out effectedCharacterID);
        startLocation = currentLocation;
        actionLocation = wantLocation;
    }
    public TurnActionInfo_Move(Vector3Int wantLocation, CharacterBase wantCharacter)
    {
        effectedCharacter = SetCharacter(wantCharacter, out effectedCharacterID);
        startLocation = GetLocation(effectedCharacter, wantLocation);
        actionLocation = wantLocation;
    }

    public override void GoNext()
    {
        if (!effectedCharacter) return;
        TileManager.PlaceObjectOnTile(effectedCharacter.gameObject, actionLocation);
    }

    public override void GoPrev()
    {
        if (!effectedCharacter) return;
        TileManager.PlaceObjectOnTile(effectedCharacter.gameObject, startLocation);
    }

    public override IEnumerator Play()
    {
        if (effectedCharacter)
        {
            if (effectedCharacter.TryGetModule(out AnimationModule animation))
            {
                yield return animation.PlayMove(actionLocation);
            }
        }
    }
}

[Serializable]
public class TurnActionInfo_KnockBack : TurnActionInfo
{
    public Vector3Int startLocation;
    public Vector3Int actionLocation;
    public CharacterBase effectedCharacter;
    public int effectedCharacterID;

    public override string ToString() => $"{effectedCharacter?.DisplayInitial}b{TileManager.GetTileText(actionLocation)}";

    public Vector3Int GetLocation(in CharacterBase targetCharacter, in Vector3Int defaultValue)
    {
        if (targetCharacter) return targetCharacter.CurrentTilePosition;
        return defaultValue;
    }

    public TurnActionInfo_KnockBack(Vector3Int currentLocation, Vector3Int wantLocation, CharacterBase wantCharacter)
    {
        effectedCharacter = SetCharacter(wantCharacter, out effectedCharacterID);
        startLocation = currentLocation;
        actionLocation = wantLocation;
    }
    public TurnActionInfo_KnockBack(Vector3Int wantLocation, CharacterBase wantCharacter)
    {
        effectedCharacter = SetCharacter(wantCharacter, out effectedCharacterID);
        startLocation = GetLocation(effectedCharacter, wantLocation);
        actionLocation = wantLocation;
    }

    public override void GoNext()
    {
        if (!effectedCharacter) return;
        TileManager.PlaceObjectOnTile(effectedCharacter.gameObject, actionLocation);
    }

    public override void GoPrev()
    {
        if (!effectedCharacter) return;
        TileManager.PlaceObjectOnTile(effectedCharacter.gameObject, startLocation);
    }

    public override IEnumerator Play()
    {
        if (effectedCharacter)
        {
            if (effectedCharacter.TryGetModule(out AnimationModule animation))
            {
                yield return animation.PlayKnockBack(actionLocation);
            }
        }
    }
}


[Serializable]
public class TurnActionInfo_Kill : TurnActionInfo
{
    public CharacterBase causeCharacter;
    public int causeCharacterID;

    public CharacterBase effectedCharacter;
    public int effectedCharacterID;

    public Vector3Int startLocation;
    public Vector3Int actionLocation;

    public override string ToString() => $"{causeCharacter?.DisplayInitial}x{(TileManager.GetTileText(actionLocation))}";

    public TurnActionInfo_Kill(in Vector3Int fromLocation, CharacterBase fromCharacter, in Vector3Int wantLocation, CharacterBase wantCharacter)
    {
        causeCharacter = SetCharacter(fromCharacter, out causeCharacterID); 
        effectedCharacter = SetCharacter(wantCharacter, out effectedCharacterID);
        actionLocation = wantLocation;
        startLocation = fromLocation;
    }

    public override void GoNext()
    {
        if (!effectedCharacter) return;
        effectedCharacter.VisualizeKill();
    }

    public override void GoPrev()
    {
        if (!effectedCharacter) return;
        effectedCharacter.UnVisualizekill(actionLocation);
    }

    public override IEnumerator Play()
    {
        if (causeCharacter)
        {
            if (causeCharacter.TryGetModule(out AnimationModule animation))
            {
                yield return animation.PlayAttack(actionLocation, effectedCharacter);
                yield return new WaitForSeconds(0.5f);
                animation.AnimationReset();
            }
        }
    }
}

[Serializable]
public class TurnActionInfo_Damage : TurnActionInfo
{
    public CharacterBase causeCharacter;
    public int causeCharacterID;

    public CharacterBase effectedCharacter;
    public int effectedCharacterID;

    public int hpBefore;
    public int hpAfter;
    public int hpDelta;

    public override string ToString() => $"{causeCharacter?.DisplayInitial}d{effectedCharacter?.DisplayInitial}{hpDelta}";

    public int GetHP(CharacterBase targetCharacter)
    {
        HitPointModule hp = targetCharacter.GetModule<HitPointModule>();
        if (hp) return hp.Current;
        else    return 0;
    }

    public TurnActionInfo_Damage(CharacterBase fromCharacter, CharacterBase wantCharacter, int damage)
    {
        causeCharacter = SetCharacter(fromCharacter, out causeCharacterID);
        effectedCharacter = SetCharacter(wantCharacter, out effectedCharacterID);
        hpDelta = damage;
        hpBefore = GetHP(wantCharacter);
        hpAfter = hpBefore - damage;
    }

    public override void GoNext()
    {
        if (!effectedCharacter) return;
        effectedCharacter.GetModule<HitPointModule>().Current = hpAfter;
    }

    public override void GoPrev()
    {
        if (!effectedCharacter) return;
        effectedCharacter.GetModule<HitPointModule>().Current = hpBefore;
    }

    public override IEnumerator Play()
    {
        if (causeCharacter)
        {
            if (causeCharacter.TryGetModule(out AnimationModule animation))
            {
                yield return animation.PlayAttack(effectedCharacter);
                animation.AnimationReset();
                yield return animation.PlayReturn();
            }
        }
    }
}