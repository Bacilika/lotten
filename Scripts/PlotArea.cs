using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Lotten.Scripts;
public partial class PlotArea : Area2D
{
	public Vector2 NeighBorUp;
	public Vector2 NeighBorDown;
	public Vector2 NeighBorLeft;
	public Vector2 NeighBorRight;
	
	public TextureButton ButtonUp;
	public TextureButton ButtonDown;
	public TextureButton ButtonLeft;
	public TextureButton ButtonRight;
	
	private GameScene _gameScene;
	

	public override void _Ready()
	{
		_gameScene = GetParent<GameScene>();
		ButtonUp = GetNode<TextureButton>("ExpandUp");
		ButtonDown = GetNode<TextureButton>("ExpandDown");
		ButtonLeft = GetNode<TextureButton>("ExpandLeft");
		ButtonRight = GetNode<TextureButton>("ExpandRight");
		
		ButtonUp.Pressed += () => OnButtonPressed("up");
		ButtonDown.Pressed += () => OnButtonPressed("down");
		ButtonLeft.Pressed += () => OnButtonPressed("left");
		ButtonRight.Pressed += () => OnButtonPressed("right");
		
		
		NeighBorDown = new Vector2(GlobalPosition.X, GlobalPosition.Y + Constants.LandSize);
		NeighBorUp = new Vector2(GlobalPosition.X, GlobalPosition.Y - Constants.LandSize);
		NeighBorRight = new Vector2(GlobalPosition.X + Constants.LandSize, GlobalPosition.Y);
		NeighBorLeft = new Vector2(GlobalPosition.X - Constants.LandSize, GlobalPosition.Y);
	}
	
	
	public void OnButtonPressed(string direction)
	{
		switch (direction)
		{
			case "up":
				_gameScene.SetUpExpansionZone(NeighBorUp);
				break;
			case "down":
				_gameScene.SetUpExpansionZone(NeighBorDown);
				break;
			case "left":
				_gameScene.SetUpExpansionZone(NeighBorLeft);
				break;
			case "right":
				_gameScene.SetUpExpansionZone(NeighBorRight);
				break;
		}
	}

	public void DisableButtons(PlotArea newPlotArea)
	{
		if (newPlotArea.Position == NeighBorUp)
		{
			ButtonUp.Visible = false;
		}
		else if(newPlotArea.Position == NeighBorDown)
		{
			ButtonDown.Visible = false;
		}
		else if(newPlotArea.Position == NeighBorLeft)
		{
			ButtonLeft.Visible = false;
		}
		else if(newPlotArea.Position == NeighBorRight)
		{
			ButtonRight.Visible = false;
		}
	}

}
