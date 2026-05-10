using System;
using UnityEngine;

public class CharacterShaderModule : CharacterModule
{
	Renderer render;
	Material material;
	public override Type RegistrationType => typeof(CharacterShaderModule);
	public override void OnRegistration(CharacterBase newOwner)
	{
		base.OnRegistration(newOwner);
		if (!newOwner) return;
		render = GetComponent<Renderer>();
		if (render) material = render.material;
		newOwner.OnHovered -= HoverEffect;
		newOwner.OnHovered += HoverEffect;
	}

	public override void OnUnregistration(CharacterBase oldOwner)
	{
		base.OnUnregistration(oldOwner);
		if (!oldOwner) return;
		oldOwner.OnHovered -= HoverEffect;

	}

	private void HoverEffect(bool isHovered)
	{
		if (!material) return;
		if(isHovered)
		{
			material.EnableKeyword("OUTBASE_ON");
		}
		else
		{
			material.DisableKeyword("OUTBASE_ON");
		}
	}

}
