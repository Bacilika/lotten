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

	private void OnPlacePlant(Plant plant)
	{
		Console.WriteLine("Plant placed");
		if (BuildingsOnGrid[plant.GridPosition] is Plot plot)
		{
			plot.Plant = plant;
			AddChild(plant);
		}
		UI.ActiveProduct = null;
	}

	private void OnPlacePlot(Building plot)
	{
		BuildingsOnGrid[plot.GridPosition] = plot.BuildingInstance;
		var gridPosition = PlotLayer.LocalToMap(GetGlobalMousePosition());
		
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
}
