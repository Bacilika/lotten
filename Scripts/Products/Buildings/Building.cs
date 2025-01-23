using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Lotten.Scripts.Products.Buildings;

public partial class Building: Product
{
	public Building BuildingInstance;
	public Building BuildingScene;
	public GameScene GameScene;
	public Func<Building> SubtypeGenerator;
	public DrawableCircle AreaOfInfluence;
	private bool _focused;
	public List<Button> BuildingActions = []; //TODO: QueueFree all buttons
	public List<Label> BuildingLabels = [];
	public List<Building> AdjacentBuildings = [];
	protected List<string> CanReceive = [];
	
	public List<IDraggable> _storedIDraggables = [];
	protected int _capacity = 20;
	public Label _contentLabel;
	public string BuildingName;
	
	protected void UpdateContentLabel()
	{
		_contentLabel.Text =  $"Capacity: {_storedIDraggables.Count}/{_capacity}";
	}

	[Signal]
	public delegate void OnShowBuildingInfoEventHandler(Building building, bool visible);


	public IDraggable RemoveDraggable()
	{
		if(_storedIDraggables.Count == 0) return null;
		var draggable = _storedIDraggables.First();
		_storedIDraggables.RemoveAt(0);
		draggable.GlobalPosition = GlobalPosition;
		return draggable;
	}	
	
	public override void _Ready()
	{
		PlantSprite = GetNode<Sprite2D>("Sprite2D");
		BodyEntered += OnBodyEntered;
		BodyExited += OnBodyExited;
		MouseEntered += OnMouseEntered;
		MouseExited += OnMouseExited;
		SetBuildingLabels();
		BuildingInstance?.SetCanReceive();
		BuildingInstance?.SetBuildingActions();
		
		BuildingInstance?.ReadyInstance();
		
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
	
	
	
	public void UpdateAdjacentBuildings()
	{
		AdjacentBuildings.Clear();
		foreach (var direction in new List<Vector2I> {Vector2I.Up, Vector2I.Down, Vector2I.Left, Vector2I.Right})
		{
			var adjacentPosition = GridPosition + direction;
			if (GameScene.BuildingsOnGrid.TryGetValue(adjacentPosition, out var adjacentBuilding))
			{
				AdjacentBuildings.Add(adjacentBuilding);
			}
		}
	}
	public void SendToBuilding(IDraggable draggable)
	{
		if (BuildingInstance is Launcher)
		{
			_storedIDraggables.Add(draggable);
			return;
		}
		foreach (var adjacentBuilding in AdjacentBuildings)
		{
			if(adjacentBuilding.GetType() == BuildingInstance.GetType()) continue;
				
			if (adjacentBuilding.Receive(draggable))
			{
				MoveDraggableTo(draggable, adjacentBuilding.BuildingScene);
				return;
				
			}
		}
		_storedIDraggables.Add(draggable);
		
	}
	public void MoveDraggableTo(IDraggable draggable, Building target)
	{
		target._storedIDraggables.Add(draggable);
	}
	
	public bool Receive(IDraggable product)
	{
		return CanReceive.Contains(product.GetType().Name);
	}
	
	/*
	 *   virtual methods that are not to be modified
	 */
	
	public override void _UnhandledInput(InputEvent @event)
	{
		
		BuildingInstance?.CustomInput(@event);
		if(_focused)
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
	
	protected virtual void CustomInput(InputEvent @event) {}
	
	protected virtual void ReadyInstance()
	{}

	protected virtual void Tick(double delta)
	{
		
	}
	protected virtual void PhysicsTick(double delta)
	{
		
	}
	public override void _Draw()
	{
		if(AreaOfInfluence != null)
		{
			DrawCircle(AreaOfInfluence.Position, AreaOfInfluence.Radius, AreaOfInfluence.Color);
		}
	}
	

	protected virtual void SetCanReceive(){}
	protected virtual void SetBuildingActions(){}
	

	protected virtual void SetBuildingLabels()
	{
		_contentLabel = new Label();
		BuildingLabels.Add(_contentLabel);
		UpdateContentLabel();
		BuildingInstance?.SetBuildingLabels();
	}

	private void OnMouseExited()
	{
		_focused = false;
	}

	private void OnMouseEntered()
	{
		_focused = true;
	}
	protected virtual void OnBodyExited(Node2D body)
	{
		BuildingInstance?.OnBodyExited(body);
	}

	protected virtual void OnBodyEntered(Node2D body)
	{
		BuildingInstance?.OnBodyEntered(body);
	}

	protected virtual void OnClick()
	{
		BuildingInstance?.OnClick();
	}
	public override void _PhysicsProcess(double delta)
	{
		BuildingInstance?.PhysicsTick(delta);
	}

	public override void _Process(double delta)
	{
		UpdateContentLabel();
		BuildingInstance?.Tick(delta);
		if (_storedIDraggables.Count > 0)
		{
			var draggable = RemoveDraggable();
			SendToBuilding(draggable);
		}
			
		
	}
}
