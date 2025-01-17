using System;
using Godot;

namespace Lotten.Scripts.Products.Buildings;

public partial class SellStation: Building
{
    public override void OnBodyEntered(Node2D body)
    {
        Console.WriteLine("Body Enterd");
        if (body is PhysicsPlant plant)
        {
            _gameScene.RemoveChild(plant);
            plant.QueueFree();
            _gameScene.UpdateMoney(plant.SellPrice);
        }
    }
}