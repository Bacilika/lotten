using System;
using Godot;

namespace Lotten.Scripts.Products.Buildings;

public partial class Launcher:Building
{
    public Vector2 TargetLaunchPosition = new Vector2(0,0);
    public IDraggable LaunchedObject;
    private GameScene gameScene;
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
        launchTimer.Start();
        var button = new Button
        {
            Text = "Set Targets",
            
        };
        button.AddThemeFontSizeOverride("normal", 9);
        button.Pressed += SetLaunchTargets;
        BuildingScene.BuildingActions.Add(button);
    }

    public void SetLaunchTargets()
    {
        Console.WriteLine("Setting launch targets");
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