using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Lotten.Scripts.Products.Buildings;

public partial class Storage: Building
{

    protected override void SetCanReceive()
    {
        CanReceive = [nameof(PhysicsPlant)];
        _storedIDraggables = null;
    }

    protected override void SetBuildingActions()
    {
        BuildingActions = [];
    }
    
    protected override void OnBodyEntered(Node2D body)
    {
        if (body is PhysicsPlant plant && BuildingScene._storedIDraggables.Count < _capacity)
        {
            BuildingScene._storedIDraggables.Add(plant);
            GameScene.RemoveChild(plant);
        }
    }
    protected override void OnClick()
    {
        if(BuildingScene._storedIDraggables.Count < 1 || GameScene.FocusedDraggable != null) return;
        var plant = (PhysicsPlant) _storedIDraggables.First();
        BuildingScene._storedIDraggables.Remove(plant);
        GameScene.AddChild(plant);
        GameScene.FocusedDraggable = plant;
        GameScene.Dragging = true;
        plant.OnDragged();
    }
}