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
	public bool InAir { get; set; }
	public string PlantName;

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
			if (!InAir && _lastPlotArea != null)
			{
				var vel = (_lastPlotArea.GlobalPosition - GlobalPosition) / 10;
				ApplyForce(vel * 50);
			}
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
		LinearDamp = 15;
		PositionAtDragStart = GlobalPosition;
		SetCollisionLayer(1);
		
		_shadowLerpedPosition = Vector2.Down * 5;
		InAir = true;
	}
	public void OnDropped()
	{
		LinearDamp = 3;
		ApplyForce(Vector2.Down * 100);
		SetCollisionLayer(1|2);
		_shadowLerpedPosition = Vector2.Zero;
		InAir = false;
	}

}
