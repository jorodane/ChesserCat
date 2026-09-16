using System;
using System.Collections;
using UnityEngine;

public delegate void ObjectiveVisualizeEvent(ObjectiveBase newObjective, bool value);

public abstract class ObjectiveBase : ScriptableObject
{
	public static ObjectiveVisualizeEvent OnObjectiveVisualize;

	public string objectiveContext;
	public string sequenceOnStart;
	public string sequenceOnClear;

	public abstract bool CheckClearCondition(TurnBaseInfo lastTurn, out GameObject clearClaimer);

	public virtual IEnumerator Start()
	{
		Initialize();
		yield return StartWithSequence();
	}
	protected IEnumerator StartWithSequence()
	{
		if(DataManager.TryLoadDataFile(sequenceOnStart, out ChatContainer loadedSequence))
		{
			ChatEvents.ClaimMainChatContainer(null, loadedSequence);
			yield return new WaitUntilChatEnd();
		}
		OnObjectiveVisualize?.Invoke(this, true);
		yield return OnObjectiveStart();
		OnObjectiveVisualize?.Invoke(this, false);
	}

    protected virtual void Initialize()
    {

    }

    protected virtual IEnumerator OnObjectiveStart()
	{
		yield return new WaitForSeconds(3.0f);
	}

	public virtual void Dettach() => Dispose();

	public virtual IEnumerator Clear(TurnBaseInfo lastTurn, GameObject clearClaimer) => ClearWithSequence(lastTurn, clearClaimer);

	protected IEnumerator ClearWithSequence(TurnBaseInfo lastTurn, GameObject clearClaimer)
	{
		if (DataManager.TryLoadDataFile(sequenceOnClear, out ChatContainer loadedSequence))
		{
			ChatEvents.ClaimMainChatContainer(clearClaimer, loadedSequence);
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
