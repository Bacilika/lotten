using System;
using System.Collections.Generic;
using Godot;

namespace Lotten.Scripts.Products.Buildings;

public partial class Building: Product
{
	public Building BuildingInstance;
	public Building BuildingScene;
	public GameScene _gameScene;
	public Func<Building> SubtypeGenerator;
	public DrawableCircle AreaOfInfluence;
	public bool focused;
	public List<Button> BuildingActions = []; //TODO: QueueFree all buttons

	[Signal]
	public delegate void OnShowBuildingInfoEventHandler(Building building, bool visible);
	public override void _Ready()
	{
		PlantSprite = GetNode<Sprite2D>("Sprite2D");
		BodyEntered += OnBodyEntered;
		BodyExited += OnBodyExited;
		MouseEntered += OnMouseEntered;
		MouseExited += OnMouseExited;
		BuildingInstance?.ReadyInstance();
		
		
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
	public override void _PhysicsProcess(double delta)
	{
		BuildingInstance?.PhysicsTick(delta);
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
			
			
		}
	}
	
	public virtual void ReadyInstance()
	{
		
	}

	public virtual void Tick(double delta)
	{
		
	}
	public virtual void PhysicsTick(double delta)
	{
		
	}
	public override void _Draw()
	{
		if(AreaOfInfluence != null)
		{
			DrawCircle(AreaOfInfluence.Position, AreaOfInfluence.Radius, AreaOfInfluence.Color);
		}
	}
	public override void _UnhandledInput(InputEvent @event)
	{
		
		BuildingInstance?.CustomInput(@event);
		if(focused)
		{
			if(@event.IsActionPressed(Inputs.LeftClick))
			{
				OnClick();
			}
			if(@event.IsActionPressed(Inputs.RightClick))
			{
				EmitSignal(SignalName.OnShowBuildingInfo, this, true);
			}
			
		}
		else
		{
			if(@event.IsActionPressed(Inputs.RightClick))
			{
				EmitSignal(SignalName.OnShowBuildingInfo, this, false);
			}
		}
		
		
	}
	public virtual void CustomInput(InputEvent @event)
	{

	}
	
	




}
