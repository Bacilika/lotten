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
		SetMouseFilter(MouseFilterEnum.Stop);
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
		if (product is Plant plant)
		{
			plant.PlantSprite.Texture = plant.Sprites[0];
		}
		
		//Icon.TextureNormal = product.HouseSprite.SpriteFrames.GetFrameTexture("Level0", 0);
		/*var containerWidth = Shop.Size.X/4;
		var minimum = new Vector2(containerWidth, containerWidth);
		SetCustomMinimumSize(minimum);
		Icon.SetCustomMinimumSize(minimum);
		CallDeferred("set_size", minimum);
		Icon.CallDeferred("set_size", minimum);*/
		//Icon.TooltipText = product.ProductName + "\n" + product.Cost + " coins";
		GetNode<RichTextLabel>("RichTextLabel").Text = $"[img]res://Sprites/coin.png[/img]x{product.Cost}";

		Icon.Pressed += OnShopIconPressed;
		//if (!product.IsUnlocked) Icon.Disabled = false;
	}
	public void OnShopIconPressed()
	{
		Shop.EmitSignal(Shop.SignalName.OnShopButtonPressed, Product); //Emitted to UI.cs
	}
	
}
