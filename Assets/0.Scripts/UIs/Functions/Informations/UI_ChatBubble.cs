using System.Collections;
using TMPro;
using UnityEngine;

public delegate void BubbleDestroyEvent(GameObject oldObject);
public class UI_ChatBubble : UI_ObjectFollowUI
{
	public event BubbleDestroyEvent OnBubbleDestroy;

	[SerializeField] TextMeshProUGUI nameText;
	[SerializeField] TextMeshProUGUI contextText;
	[SerializeField] Animator anim;
	[SerializeField] GameObject nextGuide;

	IEnumerator playingCoroutine;

	bool started = false;
	bool closing = false;

	protected override void OnDisable()
	{
		base.OnDisable();
		UnsetObject();
		closing = false;
		started = false;
	}

	protected override void OnUnsetObject(GameObject oldObject)
	{
		base.OnUnsetObject(oldObject);
		OnBubbleDestroy?.Invoke(oldObject);
		OnBubbleDestroy = null;
	}

	public void SetText(in string nameTag, in string context)
	{
		if(playingCoroutine is not null) StopCoroutine(playingCoroutine);
		if (started && anim) anim.SetTrigger("Continue");
		if (nameText) nameText.SetText(nameTag);
		if (contextText) contextText.SetText(context);
		SetAsLastSibiling();
		started = true;
		closing = false;
	}

	public void SetText(in ChatData data) => SetText(data.nameTag, data.context);

	public void SetNextGuide()
	{
		if(nextGuide) nextGuide.SetActive(true);
	}

	public void SetTimer(float closeTime) => PlayCoroutine(CloseAfterTimer(closeTime));
	public void Close()
	{
		if(started && !closing) PlayCoroutine(CloseWithAnimation());
	}


	void PlayCoroutine(IEnumerator newCoroutine)
	{
		if (playingCoroutine is not null) StopCoroutine(playingCoroutine);
		playingCoroutine = newCoroutine;
		StartCoroutine(playingCoroutine);
	}

	IEnumerator CloseAfterTimer(float closeTime)
	{
		nextGuide.SetActive(false);
        closing = false;
		yield return new WaitForSeconds(closeTime);
		yield return CloseWithAnimation();
	}

	IEnumerator CloseWithAnimation()
	{
		closing = true;
		if (started && anim)
		{
			anim.SetTrigger("Shrink");
			yield return new WaitForSeconds(.25f);
			started = false;
		}
		ObjectManager.DestroyObject(gameObject);
		closing = false;
	}
}
