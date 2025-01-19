using Godot;
using System;
using System.Collections.Generic;
namespace Lotten.Scripts.Products.Buildings;
public partial class BuildingInfo : Panel
{
	public Building Building;
	public Label BuildingName;
	public HBoxContainer BuildingActions;
	public Button MoveButton;
	public Button CloseButton;
	public Button SellButton;
	public List<Button> ExtraBuildingActions = [];
	
	public override void _Ready()
	{
		BuildingName = GetNode<Label>("VBoxContainer/Name");
		BuildingActions = GetNode<HBoxContainer>("VBoxContainer/BuildingActions");
		MoveButton = BuildingActions.GetNode<Button>("MoveButton");
		SellButton = BuildingActions.GetNode<Button>("SellButton");
		MoveButton.Pressed += () => { Console.WriteLine("Move Button Pressed"); };
		SellButton.Pressed += () => { Console.WriteLine("Sell Button Pressed"); };
		CloseButton = GetNode<Button>("Close");
	}
	

	public void ShowBuildingInfo(Building building)
	{
		Building = building;
		BuildingName.Text = building.ProductName;
		foreach (var extraBuildingAction in ExtraBuildingActions)
		{
			BuildingActions.RemoveChild(extraBuildingAction);
		}
		ExtraBuildingActions.Clear();
		
		foreach (var button in building.BuildingActions)
		{
			BuildingActions.AddChild(button);
			ExtraBuildingActions.Add(button);
		}
	}
}
