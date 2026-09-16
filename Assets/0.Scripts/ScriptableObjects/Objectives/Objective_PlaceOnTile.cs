using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Objective_PlaceOnTile", menuName = "Objective/PlaceOnTile/Object")]
public class Objective_PlaceOnTile : ObjectiveBase
{
    public Vector3Int[] targetTiles;

    public string highlightPrefab = "TileHighlight";
    public string completePrefab = "TileHighlightComplete";

    GameObject[] effectInstances;

    protected override void Initialize()
    {
        base.Initialize();
        if(targetTiles is not null && targetTiles.Length > 0)
        {
            effectInstances = new GameObject[targetTiles.Length];
            for(int i = 0; i < targetTiles.Length; i++)
            {
                TileBase targetTile = TileManager.GetTile(targetTiles[i]);
                if(targetTile)
                {
                    GameObject instance = ObjectManager.CreateObject(highlightPrefab, targetTile.transform);
                    effectInstances[i] = instance;
                    instance.SetActive(false);
                }
            }
        }
    }

    protected override void Dispose()
    {
        base.Dispose();
        if(effectInstances is not null)
        {
            foreach(GameObject currentInstance in effectInstances) ObjectManager.DestroyObject(currentInstance);
            effectInstances = null;
        }
    }

    public override bool CheckClearCondition(TurnBaseInfo lastTurn, out GameObject clearClaimer)
    {
		GameObject resultClaimer = null;
        bool result = true;
		if (targetTiles is not null)
		{
            for(int i = 0; i < targetTiles.Length; i++)
            {
                Vector3Int currentTile = targetTiles[i];

				bool currentResult = CheckTile(currentTile, ref resultClaimer);
                if (effectInstances is not null && effectInstances.TryGetValue(i, out GameObject currentEffect) && currentEffect)
                {
                    bool originActive = currentEffect.activeSelf;
                    bool currentActive = !currentResult;
                    if(originActive && !currentActive)
                    {
                        GameObject instance = ObjectManager.CreateObject(completePrefab);
                        instance.transform.position = currentEffect.transform.position;
                    }
                    currentEffect.SetActive(currentActive);
                }
                result &= currentResult;
            }
		}
		clearClaimer = result ? resultClaimer : null;
        return result;
	}

    public virtual bool CheckTile(in Vector3Int targetTile, ref GameObject clearClaimer)
    {
		GameObject currentObject = TileManager.GetObjectOnTile(targetTile);
        bool result = currentObject;
		if (result) clearClaimer ??= currentObject;
		return result;
    }

    protected override IEnumerator OnObjectiveStart()
    {
        UI_IngameAreaVisalizer.ClaimIngameAreaBlock("ObjectiveShower");
        foreach (GameObject currentInstance in effectInstances)
        {
            if (!currentInstance) continue;
            currentInstance.SetActive(true);
            yield return CameraManager.ClaimLockSmooth(currentInstance.transform.position, 3.0f, 0.5f, 0.5f);
        }
        yield return CameraManager.ClaimUnlockSmooth(0.2f);
        UI_IngameAreaVisalizer.ClaimIngameAreaUnblock("ObjectiveShower");
    }
}
