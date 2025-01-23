using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Lotten.Scripts.Products.Buildings;

public partial class Launcher:Building
{
    private Vector2 _targetLaunchPosition = new(0,0);
    private IDraggable _launchedObject;
    private GameScene gameScene;
    private bool _targetsSet;
    private Panel _targetMarker;
    
    private readonly List<Vector2I> _targets = [];
    private readonly List<Panel> _targetMarkers = [];
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
            GameScene.TargetSetMode = value;
            _targetMarker.Visible = value;
        } 
    }
    [Signal]
    public delegate void OnTargetSelectionEventHandler(Building building);
    
    [Signal]
    public delegate void OnTargetUpdateEventHandler(string targets);

    protected override void SetCanReceive()
    {
        CanReceive = [nameof(Water), nameof(PhysicsPlant)];
    }

    protected override void SetBuildingActions()
    {
        BuildingActions = [];
    }

    protected override void SetBuildingLabels()
    {
        var button = new Button();
        button.Text = "Set Targets";
        button.Pressed += SetLaunchTargets;
        BuildingScene.BuildingActions.Add(button);
    }
    private void SetLaunchTargets()
    {
        EmitSignal(SignalName.OnTargetSelection, this);
        IsSettingTargets = true;
        Console.WriteLine("Setting launch targets");
        _targetMarker.GlobalPosition = GameScene.GrassLayer.LocalToMap(GameScene.GetGlobalMousePosition());
    }


    protected override void ReadyInstance()
    {
        gameScene = BuildingScene.GameScene;
        _launchTimer = new Timer();
        _launchTimer.Timeout += OnLaunchTimerTimeout;
        _launchTimer.WaitTime = 5;
        gameScene.AddChild(_launchTimer);
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
        SetNextTarget();
        
        SetLaunchObject();
    }

    private void SetNextTarget()
    {
        var stop = false;
        foreach (var target in _targets)
        {
            if (stop || _previousTarget is null)
            {
                _targetLaunchPosition = GameScene.GrassLayer.MapToLocal(target);
                _previousTarget = target;
                break;
            }
            if(target == _previousTarget) 
                stop = true;
                
        }
        if (_previousTarget == _targets.Last())
        {
            _targetLaunchPosition =  GameScene.GrassLayer.MapToLocal(_targets.First());
            _previousTarget = _targets.First();
        }
    }
    
    private void SetLaunchObject()
    {
        var draggable = BuildingScene.RemoveDraggable();
        if (draggable is null)
            return;
        if(draggable is Water water)
            gameScene.AddChild(water);
        else
            gameScene.AddChild((PhysicsPlant) draggable);
        _launchedObject = draggable;
        draggable.InAir = true;
        ((RigidBody2D)draggable).SetCollisionLayer(0);
        ((RigidBody2D)draggable).SetCollisionMask(0);
    }

    private void SetUpTargetMarker()
    {
        _targetMarker = new Panel();
        _targetMarker.Theme = ResourceLoader.Load<Theme>("res://Themes/RedBorderTheme.tres");
        _targetMarker.Size = GameScene.TileMapValues.TileSize;
        _targetMarker.SetMouseFilter(Control.MouseFilterEnum.Ignore);
        GameScene.AddChild(_targetMarker);
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


    protected override void Tick(double delta)
    {
        if (IsSettingTargets)
        {
            var deltaFloat = (float)delta;
            var gridPosition = GameScene.GrassLayer.LocalToMap(GameScene.GetGlobalMousePosition());
            _targetGridPosition = GameScene.GrassLayer.MapToLocal(gridPosition) - GameScene.TileMapValues.TileSize / 2;
            _targetMarker.GlobalPosition = _targetMarker.GlobalPosition.Lerp(_targetGridPosition, 15 * deltaFloat);
        }
    }

    protected override void PhysicsTick(double delta)
    {
        
        if(_launchedObject is null) return;
        RigidBody2D rigidLaunchable = (RigidBody2D)_launchedObject;
        var vel = (_targetLaunchPosition - rigidLaunchable.GlobalPosition) / 10;
        rigidLaunchable.ApplyForce(vel*250);
        if(rigidLaunchable.GlobalPosition.DistanceTo(_targetLaunchPosition) < 10)
        {
            _launchedObject.OnDropped();
            _launchedObject = null;
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
        GameScene.AddChild(newTarget);
    }
    public void RemoveTarget(Vector2I target)
    {
        foreach (var marker in _targetMarkers)
        {
            var markerGridPos = GameScene.GrassLayer.LocalToMap(marker.GlobalPosition);
            if(markerGridPos == target)
            {
               
                _targetMarkers.Remove(marker);
                GameScene.RemoveChild(marker);
                marker.QueueFree();
                _targets.Remove(target);
                return;
            }
        }
    }

    protected override void CustomInput(InputEvent @event)
    {
        if(!IsSettingTargets)return;
        if (@event.IsActionPressed(Inputs.LeftClick))
        {
            Console.WriteLine("Custom input left click");
            var gridPosition = GameScene.GrassLayer.LocalToMap(_targetGridPosition);
            AddTarget(gridPosition);
            
        }
        else if (@event.IsActionPressed(Inputs.RightClick))
        {
            var gridPosition = GameScene.GrassLayer.LocalToMap(_targetGridPosition);
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
            _targetsSet = true;
            _launchTimer.Start();
            Console.WriteLine("starting timer");
        }
    }

}