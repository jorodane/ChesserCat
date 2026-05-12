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

	public void Connect(ControllerBase target) => this.GeneralConnect(ref _connectedController, target, OnConnected);

	protected virtual void OnConnected(ControllerBase target)
	{
		Refresh();
	}

	public void Disconnect(ControllerBase target) => this.GeneralDisconnect(ref target, OnDisconnected);
	protected virtual void OnDisconnected(ControllerBase target)
	{
		int characterMaxCount = target.Characters.Count;
		int slotMaxCount = currentSlots.Count;
		for (int i = 0; i < slotMaxCount; i++)
		{
			UIBase currentUI = currentSlots[i];
			CharacterBase currentCharacter = i < characterMaxCount ? target.Characters[i] : null;

			if (currentUI is ICharacterConnectable asCharacterUI) asCharacterUI.Disconnect(currentCharacter);
		}
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
}
