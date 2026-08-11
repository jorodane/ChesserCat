using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
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
        if (resetAnim && effectedCharacter) effectedCharacter.AnimationReset();
    }

    public override void GoPrev(bool resetAnim)
    {
        if (!effectedCharacter) return;
        TileManager.PlaceObjectOnTile(effectedCharacter.gameObject, startLocation);
        if (resetAnim && effectedCharacter) effectedCharacter.AnimationReset();
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
        if (resetAnim && effectedCharacter) effectedCharacter.AnimationReset();

    }

    public override void GoPrev(bool resetAnim)
    {
        if (!effectedCharacter) return;
        TileManager.PlaceObjectOnTile(effectedCharacter.gameObject, startLocation);
        if (resetAnim && effectedCharacter) effectedCharacter.AnimationReset();
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

    public override void GoNext(bool resetAnim) 
    {
        //if (resetAnim && causeCharacter)
        //{
        //    causeCharacter.AnimationReset();
        //    causeCharacter.ResetPosition();
        //}
    }

    public override void GoPrev(bool resetAnim) 
    {
        //if (resetAnim && causeCharacter)
        //{
        //    causeCharacter.AnimationReset();
        //    causeCharacter.ResetPosition();
        //}
    }

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

    public override void GoNext(bool resetAnim)
    {
        if (resetAnim && effectedCharacter)
        {
            effectedCharacter.AnimationReset();
            effectedCharacter.ResetPosition();
        }
    }

    public override void GoPrev(bool resetAnim) 
    {
        if (resetAnim && effectedCharacter)
        {
            effectedCharacter.AnimationReset();
            effectedCharacter.ResetPosition();
        }
    }

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
public class TurnActionInfo_HealthChange : TurnActionInfo
{
    public CharacterBase causeCharacter;
    public int causeCharacterID;

    public CharacterBase effectedCharacter;
    public int effectedCharacterID;

    public int hpBefore;
    public int hpAfter;
    public int hpDelta;

    public override string ToString() => $"{causeCharacter?.DisplayInitial}d{effectedCharacter?.DisplayInitial}{hpDelta}";

    public TurnActionInfo_HealthChange(CharacterBase fromCharacter, CharacterBase wantCharacter, int delta)
    {
        causeCharacter = SetCharacter(fromCharacter, out causeCharacterID);
        effectedCharacter = SetCharacter(wantCharacter, out effectedCharacterID);
        hpDelta = delta;
        hpBefore = GetHP(wantCharacter, ref hpDelta, out hpAfter);
    }

    public int GetHP(CharacterBase targetCharacter, ref int delta, out int after)
    {
        HitPointModule hp = targetCharacter.GetModule<HitPointModule>();
        if (hp)
        {
            int origin = hp.GetCurrent();
            if (delta < 0)
            {
                delta = -Mathf.Min(-delta, origin);
                after = origin + delta;
                return origin;
            }
            else if(delta > 0)
            {
                delta = Mathf.Min(delta, hp.Fillable);
                after = origin + delta;
                return origin;
            }
            else
            {
                delta = 0;
                after = origin;
                return origin;
            }
        }
        else
        {
            delta = 0;
            after = 0;
            return 0;
        }
    }

    public override IEnumerable<HealthDeltaData> GetHealthDelta()
    {
        if (hpDelta == 0) yield break;
        yield return new() { character = effectedCharacter, delta = hpDelta };
    }

    public override void GoNext(bool resetAnim)
    {
        if (!effectedCharacter) return;
        SetTargetHP(hpAfter, false);
    }

    public override void GoPrev(bool resetAnim)
    {
        if (!effectedCharacter) return;
        SetTargetHP(hpBefore, false);
    }

    public void SetTargetHP(int targetHP, bool isAnimation)
    {
        HitPointModule module = effectedCharacter.GetModule<HitPointModule>();
        if(module) module.SetCurrent(targetHP, isAnimation);
    }

    public override IEnumerator Play()
    {
        yield break;
    }
}

[Serializable]
public class TurnActionInfo_Damage : TurnActionInfo_HealthChange
{
    public TurnActionInfo_Damage(CharacterBase fromCharacter, CharacterBase wantCharacter, int damage) : base(fromCharacter, wantCharacter, -damage) { }

    public override void GoNext(bool resetAnim)
    {
        base.GoNext(resetAnim);
        if (!effectedCharacter) return;
        if (resetAnim && effectedCharacter) effectedCharacter.AnimationReset();
    }

    public override void GoPrev(bool resetAnim)
    {
        base.GoPrev(resetAnim);
        if (!effectedCharacter) return;
        if (resetAnim && effectedCharacter) effectedCharacter.AnimationReset();
    }

    public override IEnumerator Play()
    {
        if (effectedCharacter)
        {
            SetTargetHP(hpAfter, true);
            if (hpDelta < 0)
            {
                effectedCharacter.AnimationTriggerNotify(AnimationTriggerType.Damaged);
                yield return new WaitForSeconds(0.5f);
            }
        }
    }
}

[Serializable]
public class TurnActionInfo_Restore : TurnActionInfo_HealthChange
{
    public TurnActionInfo_Restore(CharacterBase fromCharacter, CharacterBase wantCharacter, int heal) : base(fromCharacter, wantCharacter, heal){}

    public override IEnumerator Play()
    {
        if (effectedCharacter)
        {
            SetTargetHP(hpAfter, true);
            yield return new WaitForSeconds(0.5f);
        }
    }
}