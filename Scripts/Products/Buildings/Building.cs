using System;
using Godot;

namespace Lotten.Scripts.Products;

public partial class Building: Product
{
	public Building BuildingInstance;
	public Building BuildingScene;
	public GameScene _gameScene;
	public Func<Building> SubtypeGenerator;
	public DrawableCircle AreaOfInfluence;
	public bool focused;
	public override void _Ready()
	{
		PlantSprite = GetNode<Sprite2D>("Sprite2D");
		BodyEntered += OnBodyEntered;
		BodyExited += OnBodyExited;
		MouseEntered += OnMouseEntered;
		MouseExited += OnMouseExited;
		ReadyInstance();
	
	}

	public void OnMouseExited()
	{
		focused = false;
	}

	public void OnMouseEntered()
	{
		focused = true;
	}

	public virtual void OnClick()
	{
		BuildingInstance?.OnClick();
	}

	public override void _Process(double delta)
	{
		BuildingInstance?.Tick(delta);
	}

	public virtual void OnBodyExited(Node2D body)
	{
		BuildingInstance?.OnBodyExited(body);
	}

	public virtual void OnBodyEntered(Node2D body)
	{
		BuildingInstance?.OnBodyEntered(body);
	}

	public override void CopyValuesFrom(Product product)
	{
		ProductName = product.ProductName;
		Description = product.Description;
		GridPosition = product.GridPosition;
		GlobalPosition = product.GlobalPosition;
		Cost = product.Cost;
		if (product is Building building)
		{ 
			BuildingInstance = building.SubtypeGenerator();
			BuildingInstance.BuildingScene = this;
			BuildingInstance.ReadyInstance();
		}
		
	}
	
	public virtual void ReadyInstance()
	{
		
	}

	public virtual void Tick(double delta)
	{
		
	}
	public override void _Draw()
	{
		if(AreaOfInfluence != null)
		{
			DrawCircle(AreaOfInfluence.Position, AreaOfInfluence.Radius, AreaOfInfluence.Color);
		}
	}
	
	




}
