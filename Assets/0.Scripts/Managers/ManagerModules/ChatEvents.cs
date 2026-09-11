using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;

[System.Serializable]
public struct ChatData
{
	[HideInInspector] 
	public GameObject from;
	public string context;
	[Header("Camera Lock")]
	public bool isCameraLock;
	public CameraLockInfo cameraLock;

	public readonly string GetNameTag()
	{
		if(from)
		{
			if (from.TryGetComponent(out CharacterBase fromCharacter)) return fromCharacter.DisplayName;
			return from.name;
		}
		return "???";
	}
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
public delegate void MainChatSequenceEvent(GameObject claimer, in ChatSequence newSequence);
public delegate void MainChatDataEvent(GameObject claimer, in ChatData NewData);
public delegate void MainChatEndEvent();


public static class ChatEvents
{
	public static TemporaryChatEvent OnClaimTemporaryChat;
	public static void ClaimTemporaryChat(GameObject from, string nameTag, string context, float closeTime) => OnClaimTemporaryChat?.Invoke(from, nameTag, context, closeTime);

	public static MainChatDataEvent OnClaimMainChatData;
	public static void ClaimMainChatData(GameObject claimer, in ChatData newData) => OnClaimMainChatData?.Invoke(claimer, newData);

	public static MainChatSequenceEvent OnClaimMainChatSequence;
	public static void ClaimMainChatSequence(GameObject claimer, in ChatSequence newSequence) => OnClaimMainChatSequence?.Invoke(claimer, newSequence);

	public static MainChatEndEvent OnClaimMainChatEnd;
	public static void ClaimMainChatEnd() => OnClaimMainChatEnd?.Invoke();

	public static bool isMainChatMode = false;
}
