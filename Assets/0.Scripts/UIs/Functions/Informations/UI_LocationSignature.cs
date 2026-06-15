using TMPro;
using UnityEngine;

public class UI_LocationSignature : UIBase
{
    [SerializeField] TextMeshProUGUI lineText;
    [SerializeField] Color defaultColor;
    [SerializeField] Color highlightedColor;

    public int  index;
    public bool isHorizontal;

    Vector3 calculatedPosition;

    public override void Registration(UIManager manager)
    {
        base.Registration(manager);
        index = transform.GetSiblingIndex();
        GameManager.OnUpdateUI -= LocationUpdate;
        GameManager.OnUpdateUI += LocationUpdate;
    }

    public override void Unregistration(UIManager manager)
    {
        base.Unregistration(manager);
        GameManager.OnUpdateUI -= LocationUpdate;
    }

    private void OnEnable()
    {
        SetIndex(transform.GetSiblingIndex());
        GameManager.OnUpdateUI -= LocationUpdate;
        GameManager.OnUpdateUI += LocationUpdate;
        TileManager.TileHoverEvent -= CheckTile;
        TileManager.TileHoverEvent += CheckTile;
    }

    private void OnDisable()
    {
        GameManager.OnUpdateUI -= LocationUpdate;
        TileManager.TileHoverEvent -= CheckTile;
    }

    void SetIndex(int newIndex)
    {
        index = newIndex;
        lineText?.SetText(isHorizontal ? TileManager.GetTileHorizonText(index) : TileManager.GetTileVerticalText(index));
    }

    void LocationUpdate(float deltaTime)
    {
        if (isHorizontal)
        {
            calculatedPosition.x = TileManager.GetTileScreenPositionHorizontal(index).x;
            calculatedPosition.y = 0;
            transform.position = calculatedPosition;
        }
        else
        {
            calculatedPosition.x = 0;
            calculatedPosition.y = TileManager.GetTileScreenPositionVertical(index).y;
            transform.position = calculatedPosition;
        }
    }

    void CheckTile(Vector3Int hoverPosition, TileBase tile)
    {
        if (!lineText) return;
        bool isSame = false;
        if (isHorizontal)   isSame = hoverPosition.x == index;
        else                isSame = hoverPosition.y == index;

        lineText.color = isSame ? highlightedColor : defaultColor;
    }
}
