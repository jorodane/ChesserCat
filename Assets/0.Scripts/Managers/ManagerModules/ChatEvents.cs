using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;

[System.Serializable]
public struct ChatData
{
	public GameObject from;
	public CameraLockInfo? cameraLock;
	public string nameTag;
	public string context;
	public float lockTime;
}

[System.Serializable]
public struct ChatSequence : IEnumerable<ChatData>
{
	public List<ChatData> datas;

	public ChatSequence(params ChatData[] newDatas) => datas = newDatas.ToList();
	public readonly IEnumerator<ChatData> GetEnumerator() => datas.GetEnumerator();
	readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	public static implicit operator List<ChatData>(in ChatSequence target) => target.datas;
}

public delegate void TemporaryChatEvent(GameObject from, string nameTag, string context, float closeTime);
public delegate void MainChatSequenceEvent(in ChatSequence newSequence);
public delegate void MainChatDataEvent(in ChatData NewData);
public delegate void MainChatEndEvent();


public static class ChatEvents
{
	public static TemporaryChatEvent OnClaimTemporaryChat;
	public static void ClaimTemporaryChat(GameObject from, string nameTag, string context, float closeTime) => OnClaimTemporaryChat?.Invoke(from, nameTag, context, closeTime);

	public static MainChatDataEvent OnClaimMainChatData;
	public static void ClaimMainChatData(in ChatData newData) => OnClaimMainChatData?.Invoke(newData);

	public static MainChatSequenceEvent OnClaimMainChatSequence;
	public static void ClaimMainChatSequence(in ChatSequence newSequence) => OnClaimMainChatSequence?.Invoke(newSequence);

	public static MainChatEndEvent OnClaimMainChatEnd;
	public static void ClaimMainChatEnd() => OnClaimMainChatEnd?.Invoke();

	public static bool isMainChatMode = false;
}
