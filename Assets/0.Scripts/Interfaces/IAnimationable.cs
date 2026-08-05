using UnityEngine;

public interface IAnimationable
{
    public void AnimationByTrigger(AnimationTriggerType wantType);
    public void AnimationReset();
}
