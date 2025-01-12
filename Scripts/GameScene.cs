using Godot;
using System;
using Godot.Collections;
using Lotten.Scripts.Products;
namespace Lotten.Scripts;
public partial class GameScene : Node2D
{
	private UI UI;
	public TileMapLayer GrassLayer;
	public TileMapLayer PlotLayer;
	public Array<Vector2I> PlotTiles = [];
	public System.Collections.Generic.Dictionary<Vector2I,Building> BuildingsOnGrid  = new();
	public IDraggable FocusedDraggable;
	public bool Dragging;
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
}
