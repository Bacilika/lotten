using Godot;

namespace Lotten.Scripts.Products.Buildings;

public partial class Plot : Building
{
    private Plant _plant;
    public bool IsWatered;
    
    private bool _hasWater;
    
    protected override void OnBodyEntered(Node2D body)
    {
        if (body is Water water && !IsWatered)
        {
            IsWatered = true;
            GameScene.RemoveChild(water);
            WaterSoil();
            water.QueueFree();
            _plant?.StartGrowTimer();
            
        }
    }
    
    private void WaterSoil()
    {
        if (GameScene.PlotTiles.Contains(GridPosition))
        {
            GameScene.WaterTiles.Add(GridPosition);
            GameScene.WaterLayer.SetCellsTerrainConnect(GameScene.WaterTiles, 0, 0);
        }
    }

    protected override void SetCanReceive()
    {
        CanReceive = [];
    }

    protected override void SetBuildingActions()
    {
        BuildingActions = [];
    }

    protected override void SetBuildingLabels()
    {
       BuildingScene.BuildingLabels = [];
    }

    public void Dry()
    {
        foreach (var tile in GameScene.WaterTiles)
        {
            GameScene.WaterLayer.EraseCell(tile);
        }
        GameScene.WaterTiles.Remove(GridPosition);
        GameScene.WaterLayer.SetCellsTerrainConnect(GameScene.WaterTiles, 0, 0);
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