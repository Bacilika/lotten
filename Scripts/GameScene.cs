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
	private PackedScene _plotAreaScene = ResourceLoader.Load<PackedScene>("res://Scenes/plot_area.tscn");
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
		SetUpExpansionZone(Vector2.Zero);
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
	public void SetUpExpansionZone(Vector2 position)
	{
		var newPlot = _plotAreaScene.Instantiate<PlotArea>();
		newPlot.GlobalPosition = position;
		AddChild(newPlot);
		ExpandLand(newPlot);
		
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

	public void ExpandLand(PlotArea area)
	{
		ExpandedAreas.Add(area);
		var gridPosition = GrassLayer.LocalToMap(area.GlobalPosition- new Vector2I(Constants.LandSize/2, Constants.LandSize/2)) ;
		for(var x = gridPosition.X; x < gridPosition.X + Constants.LandTiles; x++)
		{
			for(var y = gridPosition.Y; y < gridPosition.Y + Constants.LandTiles; y++)
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
		AddChild(plot);
		plot.BuildingInstance._gameScene = this;
		plot.GetNode<Sprite2D>("Sprite2D").Visible = false;
		BuildingsOnGrid[plot.GridPosition] = plot.BuildingInstance;
		var gridPosition = PlotLayer.LocalToMap(GetGlobalMousePosition());
		plot.GlobalPosition = gridPosition * Constants.TileSize + new Vector2(Constants.TileSize/2f, Constants.TileSize/2f);
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
			var minx = area.GlobalPosition.X - Constants.LandSize/2f;
			var maxx = area.GlobalPosition.X + Constants.LandSize/2f;
			var miny = area.GlobalPosition.Y - Constants.LandSize/2f;
			var maxy = area.GlobalPosition.Y + Constants.LandSize/2f;
			if(position.X > minx && position.X < maxx && 
			   position.Y > miny && position.Y < maxy)
			{
				return area;
			}
		}
		return null;
	}
}
