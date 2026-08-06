using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public abstract class TurnActionInfo
{
    public abstract void GoNext(bool resetAnim);
    public abstract void GoPrev(bool resetAnim);
    public abstract IEnumerator Play();

    public IAnimationable currentAnimator;

    public CharacterBase SetCharacter(in CharacterBase targetCharacter, out int targetCharacterID)
    {
        if (targetCharacter)
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
    public virtual IEnumerable<HealthDeltaData> GetHealthDelta()
    {
        yield break;
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

    public override void GoNext(bool resetAnim)
    {
        if (!effectedCharacter) return;
        TileManager.PlaceObjectOnTile(effectedCharacter.gameObject, actionLocation);
        if(resetAnim) effectedCharacter.AnimationReset();
        effectedCharacter.AnimationReset();

    }

    public override void GoPrev(bool resetAnim)
    {
        if (!effectedCharacter) return;
        TileManager.PlaceObjectOnTile(effectedCharacter.gameObject, startLocation);
        if(resetAnim) effectedCharacter.AnimationReset();
        effectedCharacter.AnimationReset();

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

    public override void GoNext(bool resetAnim)
    {
        if (!effectedCharacter) return;
        TileManager.PlaceObjectOnTile(effectedCharacter.gameObject, actionLocation);
        if(resetAnim) effectedCharacter.AnimationReset();
        effectedCharacter.AnimationReset();

    }

    public override void GoPrev(bool resetAnim)
    {
        if (!effectedCharacter) return;
        TileManager.PlaceObjectOnTile(effectedCharacter.gameObject, startLocation);
        if(resetAnim) effectedCharacter.AnimationReset();
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

    public override void GoNext(bool resetAnim)
    {
        if (!effectedCharacter) return;
        TileManager.RemoveObjectOnTile(effectedCharacter.gameObject, actionLocation);
        if (resetAnim) effectedCharacter.VisualizeOut();
    }

    public override void GoPrev(bool resetAnim)
    {
        if (!effectedCharacter) return;
        TileManager.PlaceObjectOnTile(effectedCharacter.gameObject, actionLocation);
        if (resetAnim)
        {
            effectedCharacter.UnVisualizeOut(actionLocation);
            effectedCharacter.AnimationReset();
        }
    }

    public override IEnumerator Play()
    {
        if (effectedCharacter)
        {
            if (effectedCharacter.TryGetModule(out AnimationModule animation))
            {
                yield return animation.PlayOut();
            }
        }
    }
}

[Serializable]
public class TurnActionInfo_Attack : TurnActionInfo
{
    public CharacterBase causeCharacter;
    public int causeCharacterID;

    public CharacterBase effectedCharacter;
    public int effectedCharacterID;

    public Vector3Int startLocation;
    public Vector3Int actionLocation;

    public override string ToString() => $"{causeCharacter?.DisplayInitial}{TileManager.GetTileText(actionLocation)}";

    public TurnActionInfo_Attack(CharacterBase fromCharacter, CharacterBase wantCharacter)
    {
        causeCharacter = SetCharacter(fromCharacter, out causeCharacterID);
        effectedCharacter = SetCharacter(wantCharacter, out effectedCharacterID);
    }

    public override void GoNext(bool resetAnim) { }

    public override void GoPrev(bool resetAnim) { }

    public override IEnumerator Play()
    {
        if (causeCharacter)
        {
            if (causeCharacter.TryGetModule(out AnimationModule animation))
            {
                yield return animation.PlayAttack(effectedCharacter);
            }
        }
    }
}

[Serializable]
public class TurnActionInfo_ReturnToCurrentTile : TurnActionInfo
{
    public CharacterBase effectedCharacter;
    public int effectedCharacterID;

    public override string ToString() => $"";

    public TurnActionInfo_ReturnToCurrentTile(CharacterBase wantCharacter)
    {
        effectedCharacter = SetCharacter(wantCharacter, out effectedCharacterID);
    }

    public override void GoNext(bool resetAnim) { }

    public override void GoPrev(bool resetAnim) { }

    public override IEnumerator Play()
    {
        if (effectedCharacter)
        {
            if (effectedCharacter.TryGetModule(out AnimationModule animation))
            {
                yield return animation.PlayMove(effectedCharacter.CurrentTilePosition);
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

    public TurnActionInfo_Damage(CharacterBase fromCharacter, CharacterBase wantCharacter, int damage)
    {
        causeCharacter = SetCharacter(fromCharacter, out causeCharacterID);
        effectedCharacter = SetCharacter(wantCharacter, out effectedCharacterID);
        hpDelta = -damage;
        hpBefore = GetHP(wantCharacter);
        hpAfter = hpBefore + hpDelta;
    }

    public int GetHP(CharacterBase targetCharacter)
    {
        HitPointModule hp = targetCharacter.GetModule<HitPointModule>();
        if (hp) return hp.Current;
        else return 0;
    }

    public override IEnumerable<HealthDeltaData> GetHealthDelta()
    {
        yield return new() { character = effectedCharacter, delta = hpDelta };
    }

    public override void GoNext(bool resetAnim)
    {
        if (!effectedCharacter) return;
        effectedCharacter.GetModule<HitPointModule>().Current = hpAfter;
        if(resetAnim) effectedCharacter.AnimationReset();

    }

    public override void GoPrev(bool resetAnim)
    {
        if (!effectedCharacter) return;
        effectedCharacter.GetModule<HitPointModule>().Current = hpBefore;
        if(resetAnim) effectedCharacter.AnimationReset();
    }

    public override IEnumerator Play()
    {
        if (effectedCharacter)
        {
            effectedCharacter.AnimationTriggerNotify(AnimationTriggerType.Damaged);
            yield return new WaitForSeconds(0.5f);
            //if (effectedCharacter.TryGetModule(out AnimationModule animation))
            //{
            //    yield return animation.PlayAttack(effectedCharacter);
            //    effectedCharacter.AnimationReset();
            //    yield return animation.PlayReturn();
            //    causeCharacter.AnimationReset();
            //}
        }
    }
}