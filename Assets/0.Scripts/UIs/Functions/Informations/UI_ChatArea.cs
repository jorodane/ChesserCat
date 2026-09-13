using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class UI_ChatArea : UIBase
{
	GameObject mainChatClaimer;
	IEnumerator<ChatData> mainChatSequence;
	ChatData? mainChatCurrent = null;
	ITextBubble mainChatBubble = null;

	[SerializeField] Image chatScreen;
	[SerializeField] UI_NarrationBubble narrationBubble;

	readonly Dictionary<GameObject, UI_ChatBubble> bubbleDictionary = new();

	void Clear()
	{
		mainChatClaimer = null;
		mainChatSequence = null;
		mainChatCurrent = null;
		mainChatBubble = null;
	}

	public override void Registration(UIManager manager)
	{
		base.Registration(manager);
		ChatEvents.OnClaimTemporaryChat -= CreateTemporaryChat;
		ChatEvents.OnClaimTemporaryChat += CreateTemporaryChat;
		ChatEvents.OnClaimMainChatSequence -= SetMainChatSequence;
		ChatEvents.OnClaimMainChatSequence += SetMainChatSequence;
		ChatEvents.OnClaimMainChatEnd -= EndMainChatSequence;
		ChatEvents.OnClaimMainChatEnd += EndMainChatSequence;
		InputManager.OnCommandInfo -= Test;
		InputManager.OnCommandInfo += Test;
    }

    public override void Unregistration(UIManager manager)
	{
		base.Unregistration(manager);
		OnMainChatEnd();
		ChatEvents.OnClaimTemporaryChat -= CreateTemporaryChat;
		ChatEvents.OnClaimMainChatSequence -= SetMainChatSequence;
		ChatEvents.OnClaimMainChatEnd -= EndMainChatSequence;
		InputManager.OnCommandInfo -= Test;
		bubbleDictionary.Clear();
    }

    public void OnMainChatStart()
	{
		InputManager.ClaimSelectByCharacter(null);
		InputManager.OnConfirm -= OnConfirm;
		InputManager.OnConfirm += OnConfirm;
		ChatEvents.isMainChatMode = true;
        if (chatScreen) chatScreen.raycastTarget = ChatEvents.isMainChatMode;
    }

	public void OnMainChatEnd()
	{
		if (!ChatEvents.isMainChatMode) return;
		ChatEvents.isMainChatMode = false;
		InputManager.OnConfirm -= OnConfirm;
        if (chatScreen) chatScreen.raycastTarget = ChatEvents.isMainChatMode;
    }

	UI_ChatBubble GetBubble(GameObject from)
	{
		if (!from) return null;
		if (bubbleDictionary.TryGetValue(from, out UI_ChatBubble targetBubble)) return targetBubble;
		return null;
	}

	UI_ChatBubble GetOrCreateBubble(GameObject from)
	{
		if (!from) return null;
		UI_ChatBubble targetBubble = GetBubble(from);
		if (bubbleDictionary.TryGetValue(from, out targetBubble)) return targetBubble;
		UIBase instance = UIManager.ClaimCreateUI("ChatBubble", transform);
		SetChild(instance);
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
	public static void ClaimMainSequenceFromName(GameObject claimer, in string sequenceName)
	{
		ChatContainer container = DataManager.LoadDataFile<ChatContainer>(sequenceName);
		ChatEvents.OnClaimMainChatSequence(claimer, container.sequence);
	}

	public void SetMainChatSequence(GameObject claimer, IEnumerable<ChatData> sequence)
	{
		mainChatClaimer = claimer;
		mainChatSequence = sequence.GetEnumerator();
		NextMainChatSequence();
	}

	public void SetMainChatSequence(GameObject claimer, in ChatSequence sequence) => SetMainChatSequence(claimer, (IEnumerable<ChatData>)sequence);

	public void NextMainChatSequence()
	{
		if (mainChatSequence is null) return;
		if (!mainChatSequence.MoveNext())
		{
			EndMainChatSequence();
			return;
		}
		mainChatCurrent = mainChatSequence.Current;
		if(mainChatCurrent is not null)
		{
			ChatData mainChatLoaded = mainChatCurrent.Value;
			mainChatLoaded.from = mainChatLoaded.FromFinder(mainChatClaimer);
			mainChatLoaded.cameraLock.lockTarget = mainChatClaimer.transform;
			mainChatCurrent = mainChatLoaded;
			if (mainChatLoaded.isCameraLock) CameraManager.ClaimCameraLock(mainChatLoaded.cameraLock);
			else CameraManager.ClaimCameraUnlock();
			CreateMainChat(mainChatLoaded);
        }
        else
        {
            EndMainChatSequence();
            return;
        }
    }

	public void EndMainChatSequence()
	{
		if (mainChatCurrent is null) return;
		EndChat(mainChatCurrent);
		Clear();
		OnMainChatEnd();
		CameraManager.ClaimCameraUnlock();
	}


	void CreateMainChat(in ChatData data)
	{
		ITextBubble newMainChat = null;
		switch(data.style)
		{
			case ChatStyle.ChatBubble: newMainChat = GetOrCreateBubble(data.from); break;
			case ChatStyle.Narration: newMainChat = narrationBubble; break;
		}
		newMainChat ??= narrationBubble;
		if(mainChatBubble is not null && newMainChat != mainChatBubble) EndMainChatSequence();
		mainChatBubble = newMainChat;
		if (mainChatBubble is null) return;
		mainChatBubble.SetText(data);
		mainChatBubble.SetNextGuide();
		OnMainChatStart();
		ChatEvents.ClaimMainChatData(mainChatClaimer, data);
    }

	void EndChat(in ChatData? data)
	{
		if(data is null) return;
		if (mainChatBubble is null) return;
		mainChatBubble.Close();
	}


	void Test(bool value)
	{
		var chars = BattleManager.GetCharacters();
		GameObject selectedObject;
		if (chars is not null && chars.Length > 0) selectedObject = chars[Random.Range(0, chars.Length)].gameObject;
		else selectedObject = null;
		if (selectedObject)
		{
			ClaimMainSequenceFromName(selectedObject.gameObject, "Intro_00");
		}
	}
}
