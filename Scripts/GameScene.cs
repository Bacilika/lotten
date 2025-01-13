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
	public Area2D PlotArea;
	public bool MouseInBounds;

	public IDraggable ProductOutOfBounds;
	
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
		List<Area2D> plotAreas = [GetNode<Area2D>("PlotArea11"),GetNode<Area2D>("PlotArea00"),GetNode<Area2D>("PlotArea10"),GetNode<Area2D>("PlotArea01")];
		foreach (var plotArea in plotAreas)
		{
			plotArea.MouseEntered += OnMouseEnteredPlotArea;
			plotArea.MouseExited += OnMouseExitedPlotArea;
		}
		var plotArea11 = GetNode<Area2D>("PlotArea11");
		var plotArea00 = GetNode<Area2D>("PlotArea00");
		var plotArea10 = GetNode<Area2D>("PlotArea10");
		var plotArea01 = GetNode<Area2D>("PlotArea01");

		
		
		ExpandedAreas.Add(plotArea11);
		
		//from plotArea11
		PreparePlotArea(plotArea01,plotArea11.GetNode<TextureButton>("ExpandLeft"));
		PreparePlotArea(plotArea10,plotArea11.GetNode<TextureButton>("ExpandUp"));
		
		//from plotArea01
		PreparePlotArea(plotArea00, plotArea01.GetNode<TextureButton>("ExpandUp"));
		
		//from plotArea10
		PreparePlotArea(plotArea00, plotArea10.GetNode<TextureButton>("ExpandLeft"));
		
		//from plotArea00
		PreparePlotArea(plotArea01, plotArea00.GetNode<TextureButton>("ExpandDown"));
		PreparePlotArea(plotArea10, plotArea00.GetNode<TextureButton>("ExpandRight"));
	}



	public void OnMouseEnteredPlotArea()
	{
		MouseInBounds = true;
		
	}
	public void OnMouseExitedPlotArea()
	{
		MouseInBounds = false;	
	}
	
	public void PreparePlotArea(Area2D area2D, TextureButton button)
	{
		button.Visible = true;
		button.Pressed += () =>
		{
			button.Visible = false;
			area2D.Visible = true;
			ExpandLand(area2D);
			ExpandedAreas.Add(area2D);
		};
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

	private void ExpandLand(Area2D area)
	{
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
			ProductOutOfBounds = null;
			FocusedDraggable?.OnDragged();

		}

		if (@event.IsActionReleased(Inputs.LeftClick))
		{
			Dragging = false;
			if (!MouseInBounds && FocusedDraggable != null)
			{
				ProductOutOfBounds = FocusedDraggable;
				
				
			}
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
		else if (ProductOutOfBounds is not null)
		{
			var deltaFloat = (float)delta;
			ProductOutOfBounds.LerpedPosition = ProductOutOfBounds.PositionAtDragStart;
			ProductOutOfBounds.GlobalPosition = ProductOutOfBounds.GlobalPosition.Lerp(ProductOutOfBounds.LerpedPosition, 10 * deltaFloat);
			
		}

	}
	
	public bool InBounds(Vector2 position)
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
				return true;
			}
		}

		return false;
	}
}
