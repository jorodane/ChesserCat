using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "TileBasement", menuName = "Tiles/TileBasement")]
public class TileBasement : ScriptableObject
{
	public TileEnterException enterCheck;
	public TileEnterException EnterCheck() => enterCheck;

	public Sprite mainVisual;

	public DecorationVariation[] variations;

	readonly Dictionary<string, Sprite> variationDictionary = new();

	public void Initialize()
	{
		if (variations is null) return;
		variationDictionary.Clear();
		foreach (DecorationVariation currentVariation in variations)
		{
			variationDictionary.Add(currentVariation.variationName, currentVariation.variationVisual);
		}
	}

	public string[] GetVariationNames() => variationDictionary?.Keys.ToArray();

	public Sprite GetVisual(string variationName = null)
	{
		if (string.IsNullOrEmpty(variationName)) return mainVisual;
		else if (variationDictionary.TryGetValue(variationName, out Sprite result)) return result;
		else return mainVisual;
	}
}
