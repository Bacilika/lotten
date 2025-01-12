using Godot;
namespace Lotten.Scripts;
public partial class PhysicsPlant : RigidBody2D, IDraggable
{
	
	public int SellPrice;
	public Sprite2D PlantSprite;
	private GameScene _gameScene;
	public void OnDropped()
	{
		
	}

	public Vector2 LerpedPosition { get; set; }
	public override void _Ready()
	{
		PlantSprite = GetNode<Sprite2D>("Sprite2D");
		_gameScene = GetParent<GameScene>();
		InputPickable = true;
		MouseEntered += OnMouseEntered;
		MouseExited += OnMouseExited;
	}

	public override void _PhysicsProcess(double delta)
	{
		MoveAndCollide(new Vector2(0, 0));
		
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
		
	}


}
