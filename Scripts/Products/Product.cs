using Godot;

namespace Lotten.Scripts.Products;

public abstract partial class Product : Area2D
{
    public string ProductName;
    public string Description;
    public Sprite2D PlantSprite;
    public int Cost;
    public Vector2I GridPosition;

    public abstract void CopyValuesFrom(Product product);

}