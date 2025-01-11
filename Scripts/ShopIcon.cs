using System;
using Godot;
using Lotten.Scripts.Products;

namespace Lotten.Scripts;

public partial class ShopIcon : Control
{
	public TextureButton Icon;
	public Product Product;
	public Shop Shop;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Icon = GetNode<TextureButton>("TextureButton");
	}
	public Product GetProduct()
	{
		return Product;
	}

	public override void _ExitTree()
	{
		//TODO: queuefree plant sprites
		Product.QueueFree();
	}

	public void SetUp(Product product, Shop shop)
	{
		Product = product;
		Shop = shop;
		Icon.TextureNormal = product.PlantSprite.Texture;
		//Icon.TextureNormal = product.HouseSprite.SpriteFrames.GetFrameTexture("Level0", 0);
		var containerHeight = Size.Y;
		var minimum = new Vector2(containerHeight, containerHeight);
		SetCustomMinimumSize(minimum);
		Icon.SetCustomMinimumSize(minimum);
		CustomMinimumSize = minimum;

		Icon.Pressed += OnShopIconPressed;
		//if (!product.IsUnlocked) Icon.Disabled = false;
	}
	public void OnShopIconPressed()
	{
		Console.WriteLine("ShopIcon pressed");
		Shop.EmitSignal(Shop.SignalName.OnShopButtonPressed, Product); //Emitted to UI.cs
	}
	
}
