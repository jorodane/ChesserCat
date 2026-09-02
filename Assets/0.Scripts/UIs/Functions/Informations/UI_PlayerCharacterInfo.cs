using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class UI_PlayerCharacterInfo : UIBase, IControllerConnectable
{
	ControllerBase _connectedController;
	public ControllerBase ConnectedController => _connectedController;

	[SerializeField] string slotPrefab;
	protected List<UIBase> currentSlots = new();

	public override void Registration(UIManager manager)
	{
		base.Registration(manager);
		OnLocalPlayerControllerChanged(BattleManager.GetLocalPlayerOnBattle());
		BattleManager.OnLocalPlayerControllerChanged -= OnLocalPlayerControllerChanged;
		BattleManager.OnLocalPlayerControllerChanged += OnLocalPlayerControllerChanged;
	}


	public override void Unregistration(UIManager manager)
	{
		base.Unregistration(manager);
		BattleManager.OnLocalPlayerControllerChanged -= OnLocalPlayerControllerChanged;
	}

	public void Connect(ControllerBase target) => this.GeneralConnect(ref _connectedController, target, OnConnected);

	protected virtual void OnConnected(ControllerBase target)
	{
		if (target)
		{
			target.OnControllerPossess   -= OnPossess;
			target.OnControllerPossess   += OnPossess;
			target.OnControllerUnPossess -= OnUnPossess;
			target.OnControllerUnPossess += OnUnPossess;
		}
		Refresh();
	}

	public void Disconnect(ControllerBase target) => this.GeneralDisconnect(ref target, OnDisconnected);
	public void Disconnect() => this.GeneralDisconnect(ref _connectedController, OnDisconnected);
	protected virtual void OnDisconnected(ControllerBase target)
	{
		foreach (UIBase currentSlot in currentSlots)
		{
			if (currentSlot is ICharacterConnectable asCharacterUI) asCharacterUI.Disconnect();
			UIManager.ClaimUnsetUI(currentSlot);
			ObjectManager.DestroyObject(currentSlot.gameObject);
		}
		currentSlots.Clear();

		if (target)
		{
			target.OnControllerPossess   -= OnPossess;
			target.OnControllerUnPossess -= OnUnPossess;
		}
	}

	void OnLocalPlayerControllerChanged(PlayerController newController)
	{
		if (newController) Connect(newController);
		else Disconnect(ConnectedController);
	}

	UIBase CreateSlot()
	{
		GameObject instance = ObjectManager.CreateObject(slotPrefab, transform);
		UIBase createdUI = instance?.GetComponent<UIBase>();
		if(createdUI) currentSlots.Add(createdUI);
		return createdUI;
	}

	public void Refresh()
	{
		if (!ConnectedController) return;

		int characterMaxCount = ConnectedController.Characters.Count;
		int slotMaxCount = currentSlots.Count;
		int maxIndex = Mathf.Max(characterMaxCount, slotMaxCount);
		for (int i = 0; i < maxIndex; i++)
		{
			UIBase currentUI = i < slotMaxCount ? currentSlots[i] : CreateSlot();
			CharacterBase currentCharacter = i < characterMaxCount ? ConnectedController.Characters[i] : null;
			if (currentUI is ICharacterConnectable asCharacterUI) asCharacterUI.Connect(currentCharacter);
		}
	}

	void OnPossess(CharacterBase target)
	{
		if (target.IsPawn) return;
		UIBase currentUI = CreateSlot();
		if (currentUI is ICharacterConnectable asCharacterUI) asCharacterUI.Connect(target);
	}

	void OnUnPossess(CharacterBase target)
	{

	}
}
