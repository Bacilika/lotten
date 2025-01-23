using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Lotten.Scripts.Products.Buildings;

public partial class RainCollector: Building
{
    public Area2D CollectionArea;
    public CollisionShape2D CollectionShape;
    public List<Water> Waterlist = [];
    protected override void SetCanReceive()
    {
        CanReceive = [];
        _storedIDraggables = null;
    }

    protected override void SetBuildingActions()
    {
        BuildingActions = [];
    }



    protected override void ReadyInstance()
    {
        CollectionArea = new Area2D();
        BuildingScene.AddChild(CollectionArea);
        
        BuildingScene.TopLevel = true;
        CollectionShape = new CollisionShape2D();
        CollectionArea.AddChild(CollectionShape);
        CollectionShape.Shape = new CircleShape2D();
        CollectionShape.Shape.Set("radius", 64);
        CollectionArea.BodyEntered += OnRainEntered;
        CollectionArea.BodyExited += OnRainExited;
    }

    private void OnRainExited(Node2D body)
    {
        if (body is not Water water || !Waterlist.Contains(water)) return;
        water.Sucked = false;
        Waterlist.Remove(water);
    }

    protected override void Tick(double delta)
    {
        List<Water> toRemove = [];
        var deltaFloat = (float)delta;
        foreach (var water in Waterlist)
        {
            var vel = (BuildingScene.GlobalPosition - water.GlobalPosition) / 10;
           water.ApplyForce(vel*1500*deltaFloat);
            if(water.GlobalPosition.DistanceTo(BuildingScene.GlobalPosition) < 12)
            {
                toRemove.Add(water);
                
            }
        }
        foreach (var water in toRemove)
        {
            Waterlist.Remove(water);
            if(GameScene.FocusedDraggable == water)
            {
                GameScene.FocusedDraggable = null;
            }
            water.Sucked = false;
            BuildingScene._storedIDraggables.Add(water);
            GameScene.RemoveChild(water);
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