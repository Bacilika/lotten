using Godot;
using System;
using System.Collections.Generic;
using Godot.Collections;
using Lotten.Scripts.Products;
using Lotten.Scripts.Products.Buildings;

namespace Lotten.Scripts;
public partial class GameScene : Node2D
{
	private UI _userInterface;
	public TileMapLayer GrassLayer;
	public TileMapLayer PlotLayer;
	public TileMapLayer WaterLayer;
	public Array<Vector2I> PlotTiles = [];
	public Array<Vector2I> GrassTiles = [];
	public Array<Vector2I> WaterTiles = [];
	public System.Collections.Generic.Dictionary<Vector2I,Building> BuildingsOnGrid  = new();
	public IDraggable FocusedDraggable;
	public bool Dragging;
	public Timer WaterTimer;
	public RandomNumberGenerator RandomNumberGenerator = new ();
	private PackedScene _waterScene = ResourceLoader.Load<PackedScene>("res://Scenes/Water.tscn");
	private PackedScene _plotAreaScene = ResourceLoader.Load<PackedScene>("res://Scenes/plot_area.tscn");
	private Control _wireView;
	public List<PlotArea> ExpandedAreas = [];
	public TileMapValues TileMapValues = new ();
	
	public bool TargetSetMode;

	[Signal]
	public delegate void OnBuildingPlacedEventHandler();
	public override void _Ready()
	{
		_userInterface = GetNode<UI>("UI");
		_userInterface.OnPlaceBuilding += OnPlaceBuilding;
		_userInterface.OnPlacePlot += OnPlacePlot;
		_userInterface.OnPlacePlant += OnPlacePlant;
		GrassLayer = GetNode<TileMapLayer>("GrassLayer");
		PlotLayer = GetNode<TileMapLayer>("PlotLayer");
		WaterLayer = GetNode<TileMapLayer>("WaterLayer");
		SetUpExpansionZone(Vector2.Zero);
		SetUpTimers();
	}

	private void SetUpTimers()
	{
		WaterTimer = new Timer
		{
			WaitTime = RandomNumberGenerator.RandiRange(15,20),
			OneShot = false,
			Autostart = true,
		};
		WaterTimer.Timeout += () =>
		{
			var area = ExpandedAreas[RandomNumberGenerator.RandiRange(0,ExpandedAreas.Count-1)];
			var x = RandomNumberGenerator.RandiRange((int)area.GlobalPosition.X - 40, (int)area.GlobalPosition.X + 40);
			var y = RandomNumberGenerator.RandiRange((int)area.GlobalPosition.Y - 40, (int)area.GlobalPosition.Y + 40);
			SpawnWater(new Vector2(x,y));
			WaterTimer.WaitTime = RandomNumberGenerator.RandiRange(15, 20);
		};
		AddChild(WaterTimer);
		
	}
	public void UpdateMoney(int amount)
	{
		_userInterface.Money += amount;
	}
	public int GetMoney()
	{
		return _userInterface.Money;
	} 

	public Water SpawnWater(Vector2 position)
	{
		var water = _waterScene.Instantiate<Water>();
		water.Position = position;
		AddChild(water);
		water.OnDropped();
		return water;
	}
	public void SetUpExpansionZone(Vector2 position)
	{
		var newLand = _plotAreaScene.Instantiate<PlotArea>();
		newLand.GlobalPosition = position;
		AddChild(newLand);
		ExpandLand(newLand);
	}
	
	public void ExpandLand(PlotArea area)
	{
		ExpandedAreas.Add(area);
		var gridPosition = GrassLayer.LocalToMap(area.GlobalPosition- TileMapValues.LandSize/2);
		for(var x = gridPosition.X-1; x < gridPosition.X + TileMapValues.LandTiles+1; x++)
		{
			for(var y = gridPosition.Y-1; y < gridPosition.Y + TileMapValues.LandTiles+1; y++)
			{
				GrassTiles.Add(new Vector2I(x,y));
				GrassLayer.SetCellsTerrainConnect(GrassTiles, 0, 0);
			}
		}

		foreach (var expandedArea in ExpandedAreas)
		{
			expandedArea.DisableButtons(area);
			area.DisableButtons(expandedArea);
		}
	}

	private void OnPlacePlot(Building plot)
	{
		PrepBuilding(plot);
		AddChild(plot);
		plot.GetNode<Sprite2D>("Sprite2D").Visible = false;
		if (!PlotTiles.Contains(plot.GridPosition))
		{
			PlotTiles.Add(plot.GridPosition);
			PlotLayer.SetCellsTerrainConnect(PlotTiles, 0, 0);
		}
		_userInterface.ActiveProduct = null;
	}

	private void PrepBuilding(Building building)
	{
		building.GameScene = this;
		building.BuildingInstance.GameScene = this;
		building.GlobalPosition = PlotLayer.MapToLocal(building.GridPosition);
		BuildingsOnGrid[building.GridPosition] = building.BuildingInstance;
		building.OnShowBuildingInfo += _userInterface.OnShowBuildingInfo;
	}
	

	private void OnPlaceBuilding(Building building)
	{

		OnBuildingPlaced += building.UpdateAdjacentBuildings;
		if(building.BuildingInstance is Launcher launcher)
		{
			launcher.OnTargetSelection += _userInterface.OnTargetSelectionView;
			launcher.OnTargetUpdate += _userInterface.OnTargetUpdate;
		}
		PrepBuilding(building);
		AddChild(building);
		_userInterface.ActiveProduct = null;
		EmitSignal(SignalName.OnBuildingPlaced);
		if (building.BuildingInstance is Launcher)
		{
			Console.WriteLine("Water tank placed");
		}
	}
	
	private void OnPlacePlant(Plant plant)
	{
		if (BuildingsOnGrid[plant.GridPosition] is Plot plot)
		{
			AddChild(plant);
			plot.Plant = plant;
		}
		_userInterface.ActiveProduct = null;
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (TargetSetMode)return;

		if(@event.IsActionPressed(Inputs.LeftClick))
		{
			Console.WriteLine("gamescene input left click");
			Dragging = true;
			FocusedDraggable?.OnDragged();
		}

		if (@event.IsActionReleased(Inputs.LeftClick))
		{
			Dragging = false;
			FocusedDraggable?.OnDropped();
			FocusedDraggable = null;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!Dragging || FocusedDraggable == null) return;
		
		var deltaFloat = (float)delta;
		if (FocusedDraggable is RigidBody2D rigidBody2D && GetGlobalMousePosition().DistanceTo(FocusedDraggable.GlobalPosition) > 5)
		{
			rigidBody2D.ApplyForce((GetGlobalMousePosition() - rigidBody2D.GlobalPosition) * 5000*deltaFloat);
		}
	}

	public void ToggleMode()
	{
		_wireView.Visible = !_wireView.Visible;
	}
	
	public PlotArea GetPlotAreaAt(Vector2 position)
	{
		foreach (var area in ExpandedAreas)
		{
			var minx = area.GlobalPosition.X - TileMapValues.LandSize.X/2f;
			var maxx = area.GlobalPosition.X + TileMapValues.LandSize.X/2f;
			var miny = area.GlobalPosition.Y - TileMapValues.LandSize.Y/2f;
			var maxy = area.GlobalPosition.Y + TileMapValues.LandSize.Y/2f;
			if(position.X > minx && position.X < maxx && 
			   position.Y > miny && position.Y < maxy)
			{
				return area;
			}
		}
		return null;
	}
}
