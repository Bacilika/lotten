using System;
using Godot;

namespace Lotten.Scripts.Products;

public partial class Building: Product
{
	public Building BuildingInstance;
	public Building BuildingScene;
	public GameScene _gameScene;
	public Func<Building> SubtypeGenerator;
	public override void _Ready()
	{
		PlantSprite = GetNode<Sprite2D>("Sprite2D");
		BodyEntered += OnBodyEntered;
		BodyExited += OnBodyExited;
	}

	public virtual void OnBodyExited(Node2D body)
	{
		BuildingInstance?.OnBodyExited(body);
	}

	public virtual void OnBodyEntered(Node2D body)
	{
		BuildingInstance?.OnBodyEntered(body);
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
			BuildingInstance.BuildingScene = this;
			BuildingInstance.ReadyInstance();
		}
		
	}
	public virtual void ReadyInstance()
	{
		
	}


}
