using Godot;
using System;
using Lotten.Scripts.Products.Buildings;

namespace Lotten.Scripts;
public partial class TargetSelectionView : Panel
{
	public Building Building;
	public RichTextLabel TargetLabel;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		TargetLabel = GetNode<RichTextLabel>("VBoxContainer/TargetLabel");
		GetNode<Button>("VBoxContainer/ButtonContainer/CancelButton").Pressed += () =>
		{
			if(Building is Launcher launcher)
			{
				launcher.IsSettingTargets = false;
				launcher.OnCancelTargetSelection();
			}
		};
		GetNode<Button>("VBoxContainer/ButtonContainer/ConfirmButton").Pressed += () =>
		{
			if(Building is Launcher launcher)
			{
				launcher.IsSettingTargets = false;
				launcher.OnConfirmTargetSelection();
			}
		};
	}
	
}
