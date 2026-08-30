using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;



[Serializable, SaveNameSet("Base.None")]
public abstract class TurnActionInfo : ISavable<ActionSaveData>
{
    Dictionary<string, string> saveDatas;

    public TurnActionInfo() { }
    public TurnActionInfo(ActionSaveData data) { LoadData(data); }

    public abstract void ConstructCustomSaveData(Dictionary<string, string> result);
    public abstract void ReceiveCustomSaveData(Dictionary<string, string> datas);
    public virtual void LoadData(in ActionSaveData data)
    {
        saveDatas = data.saveDataList?.GetDictionary();
        if(saveDatas is not null) ReceiveCustomSaveData(saveDatas);
    }
    public virtual ActionSaveData MakeSaveData() => new()
    {
        saveDataList = this.MakeCustomSaveData(),
        actionName = GetType().GetCustomAttribute<SaveNameSet>()?.Value
    };

    public abstract void GoNext(bool resetAnim);
    public abstract void GoPrev(bool resetAnim);
    public abstract IEnumerator Play();

    public IAnimationable currentAnimator;

    public virtual IEnumerable<HealthDeltaData> GetHealthDelta() { yield break; }

}

[Serializable, SaveNameSet("Base.Move")]
public class TurnActionInfo_Move : TurnActionInfo
{
    public Vector3Int startLocation;
    public Vector3Int actionLocation;
    public CharacterBase effectedCharacter;


    public override void ConstructCustomSaveData(Dictionary<string, string> result)
    {
        result["startLocation"] = startLocation.ToString();
        result["actionLocation"] = actionLocation.ToString();
        result["effectedCharacterID"] = effectedCharacter.GetID().ToString();
    }

    public override void ReceiveCustomSaveData(Dictionary<string, string> datas)
    {
        string currentData;
        if (datas.TryGetValue("startLocation", out currentData)) startLocation = currentData.GetVector3Int();
        if (datas.TryGetValue("actionLocation", out currentData)) actionLocation = currentData.GetVector3Int();
        if (datas.TryGetValue("effectedCharacterID", out currentData)) effectedCharacter = BattleManager.GetCharacterFromID(int.Parse(currentData));
    }

    public Vector3Int GetLocation(in CharacterBase targetCharacter, in Vector3Int defaultValue)
    {
        if (targetCharacter) return targetCharacter.CurrentTilePosition;
        return defaultValue;
    }

    public TurnActionInfo_Move(ActionSaveData data) { LoadData(data); }

