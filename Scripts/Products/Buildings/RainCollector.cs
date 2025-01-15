using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Lotten.Scripts.Products.Buildings;

public partial class RainCollector: Building
{
    public Area2D CollectionArea;
    public CollisionShape2D CollectionShape;
    public List<Water> Waterlist = new();
    public override void ReadyInstance()
    {
        CollectionArea = new Area2D();
        BuildingScene.AddChild(CollectionArea);
        
        BuildingScene.TopLevel = true;
        CollectionShape = new CollisionShape2D();
        CollectionArea.AddChild(CollectionShape);
        CollectionShape.Shape = new CircleShape2D();
        CollectionShape.Shape.Set("radius", 32);
        CollectionArea.BodyEntered += OnRainEntered;
        CollectionArea.BodyExited += OnRainExited;
    }

    private void OnRainExited(Node2D body)
    {
        if (body is Water water)
        {
            if (Waterlist.Contains(water))
            {
                water.Sucked = false;
                Waterlist.Remove(water);
            }
        }
}

    public override void Tick(double delta)
    {
        List<Water> toRemove = new();
        var deltaFloat = (float)delta;
        foreach (var water in Waterlist)
        {
           
            water.LerpedPosition = BuildingScene.GlobalPosition;
            water.GlobalPosition = water.GlobalPosition.Lerp(water.LerpedPosition, 1 * deltaFloat);
            if(water.GlobalPosition.DistanceTo(BuildingScene.GlobalPosition) < 6)
            {
                toRemove.Add(water);
            }
        }
        foreach (var water in toRemove)
        {
            Waterlist.Remove(water);
            if(_gameScene.FocusedDraggable == water)
            {
                _gameScene.FocusedDraggable = null;
            }
            _gameScene.RemoveChild(water);
            water.QueueFree();
        }
        toRemove.Clear();
       BuildingScene.QueueRedraw();
        
       
        
    }

    private void OnRainEntered(Node2D body)
    {
        if (body is Water water)
        {
            if( Waterlist.Contains(water)) return;
            if(water.Sucked) return;
            Waterlist.Add(water);
            water.Sucked = true;
            
        }
    }
}