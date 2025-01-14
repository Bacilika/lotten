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
	private bool _inAir;
	

	public void OnDropped()
	{
		SetCollisionLayer(1|2);
		ApplyForce(Vector2.Down * 10000);
		_shadowLerpedPosition = Vector2.Zero;
		_inAir = false;
	}

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
		if (_gameScene.GetPlotAreaAt(GlobalPosition) != null)
		{
			_lastPlotArea = _gameScene.GetPlotAreaAt(GlobalPosition);
		}
		else
		{
			if(!_inAir && _lastPlotArea != null)
				ApplyForce((_lastPlotArea.GlobalPosition - GlobalPosition)*30);
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
		_inAir = true;
	}
}
