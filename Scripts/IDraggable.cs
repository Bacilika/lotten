using Godot;

namespace Lotten.Scripts;

public interface IDraggable
{
    void OnMouseEntered();
    void OnMouseExited();

    void OnDragged();

    void OnDropped();
    
    Vector2 LerpedPosition { get; set; }
    Vector2 GlobalPosition { get; set; }
    
}