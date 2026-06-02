using UnityEngine;

public class UI_IngameAreaVisalizer : MonoBehaviour
{
    void OnEnable()
    {
        TileManager.SetTileOffsetVisual(Camera.main.ScreenToWorldPoint(transform.position));
    }
}
