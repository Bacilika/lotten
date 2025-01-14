using System;
using Godot;

namespace Lotten.Scripts.Products;

public partial class Plot : Building
{
    private Plant _plant;
    public bool IsWatered;
    
    private bool _hasWater;
    
    public override void OnBodyEntered(Node2D body)
    {
        if (body is Water water && !IsWatered)
        {
            IsWatered = true;
            _gameScene.RemoveChild(water);
            BuildingScene.GetNode<Sprite2D>("water").Visible = true;
            water.QueueFree();
            _plant?.StartGrowTimer();
            
        }
    }

    public override void OnBodyExited(Node2D body)
    {
    }

    public void Dry()
    {
        BuildingScene.GetNode<Sprite2D>("water").Visible = false;
        IsWatered = false;
    }



    public Plant Plant { 
        get=>_plant;
        set
        {
            if(value == null)
            {
                _plant = null;
                return;
            }
            _plant = value;
            _plant.Plot = this;
            _plant.OnPlanted();
            
        }
    }
   
    
    
}