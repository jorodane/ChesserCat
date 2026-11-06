using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UI_MapWindow : OpenableUIBase, IScrollHandler, IPointerMoveHandler
{
    [SerializeField] Image worldImage;
    [SerializeField] TextMeshProUGUI worldNameText;
    [SerializeField] RectTransform worldBackground;
    [SerializeField] Transform stageButtonContainer;
    UI_Button_StageSelect[] loadedStageSelectors;
    WorldBase _currentWorld;
    public WorldBase CurrentWorld => _currentWorld;

    [SerializeField] WorldBase testWorld;

	float scaleCurrent = 1.0f;
	float scaleMin = 0.5f;
	float scaleMax = 2.0f;

    private void OnEnable()
    {
        SetWorld(testWorld);
    }

    public void SetWorld(WorldBase newWorld)
    {
        if (CurrentWorld == newWorld) return;
        _currentWorld = newWorld;
        if(newWorld)
        {
            worldNameText.text = CurrentWorld.name;
            worldImage.enabled = worldImage.sprite = newWorld.worldImage;
			scaleCurrent = newWorld.scaleBase;
			scaleMin = Mathf.Max(newWorld.scaleMin, 0.05f);
			scaleMax = newWorld.scaleMax;
			worldImage.transform.localScale = scaleCurrent * Vector3.one;
			SetStageLocators(newWorld.stages);
		}
        else
        {
            worldNameText.text = string.Empty;
            worldImage.enabled = false;
			ClearStageLocators();
		}
	}

	public void ClearStageLocators()
	{
		if (loadedStageSelectors is not null)
		{
			foreach (UI_Button_StageSelect currentStageSelector in loadedStageSelectors)
			{
				if (!currentStageSelector) continue;
				ObjectManager.DestroyObject(currentStageSelector.gameObject);
			}
		}
		loadedStageSelectors = null;
	}

	public void SetStageLocators(in StageLocator[] locators)
	{
		ClearStageLocators();
		if (locators is null || locators.Length <= 0 || !stageButtonContainer) return;
		loadedStageSelectors = new UI_Button_StageSelect[locators.Length];
		int index = 0;
		foreach (StageLocator currentLocator in locators)
		{
			UI_Button_StageSelect currentSelector = CreateStageSelector(currentLocator);
			if (!currentSelector) break;
			loadedStageSelectors[index++] = currentSelector;
		}
	}

	public UI_Button_StageSelect CreateStageSelector(in StageLocator locator)
	{
		GameObject newButton = ObjectManager.CreateObject("StageButton", stageButtonContainer, locator.location);
		if (!newButton) return null;
		newButton.transform.localPosition = locator.location;
		UI_Button_StageSelect newSelector = newButton.GetComponent<UI_Button_StageSelect>();
		if (newSelector)
		{
			newSelector.SetStage(locator.stage);
			newSelector.SetCondition(locator.unlockCondition);
			newSelector.SetLabel(locator.label);
		}
		return newSelector;
	}

	public void AddWorldScale(in Vector3 offset, float delta)
	{
		float result = Mathf.Clamp(scaleCurrent + delta, scaleMin, scaleMax);
		float diff = result - scaleCurrent;
		if (Mathf.Abs(diff) < float.Epsilon) return;
		if(worldImage && worldImage.transform is RectTransform worldTransform)
		{
			worldTransform.localScale = Vector3.one * result;
			Vector3 positionDiff = (worldTransform.position - offset);
			Vector3 positionShift = positionDiff * ((result / scaleCurrent) - 1.0f);
            MoveWorldPosition(positionShift);
		}
		scaleCurrent = result;
	}

    public void MoveWorldPosition(Vector2 screenShift)
    {
        if (worldImage && worldImage.transform is RectTransform worldTransform)
        {
            if(worldBackground)
			{
                Rect backgroundRect = worldBackground.rect;
				Vector2 worldPosition = worldTransform.position;
                Rect worldRect = worldTransform.rect;
				Vector2 worldSize = worldTransform.localScale;

				backgroundRect.center = worldBackground.position;
                worldRect.size *= worldSize;
				worldRect.center = worldPosition + screenShift;
                screenShift -= backgroundRect.InversedAABB(worldRect);
            }

            worldTransform.position += (Vector3)screenShift;
        }
    }

	public void OnScroll(PointerEventData eventData)
	{
		float delta = eventData.scrollDelta.y.normalized();
		AddWorldScale(InputManager.CursorScreenPosition, delta * 0.05f);
	}

    public void OnPointerMove(PointerEventData eventData)
    {
        if (InputManager.IsCursorPress_Left)
        {
            MoveWorldPosition(eventData.delta);
        }
    }
}
