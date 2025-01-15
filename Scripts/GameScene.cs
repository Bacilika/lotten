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
	public Array<Vector2I> PlotTiles = [];
	public Array<Vector2I> GrassTiles = [];
	public System.Collections.Generic.Dictionary<Vector2I,Building> BuildingsOnGrid  = new();
	public IDraggable FocusedDraggable;
	public bool Dragging;
	public Timer WaterTimer;
	public RandomNumberGenerator RandomNumberGenerator = new ();
	private PackedScene _waterScene = ResourceLoader.Load<PackedScene>("res://Scenes/Water.tscn");
	private Control _wireView;
	public List<PlotArea> ExpandedAreas = [];

	public override void _Ready()
	{
		var window = GetTree().Root;
		window.Mode = Window.ModeEnum.Windowed;
		window.Size = new Vector2I(1280, 720);
		window.MoveToCenter();
		_userInterface = GetNode<UI>("UI");
		_userInterface.OnPlaceBuilding += OnPlaceBuilding;
		_userInterface.OnPlacePlot += OnPlacePlot;
		_userInterface.OnPlacePlant += OnPlacePlant;
		GrassLayer = GetNode<TileMapLayer>("GrassLayer");
		PlotLayer = GetNode<TileMapLayer>("PlotLayer");
		SetUpExpansionZones();
		SetUpTimers();
		_wireView = GetNode<Control>("WireView");
		
	}

	public void SetUpTimers()
	{
		WaterTimer = new Timer
		{
			WaitTime = RandomNumberGenerator.RandiRange(1,2),
			OneShot = false,
			Autostart = true,
		};
		WaterTimer.Timeout += () =>
		{
			var area = ExpandedAreas[RandomNumberGenerator.RandiRange(0,ExpandedAreas.Count-1)];
			var x = RandomNumberGenerator.RandiRange((int)area.GlobalPosition.X - 40, (int)area.GlobalPosition.X + 40);
			var y = RandomNumberGenerator.RandiRange((int)area.GlobalPosition.Y - 40, (int)area.GlobalPosition.Y + 40);
			SpawnWater(new Vector2(x,y));
			WaterTimer.WaitTime = RandomNumberGenerator.RandiRange(1, 2);
		};
		AddChild(WaterTimer);
		
	}

	public Water SpawnWater(Vector2 position)
	{
		var water = _waterScene.Instantiate<Water>();
		water.Position = position;
		AddChild(water);
		return water;
	}
	private void SetUpExpansionZones()
	{
		var plotArea11 = GetNode<PlotArea>("PlotArea11");
		var plotArea00 = GetNode<PlotArea>("PlotArea00");
		var plotArea10 = GetNode<PlotArea>("PlotArea10");
		var plotArea01 = GetNode<PlotArea>("PlotArea01");
		List<PlotArea> plotAreas = [plotArea11,plotArea00,plotArea10,plotArea01];
		
		foreach (var plotArea in plotAreas)
		{
			plotArea.OnPlotAreaExpanded += ExpandLand;
		}
		
		plotArea11.SetUp(plotArea10,null,plotArea01,null);
		plotArea00.SetUp(null,plotArea01,null,plotArea10);
		plotArea01.SetUp(plotArea00,null,null,plotArea11);
		plotArea10.SetUp(null,plotArea11,plotArea00,null);
		
		plotArea11.EmitSignal(PlotArea.SignalName.OnPlotAreaExpanded, plotArea11);
		
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

	private void ExpandLand(PlotArea area)
	{
		ExpandedAreas.Add(area);
		var gridPosition = PlotLayer.LocalToMap(area.GlobalPosition- new Vector2(40,40));
		for(var x = gridPosition.X; x < gridPosition.X + 5; x++)
		{
			for(var y = gridPosition.Y; y < gridPosition.Y + 5; y++)
			{
				GrassTiles.Add(new Vector2I(x,y));
				GrassLayer.SetCellsTerrainConnect(GrassTiles, 0, 0);
			}
		}
	}

	private void OnPlacePlot(Building plot)
	{
		AddChild(plot);
		plot.BuildingInstance._gameScene = this;
		plot.GetNode<Sprite2D>("Sprite2D").Visible = false;
		BuildingsOnGrid[plot.GridPosition] = plot.BuildingInstance;
		var gridPosition = PlotLayer.LocalToMap(GetGlobalMousePosition());
		plot.GlobalPosition = gridPosition * 16 + new Vector2(8, 8);
		if (!PlotTiles.Contains(gridPosition))
		{
			PlotTiles.Add(gridPosition);
			PlotLayer.SetCellsTerrainConnect(PlotTiles, 0, 0);
		}
		_userInterface.ActiveProduct = null;
	}

	private void OnPlaceBuilding(Building building)
	{
		BuildingsOnGrid[building.GridPosition] = building;
		building.BuildingInstance._gameScene = this;
		_userInterface.ActiveProduct = null;
		AddChild(building);
	}

	public override void _Input(InputEvent @event)
	{
		if(@event.IsActionPressed(Inputs.LeftClick))
		{
			Dragging = true;
			FocusedDraggable?.OnDragged();
			foreach (var kvp in BuildingsOnGrid)
			{
				var building = kvp.Value;
				if (building.focused)
				{
					building.OnClick();
				}
			}
		}

		if (@event.IsActionReleased(Inputs.LeftClick))
		{
			Dragging = false;
			FocusedDraggable?.OnDropped();
			FocusedDraggable = null;
		}
	}
	
	public override void _Process(double delta)
	{
		if (Dragging && FocusedDraggable != null)
		{
			var deltaFloat = (float)delta;
			FocusedDraggable.LerpedPosition = GetGlobalMousePosition();
			FocusedDraggable.GlobalPosition = FocusedDraggable.GlobalPosition.Lerp(FocusedDraggable.LerpedPosition,  15* deltaFloat);
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
			var minx = area.GlobalPosition.X - 40;
			var maxx = area.GlobalPosition.X + 40;
			var miny = area.GlobalPosition.Y - 40;
			var maxy = area.GlobalPosition.Y + 40;
			if(position.X > minx && position.X < maxx && 
			   position.Y > miny && position.Y < maxy)
			{
				return area;
			}
		}
		return null;
	}
}
