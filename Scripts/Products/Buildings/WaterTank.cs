using Godot;

namespace Lotten.Scripts.Products.Buildings;

public partial class WaterTank: Building
{
    public int StoredWater = 0;
    public int Capacity = 20;
   
    public override void OnBodyEntered(Node2D body)
    {
        if (body is Water water && StoredWater < Capacity)
        {
            _gameScene.RemoveChild(water);
            water.QueueFree();
            StoredWater++;

        }
    }
    public override void OnClick()
    {
        if(StoredWater < 1 || _gameScene.FocusedDraggable != null) return; 
        var water = _gameScene.SpawnWater(BuildingScene.GlobalPosition);
        _gameScene.FocusedDraggable = water;
        _gameScene.Dragging = true;
        water.OnDragged();
        StoredWater--;
    }
}