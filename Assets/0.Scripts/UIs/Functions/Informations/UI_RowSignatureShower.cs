using System;
using UnityEngine;

public class UI_RowSignatureShower : UIBase
{
	[SerializeField] string horizontalPrefab = "LocationSignature";
	[SerializeField] string verticalPrefab = "LocationSignature_Vertical";

	[SerializeField] Transform horizontalLine;
	[SerializeField] Transform verticalLine;

	public override void Registration(UIManager manager)
	{
		base.Registration(manager);
		TileManager.OnFieldSizeChanged -= OnFieldSizeChanged;
		TileManager.OnFieldSizeChanged += OnFieldSizeChanged;
	}

	public override void Unregistration(UIManager manager)
	{
		base.Unregistration(manager);
		TileManager.OnFieldSizeChanged -= OnFieldSizeChanged;
	}

	void OnFieldSizeChanged(int width, int height)
	{
		SetHorizontalSize(width);
		SetVerticalSize(height);
	}

	public void SetChildCount(Transform targetTransform, int count, string prefabName)
	{
		int delta = count - targetTransform.childCount;

		while (delta < 0)
		{
			Transform currentChild = targetTransform.GetChild(targetTransform.childCount - 1);
			ObjectManager.DestroyObject(currentChild.gameObject);
			++delta;
		}

		while (delta > 0)
		{
			GameObject instance = ObjectManager.CreateObject(prefabName);
			if (!instance) return;
			instance.transform.SetParent(targetTransform, false);
			if(instance.TryGetComponent(out UI_LocationSignature newSignature))
			{
				newSignature.SetIndex(instance.transform.GetSiblingIndex());
			}
			--delta;
		}
	}

	public void SetHorizontalSize(int width) => SetChildCount(horizontalLine, width, horizontalPrefab);

	public void SetVerticalSize(int height) => SetChildCount(verticalLine, height, verticalPrefab);
}
