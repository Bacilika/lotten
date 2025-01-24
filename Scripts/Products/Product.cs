﻿using Godot;

namespace Lotten.Scripts.Products;

public abstract partial class Product : Area2D
{
    public string ProductName;
    public string Description;
    public int Cost;
    public int SellPrice;
    public Sprite2D PlantSprite;
    
    public Vector2I GridPosition;
    public Vector2 LerpedPosition;

    public override void _Ready()
    {
        LerpedPosition = Position;
    }

    public abstract void CopyValuesFrom(Product product);

    public virtual void OnExitTree()
    {
        //PlantSprite.QueueFree();
    }

}