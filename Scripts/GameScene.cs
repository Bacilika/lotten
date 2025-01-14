using Godot;
using System;
using System.Collections.Generic;
using Godot.Collections;
using Lotten.Scripts.Products;
namespace Lotten.Scripts;
public partial class GameScene : Node2D
{
	private UI UI;
	public TileMapLayer GrassLayer;
	public TileMapLayer PlotLayer;
	public Array<Vector2I> PlotTiles = [];
	public Array<Vector2I> GrassTiles = [];
	public System.Collections.Generic.Dictionary<Vector2I,Building> BuildingsOnGrid  = new();
	public IDraggable FocusedDraggable;
	public bool Dragging;
	public bool MouseInBounds;
	
	public List<Area2D> ExpandedAreas = [];

	public override void _Ready()
	{
		var window = GetTree().Root;
		window.Mode = Window.ModeEnum.Windowed;
		window.Size = new Vector2I(1280, 720);
		window.MoveToCenter();
		UI = GetNode<UI>("UI");
		UI.OnPlaceBuilding += OnPlaceBuilding;
		UI.OnPlacePlot += OnPlacePlot;
		UI.OnPlacePlant += OnPlacePlant;
		GrassLayer = GetNode<TileMapLayer>("GrassLayer");
		PlotLayer = GetNode<TileMapLayer>("PlotLayer");
		SetUpExpansionZones();
		
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
			plotArea.MouseEntered += OnMouseEnteredPlotArea;
			plotArea.MouseExited += OnMouseExitedPlotArea;
			plotArea.OnPlotAreaExpanded += ExpandLand;
		}
		
		plotArea11.SetUp(plotArea10,null,plotArea01,null);
		plotArea00.SetUp(null,plotArea01,null,plotArea10);
		plotArea01.SetUp(plotArea00,null,null,plotArea11);
		plotArea10.SetUp(null,plotArea11,plotArea00,null);
		
		plotArea11.EmitSignal(PlotArea.SignalName.OnPlotAreaExpanded, plotArea11);
		
	}
	
	public void OnMouseEnteredPlotArea()
	{
		MouseInBounds = true;
	}
	public void OnMouseExitedPlotArea()
	{
		MouseInBounds = false;	
	}
	
	private void OnPlacePlant(Plant plant)
	{
		Console.WriteLine("Plant placed");
		if (BuildingsOnGrid[plant.GridPosition] is Plot plot)
		{
			AddChild(plant);
			plot.Plant = plant;
		}
		UI.ActiveProduct = null;
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
		Console.WriteLine("Plot placed");
		UI.ActiveProduct = null;
	}

	private void OnPlaceBuilding(Building building)
	{
		BuildingsOnGrid[building.GridPosition] = building;
		Console.WriteLine("Building placed");
		UI.ActiveProduct = null;
	}

	public override void _Input(InputEvent @event)
	{
		if(@event.IsActionPressed(Inputs.LeftClick))
		{
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
	
	public override void _Process(double delta)
	{
		if (Dragging && FocusedDraggable != null)
		{
			var deltaFloat = (float)delta;
			FocusedDraggable.LerpedPosition = GetGlobalMousePosition();
			FocusedDraggable.GlobalPosition = FocusedDraggable.GlobalPosition.Lerp(FocusedDraggable.LerpedPosition, 15 * deltaFloat);
			
		}
		

	}
	
	public Area2D InBounds(Vector2 position)
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
