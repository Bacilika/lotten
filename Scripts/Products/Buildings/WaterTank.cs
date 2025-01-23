using System.Linq;
using Godot;

namespace Lotten.Scripts.Products.Buildings;

public partial class WaterTank: Building
{

    protected override void SetCanReceive()
    {
        CanReceive.Add(nameof(Water));
        _storedIDraggables = null;
    }

    protected override void SetBuildingActions()
    {
        BuildingActions = [];
    }

    protected override void OnBodyEntered(Node2D body)
    {
        if (body is Water water && BuildingScene._storedIDraggables.Count < _capacity)
        {
            GameScene.RemoveChild(water);
            BuildingScene._storedIDraggables.Add(water);

        }
    }
    protected override void OnClick()
    {
        if(BuildingScene._storedIDraggables.Count < 1 || GameScene.FocusedDraggable != null) return;
        
        var water = (Water) BuildingScene.RemoveDraggable();
        GameScene.AddChild(water);
        GameScene.FocusedDraggable = water;
        GameScene.Dragging = true;
        water.OnDragged();
        
    }
    
}