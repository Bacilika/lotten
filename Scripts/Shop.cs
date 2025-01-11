#nullable enable
using System;
using System.Collections.Generic;
using Godot;
using Lotten.Scripts.Products;

namespace Lotten.Scripts;


public partial class Shop : Control
{
	[Signal]
	public delegate void OnShopButtonPressedEventHandler(Product type);
	private bool _locked = true;

	public override void _Ready()
	{
		//placeAudio = GetNode<AudioStreamPlayer2D>("PlaceBuildingAudio");
		//deleteAudio = GetNode<AudioStreamPlayer2D>("DeleteBuildingAudio");


	}

}
