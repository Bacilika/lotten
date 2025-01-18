using Godot;
using System;
using Lotten.Scripts.Products;
using Lotten.Scripts.Products.Buildings;

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
	private RichTextLabel _moneyLabel;
	private int _money;
	public int Money
	{
		get=>_money;
		set
		{
			_money = value;
			_moneyLabel.Text = $" [img]res://Sprites/coin.png[/img]x{_money}";
		}
	}

	public Product ActiveProduct
	{
		get => _activeProduct;
		set
		{
			if(value == _activeProduct) return;
			if (value is null || _activeProduct is not null)
			{
				_gameScene.RemoveChild(_activeProduct);
			}
			if(value is not null)
			{
				value.GlobalPosition = _gameScene.GetGlobalMousePosition();
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
		_moneyLabel = GetNode<RichTextLabel>("MoneyLabel");
		Money = 1000;
		GetNode<Button>("Button").Pressed += () => { _gameScene.ToggleMode();};
	}

	public override void _Process(double delta)
	{
		
		if(ActiveProduct != null)
		{
			var deltaFloat = (float)delta;
			ActiveProduct.GridPosition = _gameScene.GrassLayer.LocalToMap(_gameScene.GetGlobalMousePosition());
			var lerpPosition = _gameScene.GrassLayer.MapToLocal(ActiveProduct.GridPosition);
			ActiveProduct.LerpedPosition = new Vector2((lerpPosition.X+_gameScene.GetGlobalMousePosition().X)/2,(lerpPosition.Y+_gameScene.GetGlobalMousePosition().Y)/2);
			ActiveProduct.GlobalPosition = ActiveProduct.GlobalPosition.Lerp(ActiveProduct.LerpedPosition, 15 * deltaFloat);
		}
	}

	public void OnPlaceProduct(Product product)
	{
		if(_gameScene.GetPlotAreaAt(_gameScene.GetGlobalMousePosition()) == null) return;
		switch (product)
		{
			case Building building:
				if (!CanPlaceBuilding(building))
				{
					return;
				}
				if (building.Duplicate() is Building newBuilding)
				{
					newBuilding.CopyValuesFrom(building);
					EmitSignal(newBuilding.BuildingInstance is Plot ? SignalName.OnPlacePlot : SignalName.OnPlaceBuilding,
						newBuilding);
					Money-= newBuilding.Cost;
				}
				break;
			case Plant plant:
				if(!CanPlacePlant(plant)) return;
				if (plant.Duplicate() is Plant newPlant)
				{
					newPlant.CopyValuesFrom(plant);
					EmitSignal(SignalName.OnPlacePlant, newPlant);
					Money-= plant.Cost;
				}
				break; 
		}
	}
	public bool CanAfford(Product product)
	{
		return Money >= product.Cost;
	}

	public bool CanPlaceBuilding(Building building)
	{
		return CanAfford(building) && !_gameScene.BuildingsOnGrid.ContainsKey(building.GridPosition);
	}
	public bool CanPlacePlant(Plant plant)
	{
		if (!CanAfford(plant)) return false;
		if(_gameScene.BuildingsOnGrid.TryGetValue(plant.GridPosition, out var building))
		{
			return building is Plot { Plant: null };
		}
		return false;
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (ActiveProduct == null) return;
		
		if(@event.IsActionPressed(Inputs.LeftClick))
		{
			OnPlaceProduct(ActiveProduct);
			_gameScene.Dragging = true;

		}
		else if(@event.IsActionPressed(Inputs.RightClick))
		{
			ActiveProduct = null;
		}
		else if(@event.IsActionReleased(Inputs.LeftClick))
		{
			OnPlaceProduct(ActiveProduct);
			_gameScene.Dragging = false;
		}
	}


	public void OnShopButtonPressed(Product product)
	{
		ActiveProduct = product;
	}
}
