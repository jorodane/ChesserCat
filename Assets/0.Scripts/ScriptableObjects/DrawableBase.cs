using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public struct DrawVariation
{
	public string variationName;
	public Sprite variationVisual;

	public static implicit operator KeyValuePair<string, Sprite>(in DrawVariation target) => new(target.variationName, target.variationVisual);
}

public abstract class DrawableBase : ScriptableObject
{
	public TileEnterException enterCheck;
	public TileEnterException EnterCheck() => enterCheck;

	public Sprite mainVisual;

	public DrawVariation[] variations;

	readonly Dictionary<string, Sprite> variationDictionary = new();

	public void Initialize()
	{
		if (variations is null) return;
		variationDictionary.Clear();
		foreach (DrawVariation currentVariation in variations)
		{
			variationDictionary.Add(currentVariation.variationName, currentVariation.variationVisual);
		}
	}

	public IEnumerable<string> GetVariationNames(bool needEmpty)
	{
		if (variationDictionary is null) yield break;
		if (needEmpty) yield return null;
		foreach (string currentValue in variationDictionary.Keys.ToArray()) yield return currentValue;
	}

	public Sprite GetVisual(string variationName = null)
	{
		if (string.IsNullOrEmpty(variationName)) return mainVisual;
		else if (variationDictionary.TryGetValue(variationName, out Sprite result)) return result;
		else return mainVisual;
	}
}
