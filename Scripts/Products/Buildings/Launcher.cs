using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Lotten.Scripts.Products.Buildings;

public partial class Launcher:Building
{
    public Vector2 TargetLaunchPosition = new(0,0);
    public IDraggable LaunchedObject;
    private GameScene gameScene;
    private bool targetsSet;
    private Panel _gridMarker;
    
    private List<Vector2I> _targets = [];
    private List<Panel> _gridMarkers = [];
    private Vector2 targetGridPosition;
    private bool _isSettingTargets;

    public bool IsSettingTargets
    {
        get => _isSettingTargets;
        set
        {
            if (value == true)
            {
                //Todo: display targets
            }
            else
            {
                //Todo: hide targets
            }
            _isSettingTargets = value;
            _gameScene.TargetSetMode = value;
            _gridMarker.Visible = value;
        } 
    }

    [Signal]
    public delegate void OnTargetSelectionEventHandler(Building building);
    
    [Signal]
    public delegate void OnTargetUpdateEventHandler(string targets);
    public override void ReadyInstance()
    {
        gameScene = BuildingScene._gameScene;
        Timer launchTimer = new Timer();
        launchTimer.Timeout += () =>
        {
            Console.WriteLine("Launching water");
            var water = ResourceLoader.Load<PackedScene>("res://Scenes/Water.tscn").Instantiate<Water>();
            water.GlobalPosition = BuildingScene.GlobalPosition;
            gameScene.AddChild(water);
            LaunchedObject = water;
            water.InAir = true;
            water.SetCollisionLayer(0);
            TargetLaunchPosition = new Vector2(0,TargetLaunchPosition.Y - 24);
            
            
        };
        launchTimer.WaitTime = 5;
        gameScene.AddChild(launchTimer);
        if (targetsSet)
        {
            launchTimer.Start();
        }
        var button = new Button
        {
            Text = "Set Targets",
            
        };
        button.AddThemeFontSizeOverride("normal", 9);
        button.Pressed += SetLaunchTargets;
        BuildingScene.BuildingActions.Add(button);
        _gridMarker = new Panel();
        _gridMarker.Theme = ResourceLoader.Load<Theme>("res://Themes/RedBorderTheme.tres");
        _gridMarker.Size = _gameScene.TileMapValues.TileSize;
        _gridMarker.SetMouseFilter(Control.MouseFilterEnum.Ignore);
        _gameScene.AddChild(_gridMarker);
        IsSettingTargets = false;
        
    }
    
    private string TargetsToString()
    {
        string targets = $"Select Targets {_targets.Count}/5\n";
        foreach (var target in _targets)
        {
           targets += $"({target.X}, {target.Y})"+ "\n"; 
        }
        return targets;
    }

    private void SetLaunchTargets()
    {
        EmitSignal(SignalName.OnTargetSelection, this);
        IsSettingTargets = true;
        Console.WriteLine("Setting launch targets");
        _gridMarker.GlobalPosition = _gameScene.GrassLayer.LocalToMap(_gameScene.GetGlobalMousePosition());
    }

    public override void Tick(double delta)
    {
        if (IsSettingTargets)
        {
            var deltaFloat = (float)delta;
            var gridPosition = _gameScene.GrassLayer.LocalToMap(_gameScene.GetGlobalMousePosition());
            targetGridPosition = _gameScene.GrassLayer.MapToLocal(gridPosition) - _gameScene.TileMapValues.TileSize / 2;
            _gridMarker.GlobalPosition = _gridMarker.GlobalPosition.Lerp(targetGridPosition, 15 * deltaFloat);
        }
    }

    public override void PhysicsTick(double delta)
    {
        
        if(LaunchedObject is null) return;
        RigidBody2D rigidLaunchable = (RigidBody2D)LaunchedObject;
        var vel = (TargetLaunchPosition - rigidLaunchable.GlobalPosition) / 10;
        rigidLaunchable.ApplyForce(vel*250);
        if(rigidLaunchable.GlobalPosition.DistanceTo(TargetLaunchPosition) < 10)
        {
            LaunchedObject.OnDropped();
            LaunchedObject = null;
        }
    }
    public void AddTarget(Vector2I target)
    {
        if (_targets.Contains(target)) return;
        if(_gridMarker.Duplicate() is not Panel newTarget) return;
        if(_targets.Count > 4) return;
        newTarget.GlobalPosition = targetGridPosition;
        _targets.Add(target);
        _gridMarkers.Add(newTarget);
        _gameScene.AddChild(newTarget);
    }
    public void RemoveTarget(Vector2I target)
    {
        foreach (var marker in _gridMarkers)
        {
            var markerGridPos = _gameScene.GrassLayer.LocalToMap(marker.GlobalPosition);
            if(markerGridPos == target)
            {
               
                _gridMarkers.Remove(marker);
                _gameScene.RemoveChild(marker);
                marker.QueueFree();
                _targets.Remove(target);
                return;
            }
        }
    }

    public override void CustomInput(InputEvent @event)
    {
        if(!IsSettingTargets)return;
        if (@event.IsActionPressed(Inputs.LeftClick))
        {
            Console.WriteLine("Custom input left click");
            var gridPosition = _gameScene.GrassLayer.LocalToMap(targetGridPosition);
            AddTarget(gridPosition);
            
        }
        else if (@event.IsActionPressed(Inputs.RightClick))
        {
            var gridPosition = _gameScene.GrassLayer.LocalToMap(targetGridPosition);
            RemoveTarget(gridPosition);
        }
        EmitSignal(SignalName.OnTargetUpdate, TargetsToString());
        
    }

    public void OnCancelTargetSelection()
    {
        _targets.Clear();
    }

    public void OnConfirmTargetSelection()
    {
        Console.WriteLine("Confirming targets");
    }
}