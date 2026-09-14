using System;
using System.Collections;
using UnityEngine;

public abstract class ObjectiveBase : ScriptableObject
{
	public string objectiveContext;
	public string sequenceOnStart;
	public string sequenceOnClear;

	public abstract bool ClearCheck();

	public virtual IEnumerator Start() => OnObjectiveStart();

	protected IEnumerator StartWithSequence()
	{
		if (!string.IsNullOrEmpty(sequenceOnStart))
		{
			ChatEvents.ClaimMainChatContainer(null, sequenceOnStart);
			yield return new WaitUntilChatEnd();
		}
		yield return OnObjectiveStart();
	}

	protected virtual IEnumerator OnObjectiveStart()
	{
		yield break;
	}


	public virtual IEnumerator Clear() => ClearWithSequence();
	protected IEnumerator ClearWithSequence()
	{
		if (!string.IsNullOrEmpty(sequenceOnStart))
		{
			ChatEvents.ClaimMainChatContainer(null, sequenceOnStart);
			yield return new WaitUntilChatEnd();
		}
		yield return OnObjectiveClear();
	}

	protected virtual IEnumerator OnObjectiveClear()
	{
		yield break;
	}
}
