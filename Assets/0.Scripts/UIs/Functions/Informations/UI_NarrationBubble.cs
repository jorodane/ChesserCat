using System.Collections;
using TMPro;
using UnityEngine;

public class UI_NarrationBubble : MonoBehaviour, ITextBubble
{
	public event BubbleDestroyEvent OnBubbleDestroy;

	[SerializeField] TextMeshProUGUI nameText;
	[SerializeField] TextMeshProUGUI contextText;
	[SerializeField] Animator anim;
	[SerializeField] GameObject nextGuide;

	IEnumerator playingCoroutine;

	bool started = false;
	bool closing = false;

	public void SetText(in string nameTag, in string context)
	{
		gameObject.SetActive(true);
		if (playingCoroutine is not null) StopCoroutine(playingCoroutine);
		if (anim)
		{
			if(started)	anim.SetTrigger("Refresh");
			else anim.SetTrigger("Appear");
		}
		if (nameText) nameText.SetText(nameTag);
		if (contextText) contextText.SetText(context);
		started = true;
		closing = false;
	}

	public void SetText(in ChatData data) => SetText(data.GetNameTag(), data.context);

	public void SetNextGuide()
	{
		if (nextGuide) nextGuide.SetActive(true);
	}

	public void SetTimer(float closeTime) => PlayCoroutine(CloseAfterTimer(closeTime));
	public void Close()
	{
		if (started && !closing) PlayCoroutine(CloseWithAnimation());
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
			anim.SetTrigger("Disappear");
			yield return new WaitForSeconds(.25f);
			started = false;
		}
		gameObject.SetActive(false);
		closing = false;
	}
}
