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
	private BuildingInfo _buildingInfo;
	private TargetSelectionView _targetSelectionView;
	
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
		_gameScene = GetParent<GameScene>();
		_moneyLabel = GetNode<RichTextLabel>("MoneyLabel");
		_buildingInfo = GetNode<BuildingInfo>("BuildingInfo");
		_targetSelectionView = GetNode<TargetSelectionView>("TargetSelectionView");
		
		_buildingInfo.GetNode<Button>("Close").Pressed += () => ShowView("Shop");
		_targetSelectionView.GetNode<Button>("VBoxContainer/ButtonContainer/CancelButton").Pressed += () => ShowView("BuildingInfo");
		_targetSelectionView.GetNode<Button>("VBoxContainer/ButtonContainer/ConfirmButton").Pressed += () => ShowView("BuildingInfo");
		
		shop.OnShopButtonPressed += OnShopButtonPressed;
		ShowView("Shop");
		Money = 1000;
	}

	public override void _Process(double delta)
	{
		if (ActiveProduct == null) return;
		
		var deltaFloat = (float)delta;
		ActiveProduct.GridPosition = _gameScene.GrassLayer.LocalToMap(_gameScene.GetGlobalMousePosition());
		var targetGridPosition = _gameScene.GrassLayer.MapToLocal(ActiveProduct.GridPosition);
		ActiveProduct.LerpedPosition = targetGridPosition.Lerp(_gameScene.GetGlobalMousePosition(), 0.25f);;
		ActiveProduct.GlobalPosition = ActiveProduct.GlobalPosition.Lerp(ActiveProduct.LerpedPosition, 15 * deltaFloat);
	}

	private void OnPlaceProduct(Product product)
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
	private bool CanAfford(Product product)
	{
		return Money >= product.Cost;
	}

	private bool CanPlaceBuilding(Building building)
	{
		return CanAfford(building) && !_gameScene.BuildingsOnGrid.ContainsKey(building.GridPosition);
	}
	private bool CanPlacePlant(Plant plant)
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

	private void ShowView(string view)
	{
		switch (view)
		{
			case "BuildingInfo":
				_buildingInfo.Visible = true;
				shop.Visible = false;
				_targetSelectionView.Visible = false;
				break;
			case "Shop":
				_buildingInfo.Visible = false;
				shop.Visible = true;
				_targetSelectionView.Visible = false;
				break;
			case "TargetSelectionView":
				_buildingInfo.Visible = false;
				shop.Visible = false;
				_targetSelectionView.Visible = true;
				break;
		}
	}



	private void OnShopButtonPressed(Product product)
	{
		ActiveProduct = product;
	}
	
	public void OnTargetSelectionView(Building building)
	{
		_targetSelectionView.Building = building;
		ShowView("TargetSelectionView");
	}

	public void OnShowBuildingInfo(Building building, bool visible)
	{
		if(_targetSelectionView.Visible) return;
		Console.WriteLine("OnShowBuildingInfo");
		if(visible)
		{
			ShowView("BuildingInfo");
			_buildingInfo.ShowBuildingInfo(building);
		}
		else
		{
			if(building == _buildingInfo.Building) //close requested by open building
			{
				ShowView("Shop");
			}
		}
	}

	public void OnTargetUpdate(string targets)
	{
		_targetSelectionView.TargetLabel.Text = targets;
	}
}
