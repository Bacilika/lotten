using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Lotten.Scripts;
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

	public override void CopyValuesFrom(Product product)
	{
		ProductName = product.ProductName;
		Description = product.Description;
		Cost = product.Cost;
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
			Console.WriteLine("Plant fully grown");
			Draggable = true;
			//TODO: Remove plant from plot
		}
		else
		{
			GrowthTimer.Start();
		}
		PlantSprite.Texture = Sprites[GrowthStage];
		
		
	}

	public void OnPlanted()
	{
		_gameScene = GetParent<GameScene>();
		PlantSprite.Texture = Sprites[1];
		GrowthTimer.WaitTime = GrowthTime;
		GrowthTimer.Timeout += GrowthTimerOnTimeout;
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
		_gameScene.AddChild(physicsPlant);
		physicsPlant.PlantSprite.Texture = Sprites.Last();
		physicsPlant.SellPrice = SellPrice;
		Plot.Plant = null;
		_gameScene.FocusedDraggable = physicsPlant;
		_gameScene.RemoveChild(this);
		QueueFree();
	}
}
