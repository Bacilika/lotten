using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class PlotArea : Area2D
{
	public PlotArea PlotUp;
	public PlotArea PlotDown;
	public PlotArea PlotLeft;
	public PlotArea PlotRight;
	public TextureButton ButtonUp;
	public TextureButton ButtonDown;
	public TextureButton ButtonLeft;
	public TextureButton ButtonRight;
	
	public Dictionary<TextureButton,PlotArea> ButtonForPlotAreas = new();

	
	[Signal]
	public delegate void OnPlotAreaExpandedEventHandler(PlotArea area2D);

	public override void _Ready()
	{
		ButtonUp = GetNode<TextureButton>("ExpandUp");
		ButtonDown = GetNode<TextureButton>("ExpandDown");
		ButtonLeft = GetNode<TextureButton>("ExpandLeft");
		ButtonRight = GetNode<TextureButton>("ExpandRight");
	}
	

	public void SetUp(PlotArea up, PlotArea down, PlotArea left, PlotArea right)
	{
		PlotUp = up;
		PlotDown = down;
		PlotLeft = left;
		PlotRight = right;
		ButtonForPlotAreas[ButtonUp] = up;
		ButtonForPlotAreas[ButtonDown] = down;
		ButtonForPlotAreas[ButtonLeft] = left;
		ButtonForPlotAreas[ButtonRight] = right;

		foreach (var kvp in ButtonForPlotAreas)
		{
			var button = kvp.Key;
			var connectedNeighbor = kvp.Value;
			if(connectedNeighbor is not null)
			{
				OnPlotAreaExpanded += connectedNeighbor.OnExpansion;
				
				button.Visible = true;
				button.Pressed += () =>
				{
					connectedNeighbor.Visible = true;
					button.Visible = false;
					connectedNeighbor.EmitSignal(SignalName.OnPlotAreaExpanded, connectedNeighbor);
				};
			}
		}
	}
	public void OnExpansion(PlotArea neighbor)
	{
		foreach (var kvp in ButtonForPlotAreas
			         .Where(kvp => kvp.Value == neighbor))
		{
			kvp.Key.Visible = false;
		}
	}

}
