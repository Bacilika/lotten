using Godot;
using System;
using Lotten.Scripts;
namespace Lotten.Scripts;
public partial class Water : RigidBody2D, IDraggable
{
	private GameScene _gameScene;
	private CollisionShape2D _collisionShape2D;

	public void OnDropped()
	{
		_collisionShape2D.Disabled = false;
		
	}

	public Vector2 LerpedPosition { get; set; }
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_gameScene = GetParent<GameScene>();
		_collisionShape2D = GetNode<CollisionShape2D>("CollisionShape2D");
		MouseEntered += OnMouseEntered;
		MouseExited += OnMouseExited;
		InputPickable = true;
		
		
	}
	

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public override void _PhysicsProcess(double delta)
	{
		MoveAndCollide(new Vector2(0, 0));
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
		_collisionShape2D.Disabled = true;
	}


}
