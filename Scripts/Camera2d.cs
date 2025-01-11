using Godot;
namespace Lotten.Scripts;
public partial class Camera2d : Godot.Camera2D
{
	private Vector2 _position;
	private Vector2 _dragStartCameraPosition = Vector2.Zero;
	private Vector2 _dragStartMousePosition = Vector2.Zero;
	private bool _isDragging;

	private Vector2 _zoom;
	private const int ZoomMaxStep = 2; //how many times we can zoom out
	private const int ZoomMinStep = 0; //how many times we can zoom in
	private int _zoomStep = 1;

	private int PanStep
	{
		get
		{
			return (int)(80 * (_zoomStep == 0 ? 0.5f : _zoomStep));
		}
	}

	public override void _Ready()
	{
		// Set minimum size for window rescaling.
		GetTree().Root.MinSize =
			new Vector2I(ProjectSettings.GetSetting(Constants.ViewPortWidthSettingPath, 0).AsInt32(),
				ProjectSettings.GetSetting(Constants.ViewPortHeightSettingPath, 0).AsInt32());

		_zoom = Zoom;
		_position = Position;
	}

	public override void _Process(double delta)
	{
		var deltaFloat = (float)delta;
		Zoom = Zoom.Lerp(_zoom, 20 * deltaFloat);
		Position = Position.Lerp(_position, 20 * deltaFloat);
	}

	public override void _Input(InputEvent @event)
	{
		// Zooming behaviour
		if (@event.IsActionPressed(Inputs.ZoomIn) && _zoomStep > ZoomMinStep)
		{
			_zoomStep--;
			_zoom *= 2;
		}
		if (@event.IsActionPressed(Inputs.ZoomOut) && _zoomStep < ZoomMaxStep)
		{
			_zoomStep++;
			_zoom /= 2;
		}

		// Panning behaviour
		if (@event.IsAction("camera_move_left"))
		{
			_position.X -= PanStep;
		}
		if (@event.IsAction("camera_move_right"))
		{
			_position.X += PanStep;
		}
		if (@event.IsAction("camera_move_up"))
		{
			_position.Y -= PanStep;
		}
		if (@event.IsAction("camera_move_down"))
		{
			_position.Y += PanStep;
		}

		if (!_isDragging && @event.IsActionPressed(Inputs.CameraPan))
		{
			_dragStartMousePosition = GetViewport().GetMousePosition();
			_dragStartCameraPosition = Position;
			_isDragging = true;
		}

		if (_isDragging && @event.IsActionReleased(Inputs.CameraPan)) _isDragging = false;

		if (_isDragging)
		{
			var moveVector = GetViewport().GetMousePosition() - _dragStartMousePosition;
			_position = _dragStartCameraPosition - moveVector * (1 / Zoom.X);
			_position = _position.Floor();
		}
	}
}
