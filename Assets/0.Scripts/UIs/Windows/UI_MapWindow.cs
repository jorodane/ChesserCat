using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_MapWindow : OpenableUIBase
{
    [SerializeField] Image worldImage;
    [SerializeField] TextMeshProUGUI worldNameText;
    [SerializeField] Transform stageButtonContainer;
    UI_Button_StageSelect[] loadedStageSelectors;
    WorldBase _currentWorld;
    public WorldBase CurrentWorld => _currentWorld;

    [SerializeField] WorldBase testWorld;

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

            if(loadedStageSelectors is not null)
            {
                foreach (UI_Button_StageSelect currentStageSelector in loadedStageSelectors)
                {
                    if (!currentStageSelector) continue;
                    ObjectManager.DestroyObject(currentStageSelector.gameObject);
                }
            }


            if (newWorld.stages is not null && newWorld.stages.Length > 0 && stageButtonContainer)
            {
                loadedStageSelectors = new UI_Button_StageSelect[newWorld.stages.Length];
                foreach (StageLocator currentLocator in newWorld.stages)
                {
                    GameObject newButton = ObjectManager.CreateObject("StageButton", stageButtonContainer, currentLocator.location);
                    if (!newButton) break;
                    newButton.transform.localPosition = currentLocator.location;
                    UI_Button_StageSelect newSelector = newButton.GetComponent<UI_Button_StageSelect>();
                    if (newSelector)
                    {
                        newSelector.SetStage(currentLocator.stage);
                        newSelector.SetCondition(currentLocator.unlockCondition);
                        newSelector.SetLabel(currentLocator.label);
                    }
                }
            }
            else
            {
                loadedStageSelectors = null;
            }
        }
        else
        {
            worldNameText.text = string.Empty;
            worldImage.enabled = false;
        }
    }
}
