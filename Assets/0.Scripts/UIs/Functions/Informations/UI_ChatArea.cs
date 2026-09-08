using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct ChatData
{
	public GameObject from;
	public string nameTag;
	public string context;
}

public delegate void TemporaryChatEvent(GameObject from, string nameTag, string context, float closeTime);
public class UI_ChatArea : UIBase
{
	public static TemporaryChatEvent OnClaimTemporaryChat;
	public static void ClaimTemporaryChat(GameObject from, string nameTag, string context, float closeTime) => OnClaimTemporaryChat?.Invoke(from, nameTag, context, closeTime);

	public static bool isMainChatMode = false;

	IEnumerator<ChatData> mainChatSequence;
	ChatData? mainChatCurrent = null;

	[SerializeField] Image chatScreen;

	readonly Dictionary<GameObject, UI_ChatBubble> bubbleDictionary = new();

	public override void Registration(UIManager manager)
	{
		base.Registration(manager);
		OnClaimTemporaryChat -= CreateTemporaryChat;
		OnClaimTemporaryChat += CreateTemporaryChat;
		InputManager.OnCommandInfo -= Test;
		InputManager.OnCommandInfo += Test;
		isMainChatMode = false;
    }

    public override void Unregistration(UIManager manager)
	{
		base.Unregistration(manager);
		OnMainChatEnd();
		OnClaimTemporaryChat -= CreateTemporaryChat;
		InputManager.OnCommandInfo -= Test;
		bubbleDictionary.Clear();
    }

    public void OnMainChatStart()
	{
		InputManager.ClaimSelectByCharacter(null);
		InputManager.OnConfirm -= OnConfirm;
		InputManager.OnConfirm += OnConfirm;
		isMainChatMode = true;
        if (chatScreen) chatScreen.raycastTarget = isMainChatMode;
    }

	public void OnMainChatEnd()
	{
		if (!isMainChatMode) return;
		isMainChatMode = false;
		InputManager.OnConfirm -= OnConfirm;
        if (chatScreen) chatScreen.raycastTarget = isMainChatMode;
    }

    UI_ChatBubble GetOrCreateBubble(GameObject from)
	{
		if (bubbleDictionary.TryGetValue(from, out UI_ChatBubble targetBubble)) return targetBubble;
		UIBase instance = UIManager.ClaimCreateUI("ChatBubble", transform);
		if (instance.TryGetComponent(out targetBubble))
		{
			targetBubble.SetObject(from);
			targetBubble.OnBubbleDestroy -= OnBubbleDestroy;
			targetBubble.OnBubbleDestroy += OnBubbleDestroy;
			bubbleDictionary.Add(from, targetBubble);
		}
		return targetBubble;
	}

    void OnConfirm(bool value)
    {
        if (!value || GameManager.IsPaused) return;
		NextMainChatSequence();
    }

    void OnBubbleDestroy(GameObject oldObject)
	{
		bubbleDictionary.Remove(oldObject);
	}

	void CreateTemporaryChat(GameObject from, string nameTag, string context, float closeTime)
	{
		UI_ChatBubble targetBubble = GetOrCreateBubble(from);
		if (!targetBubble) return;
		targetBubble.SetText(nameTag, context);
		targetBubble.SetTimer(closeTime);
	}

	public void SetMainChatSequence(IEnumerable<ChatData> sequence)
	{
		EndMainChatSequence();
		mainChatSequence = sequence.GetEnumerator();
		NextMainChatSequence();
	}

	public void NextMainChatSequence()
	{
		if (mainChatSequence is null) return;
		if (!mainChatSequence.MoveNext())
		{
			EndMainChatSequence();
			return;
		}
		mainChatCurrent = mainChatSequence.Current;
		CreateMainChat(mainChatCurrent.Value);
	}

	public void EndMainChatSequence()
	{
		EndChat(mainChatCurrent);
		mainChatCurrent = null;
		mainChatSequence = null;
		OnMainChatEnd();
	}


	void CreateMainChat(in ChatData data)
	{
		UI_ChatBubble targetBubble = GetOrCreateBubble(data.from);
		if (!targetBubble) return;
		targetBubble.SetText(data);
		targetBubble.SetNextGuide();
		OnMainChatStart();
    }

	void EndChat(in ChatData? data)
	{
		if(data is null) return;
		UI_ChatBubble targetBubble = GetOrCreateBubble(data.Value.from);
		if (!targetBubble) return;
		targetBubble.Close();
	}


	void Test(bool value)
	{
		var chars = BattleManager.GetCharacters();
		CharacterBase selectedCharacter = chars[Random.Range(0, chars.Length)];
		if(selectedCharacter)
		{
			//ClaimTemporaryChat(selectedCharacter.gameObject, selectedCharacter.DisplayName, "*Harumph*", 3.0f);
			//CreateMainChat(new() { from = selectedCharacter.gameObject, nameTag = selectedCharacter.DisplayName, context = "*Harumph*" });
			SetMainChatSequence(new ChatData[]
			{
				new() { from = selectedCharacter.gameObject, nameTag = selectedCharacter.DisplayName, context = "*에헴*" },
				new() { from = selectedCharacter.gameObject, nameTag = selectedCharacter.DisplayName, context = "모두 내 말을 듣게" }
			});
		}
	}
}
