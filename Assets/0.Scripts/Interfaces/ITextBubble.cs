using UnityEngine;

public interface ITextBubble
{
	public void SetText(in string nameTag, in string context);
	public void SetText(in ChatData data);
	public void SetNextGuide();
	public void SetTimer(float closeTime);
	public void Close();
}
