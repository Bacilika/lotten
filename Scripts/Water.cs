using Godot;
using System;
using Lotten.Scripts;
namespace Lotten.Scripts;
public partial class Water : RigidBody2D, IDraggable
{
	private GameScene _gameScene;
	private CollisionShape2D _collisionShape2D;
	private Vector2 _shadowLerpedPosition;
	private Sprite2D _shadow;
	private PlotArea _lastPlotArea;
	public bool InAir;
	public bool Sucked = false;

	

	public Vector2 LerpedPosition { get; set; }

	public Vector2 PositionAtDragStart { get; set; }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_gameScene = GetParent<GameScene>();
		_collisionShape2D = GetNode<CollisionShape2D>("CollisionShape2D");
		MouseEntered += OnMouseEntered;
		MouseExited += OnMouseExited;
		InputPickable = true;
		_shadow = GetNode<Sprite2D>("shadow");
		ContactMonitor = true;
		MaxContactsReported = 1;
		_lastPlotArea = _gameScene.GetPlotAreaAt(GlobalPosition);
	}


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		var deltaFloat = (float)delta;
		_shadow.Position = _shadow.Position.Lerp(_shadowLerpedPosition, 15 * deltaFloat);
	}

	public override void _PhysicsProcess(double delta)
	{
		
		MoveAndCollide(new Vector2(0, 0));
		if (_gameScene.GetPlotAreaAt(GlobalPosition) != null) //in bounds
		{
			_lastPlotArea = _gameScene.GetPlotAreaAt(GlobalPosition);
		}
		else //out of bounds
		{
			if (!InAir && _lastPlotArea != null)
			{
				var vel = (_lastPlotArea.GlobalPosition - GlobalPosition) / 10;
				ApplyForce(vel*50);
			}
				
		}
	}

	public void OnMouseEntered()
	{
		if(!_gameScene.Dragging)
			_gameScene.FocusedDraggable = this;
	}

	public void OnMouseExited()
	{
		if(!_gameScene.Dragging)
			_gameScene.FocusedDraggable = null;
	}
	public void OnDragged()
	{
		PositionAtDragStart = GlobalPosition;
		SetCollisionLayer(1);
		_shadowLerpedPosition = Vector2.Down * 5;
		LinearDamp = 15;
		InAir = true;
	}
	public void OnDropped()
	{
		LinearDamp = 3;
		SetCollisionLayer(1|2);
		ApplyForce(Vector2.Down * 100);
		_shadowLerpedPosition = Vector2.Zero;
		InAir = false;
	}
}
