using System;
using System.Collections;
using UnityEngine;

public abstract class ObjectiveBase : ScriptableObject
{
	public string objectiveContext;
	public ChatContainer sequenceOnStart;
	public ChatContainer sequenceOnClear;

	public abstract bool CheckClearCondition(TurnBaseInfo lastTurn, out GameObject clearClaimer);

	public virtual IEnumerator Start()
	{
		Initialize();
		yield return StartWithSequence();
	}
	protected IEnumerator StartWithSequence()
	{
		if (sequenceOnStart)
		{
			ChatEvents.ClaimMainChatContainer(null, sequenceOnStart);
			yield return new WaitUntilChatEnd();
		}
		yield return OnObjectiveStart();
	}

    protected virtual void Initialize()
    {

    }

    protected virtual IEnumerator OnObjectiveStart()
	{
		yield break;
	}

	public virtual void Dettach() => Dispose();

	public virtual IEnumerator Clear(TurnBaseInfo lastTurn, GameObject clearClaimer) => ClearWithSequence(lastTurn, clearClaimer);

	protected IEnumerator ClearWithSequence(TurnBaseInfo lastTurn, GameObject clearClaimer)
	{
		if (sequenceOnClear)
		{
			ChatEvents.ClaimMainChatContainer(clearClaimer, sequenceOnClear);
			yield return new WaitUntilChatEnd();
		}
		yield return OnObjectiveClear();
	}

	protected virtual void Dispose()
	{

	}

	protected virtual IEnumerator OnObjectiveClear()
	{
		yield break;
	}
}
