using Godot;
using System;
using Lotten.Scripts.Products;
namespace Lotten.Scripts;
public partial class UI : CanvasLayer
{
	private Shop shop;
	[Signal]
	public delegate void OnPlaceBuildingEventHandler(Building building);
	[Signal]
	public delegate void OnPlacePlantEventHandler(Plant plant);	
	[Signal]
	public delegate void OnPlacePlotEventHandler(Building plot);
	
	private GameScene _gameScene;
	private Product _activeProduct;
	public Product ActiveProduct
	{
		get => _activeProduct;
		set
		{
			if (value is null)
			{
				_gameScene.RemoveChild(_activeProduct);
				
			}
			else
			{
				_gameScene.AddChild(value);
			}
			_activeProduct = value;
		} 
	}
	
	public override void _Ready()
	{
		shop = GetNode<Shop>("Shop");
		shop.OnShopButtonPressed += OnShopButtonPressed;
		_gameScene = GetParent<GameScene>();
	}

	public override void _Process(double delta)
	{
		if(ActiveProduct != null)
		{
			ActiveProduct.GridPosition = _gameScene.GrassLayer.LocalToMap(_gameScene.GetGlobalMousePosition());
			ActiveProduct.GlobalPosition = ActiveProduct.GridPosition * 16;
			
		}
	}

	public void OnPlaceProduct(Product product)
	{
		switch (product)
		{
			case Building building:
				if(!CanPlaceBuilding(building)) return;
				if (building.Duplicate() is Building newBuilding)
				{
					newBuilding.CopyValuesFrom(building);
					EmitSignal(newBuilding.BuildingInstance is Plot ? SignalName.OnPlacePlot : SignalName.OnPlaceBuilding,
						newBuilding);
				}
				break;
			case Plant plant:
				if(!CanPlacePlant(plant)) return;
				if (plant.Duplicate() is Plant newPlant)
				{
					newPlant.CopyValuesFrom(plant);
					EmitSignal(SignalName.OnPlacePlant, newPlant);
					
				}
				break; 
		}
	}

	public bool CanPlaceBuilding(Building building)
	{
		return !_gameScene.BuildingsOnGrid.ContainsKey(building.GridPosition);
	}
	public bool CanPlacePlant(Plant plant)
	{
		if(_gameScene.BuildingsOnGrid.TryGetValue(plant.GridPosition, out var building))
		{
			return building is Plot { Plant: null };
		}
		return false;
	}

	public override void _Input(InputEvent @event)
	{
		if (ActiveProduct == null) return;
		
		if(@event.IsActionPressed(Inputs.LeftClick))
		{
			OnPlaceProduct(ActiveProduct);

		}
		else if(@event.IsActionPressed(Inputs.RightClick))
		{
			ActiveProduct = null;
		}
	}


	public void OnShopButtonPressed(Product product)
	{
		Console.WriteLine("UI received signal");
		
		ActiveProduct = product;
		
	}
}
