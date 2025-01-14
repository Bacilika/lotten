using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Lotten.Scripts.Products;

namespace Lotten.Scripts;

public abstract partial class AbstractShopIconContainer : ScrollContainer
{
	private Color _canBuy = new(1, 1, 1); //transparent
	private Color _cantAfford = new(1, 0, 0); //red
	private Color _disabled = new("#696969"); //gray

	public BoxContainer ChildContainer;
	public Shop GameShop;

	public List<Product> Products = [];
	public PackedScene ShopIconScene;
	public List<ShopIcon> Stock = [];


	public override void _Ready()
	{
		ShopIconScene = ResourceLoader.Load<PackedScene>("res://Scenes/shop_icon.tscn");
		ChildContainer = GetNode<BoxContainer>("BoxContainer");
		GameShop = GetParent().GetParent<Shop>();
		LoadTextures();
		foreach (var product in Products)
		{
			
			var shopIconControl = ShopIconScene.Instantiate<ShopIcon>();
			ChildContainer.AddChild(shopIconControl);
			shopIconControl.SetUp(product, GameShop);
			Stock.Add(shopIconControl);
		}
		/*Stock.Sort((x, y) => x.Product.PlayerLevel.CompareTo(y.Product.PlayerLevel));
		foreach (var child in childContainer.GetChildren()) childContainer.RemoveChild(child);*/

		//foreach (var item in Stock) childContainer.AddChild(item);
	}
	
	public abstract void LoadTextures();
	//Loads sprite sheet from tab tree in godot editor
   
	public override void _ExitTree()
	{
		foreach (var product in Products)
		{
			//product.OnExitTree();
			product.QueueFree();
		}
		foreach (var icon in Stock) icon.QueueFree();
	}
	
	public override void _Process(double delta)
	{
		//UpdateStock(GameLogistics.Resources);
	}

	public void UpdateStock(Dictionary<string, int> resources)
	{
	}
}
