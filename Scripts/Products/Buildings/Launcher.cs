using System;
using Godot;

namespace Lotten.Scripts.Products.Buildings;

public partial class Launcher:Building
{
    public Vector2 TargetLaunchPosition = new Vector2(0,0);
    public IDraggable LaunchedObject;
    private GameScene gameScene;
    private bool targetsSet;
    private Panel _gridMarker;

    private bool _isSettingTargets;

    [Signal]
    public delegate void OnTargetSelectionEventHandler(Building building);
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
        
    }

    public void SetLaunchTargets()
    {
        EmitSignal(SignalName.OnTargetSelection, this);
        _isSettingTargets = true;
        Console.WriteLine("Setting launch targets");
        _gridMarker.GlobalPosition = _gameScene.GrassLayer.LocalToMap(_gameScene.GetGlobalMousePosition());
        _gameScene.AddChild(_gridMarker); //TODO: replace addchild with visibility and add child in ready
    }

    public override void Tick(double delta)
    {
        if (_isSettingTargets)
        {
            var deltaFloat = (float)delta;
            var gridPosition = _gameScene.GrassLayer.LocalToMap(_gameScene.GetGlobalMousePosition());
            var targetGridPosition = _gameScene.GrassLayer.MapToLocal(gridPosition) - _gameScene.TileMapValues.TileSize / 2;
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
}