    public TurnActionInfo_Move(Vector3Int currentLocation, Vector3Int wantLocation, CharacterBase wantCharacter)
    {
        effectedCharacter = wantCharacter;
        startLocation = currentLocation;
        actionLocation = wantLocation;
    }
    public TurnActionInfo_Move(Vector3Int wantLocation, CharacterBase wantCharacter)
    {
        effectedCharacter = wantCharacter;
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

[Serializable, SaveNameSet("Base.KnockBack")]
public class TurnActionInfo_KnockBack : TurnActionInfo
{
    public Vector3Int startLocation;
    public Vector3Int actionLocation;
    public CharacterBase effectedCharacter;


    public override void ConstructCustomSaveData(Dictionary<string, string> result)
    {
        result["startLocation"] = startLocation.ToString();
        result["actionLocation"] = actionLocation.ToString();
        result["effectedCharacterID"] = effectedCharacter.GetID().ToString();
	}

    public override void ReceiveCustomSaveData(Dictionary<string, string> datas)
    {
        string currentData;
        if (datas.TryGetValue("startLocation", out currentData)) startLocation = currentData.GetVector3Int();
        if (datas.TryGetValue("actionLocation", out currentData)) actionLocation = currentData.GetVector3Int();
        if (datas.TryGetValue("effectedCharacterID", out currentData)) effectedCharacter = BattleManager.GetCharacterFromID(int.Parse(currentData));
	}

    public Vector3Int GetLocation(in CharacterBase targetCharacter, in Vector3Int defaultValue)
    {
        if (targetCharacter) return targetCharacter.CurrentTilePosition;
        return defaultValue;
    }

    public TurnActionInfo_KnockBack(ActionSaveData data) { LoadData(data); }

    public TurnActionInfo_KnockBack(Vector3Int currentLocation, Vector3Int wantLocation, CharacterBase wantCharacter)
    {
        effectedCharacter = wantCharacter;
        startLocation = currentLocation;
        actionLocation = wantLocation;
    }
    public TurnActionInfo_KnockBack(Vector3Int wantLocation, CharacterBase wantCharacter)
    {
        effectedCharacter = wantCharacter;
        startLocation = GetLocation(effectedCharacter, wantLocation);
        actionLocation = wantLocation;
    }

    public override void GoNext(bool resetAnim)
    {
        if (!effectedCharacter) return;
        TileManager.PlaceObjectOnTile(effectedCharacter.gameObject, actionLocation);
        if (resetAnim) effectedCharacter.AnimationReset();

    }

    public override void GoPrev(bool resetAnim)
    {
        if (!effectedCharacter) return;
        TileManager.PlaceObjectOnTile(effectedCharacter.gameObject, startLocation);
        if (resetAnim) effectedCharacter.AnimationReset();
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


[Serializable, SaveNameSet("Base.Out")]
public class TurnActionInfo_Out : TurnActionInfo
{
    public CharacterBase causeCharacter;
    public CharacterBase effectedCharacter;
    public Vector3Int startLocation;
    public Vector3Int actionLocation;

    public override void ConstructCustomSaveData(Dictionary<string, string> result)
    {
        result["causeCharacterID"] = causeCharacter.GetID().ToString();
        result["effectedCharacterID"] = effectedCharacter.GetID().ToString();

		result["startLocation"] = startLocation.ToString();
        result["actionLocation"] = actionLocation.ToString();
    }

    public override void ReceiveCustomSaveData(Dictionary<string, string> datas)
    {
        string currentData;
        if (datas.TryGetValue("causeCharacterID", out currentData)) causeCharacter = BattleManager.GetCharacterFromID(int.Parse(currentData));
		if (datas.TryGetValue("effectedCharacterID", out currentData)) effectedCharacter =BattleManager.GetCharacterFromID(int.Parse(currentData));

		if (datas.TryGetValue("startLocation", out currentData)) startLocation = currentData.GetVector3Int();
        if (datas.TryGetValue("actionLocation", out currentData)) actionLocation = currentData.GetVector3Int();
    }

    public TurnActionInfo_Out(ActionSaveData data) { LoadData(data); }

    public TurnActionInfo_Out(in Vector3Int fromLocation, CharacterBase fromCharacter, in Vector3Int wantLocation, CharacterBase wantCharacter)
    {
        causeCharacter = fromCharacter; 
        effectedCharacter = wantCharacter;
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

[Serializable, SaveNameSet("Base.JumpAttackAnim")]
public class TurnActionInfo_JumpAttackAnim : TurnActionInfo
{
    public CharacterBase causeCharacter;
    public CharacterBase effectedCharacter;
    public Vector3Int startLocation;
    public Vector3Int actionLocation;

    public override void ConstructCustomSaveData(Dictionary<string, string> result)
    {
        result["causeCharacterID"] = causeCharacter.GetID().ToString();
        result["effectedCharacterID"] = effectedCharacter.GetID().ToString();

		result["startLocation"] = startLocation.ToString();
        result["actionLocation"] = actionLocation.ToString();
    }

    public override void ReceiveCustomSaveData(Dictionary<string, string> datas)
    {
        string currentData;
        if (datas.TryGetValue("causeCharacterID", out currentData)) causeCharacter = BattleManager.GetCharacterFromID(int.Parse(currentData));
        if (datas.TryGetValue("effectedCharacterID", out currentData)) effectedCharacter =  BattleManager.GetCharacterFromID(int.Parse(currentData));

		if (datas.TryGetValue("startLocation", out currentData)) startLocation = currentData.GetVector3Int();
        if (datas.TryGetValue("actionLocation", out currentData)) actionLocation = currentData.GetVector3Int();
    }

    public TurnActionInfo_JumpAttackAnim(ActionSaveData data) { LoadData(data); }

    public TurnActionInfo_JumpAttackAnim(CharacterBase fromCharacter, CharacterBase wantCharacter)
    {
        causeCharacter = fromCharacter;
        effectedCharacter = wantCharacter;
    }

    public override void GoNext(bool resetAnim) {}

    public override void GoPrev(bool resetAnim) {}

    public override IEnumerator Play()
    {
        if (causeCharacter)
        {
            if (causeCharacter.TryGetModule(out AnimationModule animation))
            {
                yield return animation.PlayJumpAttack(effectedCharacter);
            }
        }
    }
}

[Serializable, SaveNameSet("Base.ReturnToCurrentTile")]
public class TurnActionInfo_ReturnToCurrentTile : TurnActionInfo
{
    public CharacterBase effectedCharacter;
    public override void ConstructCustomSaveData(Dictionary<string, string> result)
    {
        result["effectedCharacterID"] = effectedCharacter.GetID().ToString();
	}

    public override void ReceiveCustomSaveData(Dictionary<string, string> datas)
    {
        string currentData;
        if (datas.TryGetValue("effectedCharacterID", out currentData)) effectedCharacter = BattleManager.GetCharacterFromID(int.Parse(currentData));
	}

    public TurnActionInfo_ReturnToCurrentTile(ActionSaveData data) { LoadData(data); }

    public TurnActionInfo_ReturnToCurrentTile(CharacterBase wantCharacter)
    {
        effectedCharacter = wantCharacter;
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


[Serializable, SaveNameSet("Base.HealthChange")]
public class TurnActionInfo_HealthChange : TurnActionInfo
{
    public CharacterBase causeCharacter;
    public CharacterBase effectedCharacter;
    public int hpBefore;
    public int hpAfter;
    public int hpDelta;

    public override void ConstructCustomSaveData(Dictionary<string, string> result)
    {
        result["causeCharacterID"] = causeCharacter.GetID().ToString();
        result["effectedCharacterID"] = effectedCharacter.GetID().ToString();
		result["hpBefore"] = hpBefore.ToString();
        result["hpAfter"] = hpAfter.ToString();
        result["hpDelta"] = hpDelta.ToString();
    }

    public override void ReceiveCustomSaveData(Dictionary<string, string> datas)
    {
        string currentData;
        if (datas.TryGetValue("causeCharacterID", out currentData)) causeCharacter = BattleManager.GetCharacterFromID(int.Parse(currentData));
        if (datas.TryGetValue("effectedCharacterID", out currentData)) effectedCharacter = BattleManager.GetCharacterFromID(int.Parse(currentData));

		if (datas.TryGetValue("hpBefore", out currentData)) hpBefore = int.Parse(currentData);
        if (datas.TryGetValue("hpAfter", out currentData)) hpAfter = int.Parse(currentData);
        if (datas.TryGetValue("hpDelta", out currentData)) hpDelta = int.Parse(currentData);
    }

    public TurnActionInfo_HealthChange(ActionSaveData data) { LoadData(data); }

    public TurnActionInfo_HealthChange(CharacterBase fromCharacter, CharacterBase wantCharacter, int delta)
    {
        causeCharacter = fromCharacter;
        effectedCharacter = wantCharacter;
        hpDelta = delta;
        hpBefore = GetHP(wantCharacter, ref hpDelta, out hpAfter);
    }

    public int GetHP(CharacterBase targetCharacter, ref int delta, out int after)
    {
        HitPointModule hp = targetCharacter.GetModule<HitPointModule>();
        if (hp)
        {
            int origin = hp.Current;
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
        SetTargetHP(hpAfter, false);
    }

    public override void GoPrev(bool resetAnim)
    {
        SetTargetHP(hpBefore, false);
    }

    public void SetTargetHP(int targetHP, bool isAnimation)
    {
        if (!effectedCharacter) return;
        HitPointModule module = effectedCharacter.GetModule<HitPointModule>();
        if (module) module.SetCurrent(targetHP, isAnimation);
    }

    public override IEnumerator Play()
    {
        yield break;
    }
}

[Serializable, SaveNameSet("Base.Damage")]
public class TurnActionInfo_Damage : TurnActionInfo_HealthChange
{
    public TurnActionInfo_Damage(ActionSaveData data) : base(data) {  }

    public TurnActionInfo_Damage(CharacterBase fromCharacter, CharacterBase wantCharacter, int damage) : base(fromCharacter, wantCharacter, -damage) { }

    public override void GoNext(bool resetAnim)
    {
        base.GoNext(resetAnim);
        if (resetAnim && effectedCharacter) effectedCharacter.AnimationReset();
    }

    public override void GoPrev(bool resetAnim)
    {
        base.GoPrev(resetAnim);
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

[Serializable, SaveNameSet("Base.Restore")]
public class TurnActionInfo_Restore : TurnActionInfo_HealthChange
{
    public TurnActionInfo_Restore(ActionSaveData data) : base(data) {  }

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