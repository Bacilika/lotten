using Godot;
namespace Lotten.Scripts;
public partial class PhysicsPlant : RigidBody2D, IDraggable
{
	
	public int SellPrice;
	public Sprite2D PlantSprite;
	private GameScene _gameScene;
	private Vector2 _shadowLerpedPosition;
	public Sprite2D Shadow;
	public Vector2I GridPosition;
	public void OnDropped()
	{
		ApplyForce(Vector2.Down * 10000);
		_shadowLerpedPosition = Vector2.Zero;
	}

	public Vector2 LerpedPosition { get; set; }
	public Vector2 PositionAtDragStart { get; set; }

	public override void _Ready()
	{
		PlantSprite = GetNode<Sprite2D>("Sprite2D");
		Shadow = GetNode<Sprite2D>("shadow");
		_gameScene = GetParent<GameScene>();
		InputPickable = true;
		MouseEntered += OnMouseEntered;
		MouseExited += OnMouseExited;
		
	}

	public override void _PhysicsProcess(double delta)
	{
		MoveAndCollide(new Vector2(0, 0));
	}

	public override void _Process(double delta)
	{
		var deltaFloat = (float)delta;
		Shadow.Position = Shadow.Position.Lerp(_shadowLerpedPosition, 15 * deltaFloat);
	}

	public void OnMouseEntered()
	{
		if (!_gameScene.Dragging)
		{
			_gameScene.FocusedDraggable = this;
		}
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
	}


}
