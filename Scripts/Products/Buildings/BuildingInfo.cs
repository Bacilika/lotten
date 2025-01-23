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
	public List<Label> ExtraBuildingLabels = [];
	private VBoxContainer _vBoxContainer;
	
	public override void _Ready()
	{
		_vBoxContainer = GetNode<VBoxContainer>("VBoxContainer");
		BuildingName = GetNode<Label>("VBoxContainer/Name");
		BuildingActions = GetNode<HBoxContainer>("VBoxContainer/BuildingActions");
		MoveButton = BuildingActions.GetNode<Button>("MoveButton");
		SellButton = BuildingActions.GetNode<Button>("SellButton");
		MoveButton.Pressed += () => { Console.WriteLine("Move Button Pressed"); };
		SellButton.Pressed += () => { Console.WriteLine("Sell Button Pressed"); };
		CloseButton = GetNode<Button>("Close");
	}

	private void AddExtraBuildingActions()
	{
		foreach (var extraBuildingAction in ExtraBuildingActions)
		{
			BuildingActions.RemoveChild(extraBuildingAction);
		}
		ExtraBuildingActions.Clear();
		
		foreach (var button in Building.BuildingActions)
		{
			BuildingActions.AddChild(button);
			ExtraBuildingActions.Add(button);
		}
	}
	private void AddExtraBuildingLabels()
	{
		foreach (var extraBuildingLabel in ExtraBuildingLabels)
		{
			_vBoxContainer.RemoveChild(extraBuildingLabel);
		}
		ExtraBuildingLabels.Clear();
		
		foreach (var label in Building.BuildingLabels)
		{
			_vBoxContainer.AddChild(label);
			ExtraBuildingLabels.Add(label);
		}
	}
	

	public void ShowBuildingInfo(Building building)
	{
		Building = building;
		BuildingName.Text = building.ProductName;
		AddExtraBuildingLabels();
		AddExtraBuildingActions();
		
	}
}
