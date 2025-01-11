using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Lotten.Scripts;
namespace Lotten.Scripts.Products;

public partial class Plant : Product
{
	public List<Texture2D> Sprites = [];
	public override void _Ready()
	{
		
			PlantSprite = GetNode<Sprite2D>("Sprite2D");
			if(Sprites.Count > 0) PlantSprite.Texture = Sprites.First();
			
	}

	public override void CopyValuesFrom(Product product)
	{
		ProductName = product.ProductName;
		Description = product.Description;
		Cost = product.Cost;
		GridPosition = product.GridPosition;
		Sprites = (product as Plant)!.Sprites;
	}
}
