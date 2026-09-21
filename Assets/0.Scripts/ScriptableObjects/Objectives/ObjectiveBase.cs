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
	public string sequenceOnFail;

	public abstract bool CheckClearCondition(TurnBaseInfo lastTurn, out GameObject clearClaimer);
	public virtual bool CheckFailCondition(TurnBaseInfo lastTurn, ControllerBase player, out GameObject clearClaimer)
	{
		clearClaimer = null;
		return player && !player.IsAnyCharacterAlive();
	}

	public void Skip()
	{
		ObjectiveStartComplish();
	}

	void OnSkipInput(bool value)
	{
		BattleManager.ClaimSkipObjectiveVisualize();
	}

	public void StartImmediately()
	{
		InputManager.OnGoFinalTurn -= OnSkipInput;
		Initialize();
		ObjectiveStartComplish();
	}

	public IEnumerator Start()
	{
		InputManager.OnGoFinalTurn -= OnSkipInput;
		Initialize();
		yield return StartWithSequence();
		ObjectiveStartComplish();
	}
	protected IEnumerator StartWithSequence()
	{
		if (DataManager.TryLoadDataFile(sequenceOnStart, out ChatContainer loadedSequence))
		{
			ChatEvents.ClaimMainChatContainer(null, loadedSequence);
			yield return new WaitUntilChatEnd();
		}
		OnObjectiveVisualize?.Invoke(this, true);
		InputManager.OnGoFinalTurn -= OnSkipInput;
		InputManager.OnGoFinalTurn += OnSkipInput;
		yield return OnObjectiveStart();
		InputManager.OnGoFinalTurn -= OnSkipInput;
		OnObjectiveVisualize?.Invoke(this, false);
	}

    protected virtual void Initialize()
    {

	}

	protected virtual IEnumerator OnObjectiveStart()
	{
		yield return new WaitForSeconds(3.0f);
	}

	protected virtual void ObjectiveStartComplish()
	{
		InputManager.OnGoFinalTurn -= OnSkipInput;
		OnObjectiveVisualize?.Invoke(this, false);
	}

	public virtual void Dettach() => Dispose();

	protected virtual void Dispose()
	{

	}

	public IEnumerator Clear(TurnBaseInfo lastTurn, GameObject clearClaimer)
	{
		yield return ClearWithSequence(lastTurn, clearClaimer);
		yield return OnObjectiveClear();
	}
	protected virtual IEnumerator ClearWithSequence(TurnBaseInfo lastTurn, GameObject clearClaimer)
	{
		if (DataManager.TryLoadDataFile(sequenceOnClear, out ChatContainer loadedSequence))
		{
			ChatEvents.ClaimMainChatContainer(clearClaimer, loadedSequence);
			yield return new WaitUntilChatEnd();
		}
	}

	protected virtual IEnumerator OnObjectiveClear()
	{
		yield break;
	}

	public IEnumerator Fail(TurnBaseInfo lastTurn, GameObject failClaimer)
	{
		yield return FailWithSequence(lastTurn, failClaimer);
		yield return OnObjectiveFail();
	}
	protected virtual IEnumerator FailWithSequence(TurnBaseInfo lastTurn, GameObject failClaimer)
	{
		if (DataManager.TryLoadDataFile(sequenceOnFail, out ChatContainer loadedSequence))
		{
			ChatEvents.ClaimMainChatContainer(failClaimer, loadedSequence);
			yield return new WaitUntilChatEnd();
		}
	}

	protected virtual IEnumerator OnObjectiveFail()
	{
		yield break;
	}
}
