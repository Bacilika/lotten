using System;
using Godot;

namespace Lotten.Scripts.Products;

public partial class Building: Product
{
	public Building BuildingInstance;

	public Func<Building> SubtypeGenerator;
	public override void _Ready()
	{
		PlantSprite = GetNode<Sprite2D>("Sprite2D");
	}
	
	
	public override void CopyValuesFrom(Product product)
	{
		ProductName = product.ProductName;
		Description = product.Description;
		GridPosition = product.GridPosition;
		Cost = product.Cost;
		if (product is Building building)
		{ 
			BuildingInstance = building.SubtypeGenerator();
		}
		
	}
}
