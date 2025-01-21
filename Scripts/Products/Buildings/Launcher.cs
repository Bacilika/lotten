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
    private Panel _targetMarker;
    
    private List<Vector2I> _targets = [];
    private List<Panel> _targetMarkers = [];
    private Vector2 _targetGridPosition;
    private bool _isSettingTargets;
    private Timer _launchTimer;
    
    
    private Vector2I? _previousTarget;

    public bool IsSettingTargets
    {
        get => _isSettingTargets;
        set
        {
            foreach (var marker in _targetMarkers)
            {
                marker.Visible = value;
            }
            _isSettingTargets = value;
            _gameScene.TargetSetMode = value;
            _targetMarker.Visible = value;
        } 
    }

    [Signal]
    public delegate void OnTargetSelectionEventHandler(Building building);
    
    [Signal]
    public delegate void OnTargetUpdateEventHandler(string targets);
    public override void ReadyInstance()
    {
        gameScene = BuildingScene._gameScene;
        _launchTimer = new Timer();
        _launchTimer.Timeout += OnLaunchTimerTimeout;
        _launchTimer.WaitTime = 5;
        gameScene.AddChild(_launchTimer);
        
        var button = new Button();
        button.Text = "Set Targets";
        button.Pressed += SetLaunchTargets;
        BuildingScene.BuildingActions.Add(button);
        
        SetUpTargetMarker();
        IsSettingTargets = false;
        
    }

    private void OnLaunchTimerTimeout()
    {
        Console.WriteLine("Launching");
        if (_targets.Count == 0)
        {
            _launchTimer.Stop();
            return;
        }
        var stop = false;
        foreach (var target in _targets)
        {
            if (stop || _previousTarget is null)
            {
                TargetLaunchPosition = _gameScene.GrassLayer.MapToLocal(target);
                _previousTarget = target;
                break;
            }
            if(target == _previousTarget) 
                stop = true;
                
        }
        if (_previousTarget == _targets.Last())
        {
            TargetLaunchPosition =  _gameScene.GrassLayer.MapToLocal(_targets.First());
            _previousTarget = _targets.First();
        }

        var water = ResourceLoader.Load<PackedScene>("res://Scenes/Water.tscn").Instantiate<Water>();
        water.GlobalPosition = BuildingScene.GlobalPosition;
        gameScene.AddChild(water);
        LaunchedObject = water;
        water.InAir = true;
        water.SetCollisionLayer(0);
    }

    private void SetUpTargetMarker()
    {
        _targetMarker = new Panel();
        _targetMarker.Theme = ResourceLoader.Load<Theme>("res://Themes/RedBorderTheme.tres");
        _targetMarker.Size = _gameScene.TileMapValues.TileSize;
        _targetMarker.SetMouseFilter(Control.MouseFilterEnum.Ignore);
        _gameScene.AddChild(_targetMarker);
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
        _targetMarker.GlobalPosition = _gameScene.GrassLayer.LocalToMap(_gameScene.GetGlobalMousePosition());
    }

    public override void Tick(double delta)
    {
        if (IsSettingTargets)
        {
            var deltaFloat = (float)delta;
            var gridPosition = _gameScene.GrassLayer.LocalToMap(_gameScene.GetGlobalMousePosition());
            _targetGridPosition = _gameScene.GrassLayer.MapToLocal(gridPosition) - _gameScene.TileMapValues.TileSize / 2;
            _targetMarker.GlobalPosition = _targetMarker.GlobalPosition.Lerp(_targetGridPosition, 15 * deltaFloat);
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
        if(_targetMarker.Duplicate() is not Panel newTarget) return;
        if(_targets.Count > 4) return;
        newTarget.GlobalPosition = _targetGridPosition;
        _targets.Add(target);
        _targetMarkers.Add(newTarget);
        _gameScene.AddChild(newTarget);
    }
    public void RemoveTarget(Vector2I target)
    {
        foreach (var marker in _targetMarkers)
        {
            var markerGridPos = _gameScene.GrassLayer.LocalToMap(marker.GlobalPosition);
            if(markerGridPos == target)
            {
               
                _targetMarkers.Remove(marker);
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
            var gridPosition = _gameScene.GrassLayer.LocalToMap(_targetGridPosition);
            AddTarget(gridPosition);
            
        }
        else if (@event.IsActionPressed(Inputs.RightClick))
        {
            var gridPosition = _gameScene.GrassLayer.LocalToMap(_targetGridPosition);
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
        if(_targets.Count > 0)
        {
            targetsSet = true;
            _launchTimer.Start();
            Console.WriteLine("starting timer");
        }
    }
}