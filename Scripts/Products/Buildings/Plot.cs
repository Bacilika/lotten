using Godot;

namespace Lotten.Scripts.Products.Buildings;

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
            WaterSoil();
            water.QueueFree();
            _plant?.StartGrowTimer();
            
        }
    }
    
    private void WaterSoil()
    {
        if (_gameScene.PlotTiles.Contains(GridPosition))
        {
            _gameScene.WaterTiles.Add(GridPosition);
            _gameScene.WaterLayer.SetCellsTerrainConnect(_gameScene.WaterTiles, 0, 0);
        }
    }

    public override void OnBodyExited(Node2D body)
    {
    }

    public void Dry()
    {
        foreach (var tile in _gameScene.WaterTiles)
        {
            _gameScene.WaterLayer.EraseCell(tile);
        }
        _gameScene.WaterTiles.Remove(GridPosition);
        _gameScene.WaterLayer.SetCellsTerrainConnect(_gameScene.WaterTiles, 0, 0);
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