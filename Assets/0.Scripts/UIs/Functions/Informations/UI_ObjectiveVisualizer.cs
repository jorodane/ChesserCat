using System;
using UnityEngine;

public class UI_ObjectiveVisualizer : UIBase
{
	[SerializeField] Animator anim;

	void OnEnable()
	{
		ObjectiveBase.OnObjectiveVisualize -= ObjectiveVisualize;
		ObjectiveBase.OnObjectiveVisualize += ObjectiveVisualize;
		BattleManager.OnObjectiveChanged -= ObjectiveChanged;
		BattleManager.OnObjectiveChanged += ObjectiveChanged;
	}

	void OnDisable()
	{
		ObjectiveBase.OnObjectiveVisualize -= ObjectiveVisualize;
		BattleManager.OnObjectiveChanged -= ObjectiveChanged;
	}

	void ObjectiveChanged(ObjectiveBase newObjective)
	{
		if (!newObjective) ObjectiveVisualize(newObjective, false);
	}

	void ObjectiveVisualize(ObjectiveBase newObjective, bool value)
	{
		anim.SetBool("Appear", value && newObjective);
	}
}
