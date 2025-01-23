using System;
using Godot;

namespace Lotten.Scripts.Products.Buildings;

public partial class SellStation: Building
{
    protected override void SetCanReceive()
    {
        CanReceive = [];
        _storedIDraggables = null;
    }

    protected override void SetBuildingActions()
    {
        BuildingActions = [];
    }

    protected override void SetBuildingLabels()
    {
        BuildingLabels = [];
    }

    protected override void OnBodyEntered(Node2D body)
    {
        if (body is not PhysicsPlant plant) return;
        
        GameScene.RemoveChild(plant);
        plant.QueueFree();
        GameScene.UpdateMoney(plant.SellPrice);
    }
}