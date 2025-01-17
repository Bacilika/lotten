using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Lotten.Scripts;
using Lotten.Scripts.Products.Buildings;

namespace Lotten.Scripts.Products;

public partial class Plant : Product, IDraggable
{
	public List<Texture2D> Sprites = [];
	
	private GameScene _gameScene;
	public Timer GrowthTimer;
	public int GrowthTime = 5;
	public int GrowthStage = 0;
	public bool Draggable = false;
	public bool Harvested = false;
	public Plot Plot;
	
	public Vector2 LerpedPosition { get; set; }
	public Vector2 PositionAtDragStart { get; set; }

	public override void _Ready()
	{
		
			PlantSprite = GetNode<Sprite2D>("Sprite2D");
			if(Sprites.Count > 0) PlantSprite.Texture = Sprites.First();
			MouseEntered += OnMouseEntered;
			MouseExited += OnMouseExited;
			
			GrowthTimer = new Timer
			{
				OneShot = true,
			};
			AddChild(GrowthTimer);
			

	}
	public override void OnExitTree()
	{
		GrowthTimer.QueueFree();
		foreach (var sprite in Sprites)
		{
			sprite.Dispose();
		}
	}

	public override void CopyValuesFrom(Product product)
	{
		ProductName = product.ProductName;
		Description = product.Description;
		Cost = product.Cost;
		SellPrice = product.SellPrice;
		GridPosition = product.GridPosition;
		if (product is Plant plant)
		{
			GrowthTime = plant.GrowthTime;
			Sprites = plant.Sprites;
			
		}
	}
	

	private void GrowthTimerOnTimeout()
	{
		
		
		GrowthStage++;
		if (GrowthStage == Sprites.Count-2)
		{
			Plot.Dry();
			Draggable = true;
			PlantSprite.Texture = Sprites[GrowthStage];
			return;
		}
		PlantSprite.Texture = Sprites[GrowthStage];
		GrowthTimer.Start();
	}
	public void StartGrowTimer()
	{
		if( !Draggable && GrowthTimer.IsStopped())
			GrowthTimer.Start();
	}

	public void OnPlanted()
	{
		_gameScene = GetParent<GameScene>();
		PlantSprite.Texture = Sprites[1];
		GrowthStage = 1;
		GrowthTimer.WaitTime = GrowthTime;
		GrowthTimer.Timeout += GrowthTimerOnTimeout;
		if(Plot.IsWatered)
			GrowthTimer.Start();
	}

	public void OnMouseEntered()
	{
		if (Draggable && !_gameScene.Dragging)
		{
			_gameScene.FocusedDraggable = this;
		}
	}

	public void OnMouseExited()
	{
		if(Draggable && !_gameScene.Dragging)
			_gameScene.FocusedDraggable = null;
	}
	public void OnDragged()
	{
		if (Draggable && !Harvested)
		{
			Harvest();
		}

	}
	public void OnDropped()
	{
	}

	public void Harvest()
	{
		Harvested = true;
		var physicsPlant = ResourceLoader.Load<PackedScene>("res://Scenes/physics_plant.tscn").Instantiate<PhysicsPlant>();
		physicsPlant.GlobalPosition = GlobalPosition;
		_gameScene.AddChild(physicsPlant);
		physicsPlant.PlantSprite.Texture = Sprites.Last();
		physicsPlant.SellPrice = SellPrice;
		Plot.Plant = null;
		_gameScene.FocusedDraggable = physicsPlant;
		_gameScene.RemoveChild(this);
		//OnExitTree();
		QueueFree();
	}
}
