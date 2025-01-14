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
	private PlotArea _lastPlotArea;
	private bool _inAir;
	public void OnDropped()
	{
		ApplyForce(Vector2.Down * 10000);
		_shadowLerpedPosition = Vector2.Zero;
		_inAir = false;
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
		_lastPlotArea = _gameScene.GetPlotAreaAt(GlobalPosition);
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
		_inAir = true;
	}
}
