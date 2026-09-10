using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class UI_ChatArea : UIBase
{
	IEnumerator<ChatData> mainChatSequence;
	ChatData? mainChatCurrent = null;

	[SerializeField] Image chatScreen;

	readonly Dictionary<GameObject, UI_ChatBubble> bubbleDictionary = new();

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

    UI_ChatBubble GetOrCreateBubble(GameObject from)
	{
		if (bubbleDictionary.TryGetValue(from, out UI_ChatBubble targetBubble)) return targetBubble;
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

	public void SetMainChatSequence(IEnumerable<ChatData> sequence)
	{
		EndMainChatSequence();
		mainChatSequence = sequence.GetEnumerator();
		NextMainChatSequence();
	}

	public void SetMainChatSequence(in ChatSequence sequence) => SetMainChatSequence((IEnumerable<ChatData>)sequence);

	public void NextMainChatSequence()
	{
		if (mainChatSequence is null) return;
		if (!mainChatSequence.MoveNext())
		{
			EndMainChatSequence();
			return;
		}
		mainChatCurrent = mainChatSequence.Current;
		CameraManager.ClaimCameraLock(mainChatCurrent?.cameraLock);
		CreateMainChat(mainChatCurrent.Value);
	}

	public void EndMainChatSequence()
	{
		EndChat(mainChatCurrent);
		mainChatCurrent = null;
		mainChatSequence = null;
		OnMainChatEnd();
		CameraManager.ClaimCameraLock(null);
	}


	void CreateMainChat(in ChatData data)
	{
		UI_ChatBubble targetBubble = GetOrCreateBubble(data.from);
		if (!targetBubble) return;
		targetBubble.SetText(data);
		targetBubble.SetNextGuide();
		OnMainChatStart();
		ChatEvents.ClaimMainChatData(data);
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
		if (selectedCharacter)
		{
			SetMainChatSequence(new ChatSequence
			(
				new ChatData() 
				{ 
					from = selectedCharacter.gameObject, nameTag = selectedCharacter.DisplayName, context = "*에헴*",
					cameraLock = new() { lockZoom = true, zoomScale = 2.0f, lockTarget = selectedCharacter.transform, lockDelay = 0.2f},
				},
				new ChatData() 
				{ 
					from = selectedCharacter.gameObject, nameTag = selectedCharacter.DisplayName, context = "모두 내 말을 듣게" ,
					cameraLock = new() { lockZoom = true, zoomScale = 2.0f, lockTarget = selectedCharacter.transform, lockDelay = 0.2f},
				}
			));
		}
	}
}
