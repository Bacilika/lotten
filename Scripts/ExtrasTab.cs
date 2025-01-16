using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Lotten.Scripts;
using Lotten.Scripts.Products;
using Lotten.Scripts.Products.Buildings;

namespace Lotten.Scripts;

public partial class ExtrasTab : AbstractShopIconContainer
{
	public override void LoadTextures()
	{
		var atlasSource = (TileSetAtlasSource)GetNode<TileMapLayer>("TileMapLayer").TileSet.GetSource(0);
		var size = atlasSource.GetAtlasGridSize();
		
		for (var y = 0; y < size.Y; y++)
		{
			for (var x = 0; x < size.X; x++)
			{
				var building = ResourceLoader.Load<PackedScene>("res://Scenes/building.tscn").Instantiate<Building>();
				var atlasCoords = new Vector2I(x, y);
				var tileData = atlasSource.GetTileData(atlasCoords, 0);
				
				if (tileData is null || building is null)
					throw new Exception("Tile data is null");

				var type = (string)tileData.GetCustomData("Type");
				switch (type)
				{
					case "Plot":
						building.SubtypeGenerator = () => new Plot
						{
							GridPosition = building.GridPosition,
						};
						break;
					case "RainCollector":
						building.SubtypeGenerator = () => new RainCollector
						{
							GridPosition = building.GridPosition,
						};
						building.AreaOfInfluence = new DrawableCircle(64,Vector2.Zero, new Color(120/255f, 123/255f, 125/255f, 0.3f));
						break;
					case "WaterTank":
						building.SubtypeGenerator = () => new WaterTank
						{
							GridPosition = building.GridPosition,
						};
						break;
					case "Temp":
						break;

					default: throw new NotImplementedException();
					
				}
				building.ProductName = (string)tileData.GetCustomData("Type");
				building.Cost = (int)tileData.GetCustomData("Price");
				building.SellPrice = (int)tileData.GetCustomData(CustomData.SellPrice);

				AddChild(building);
				var img = atlasSource.Texture.GetImage().GetRegion(atlasSource.GetTileTextureRegion(atlasCoords));
				building!.PlantSprite.Texture = ImageTexture.CreateFromImage(img);
				Products.Add(building); 
				RemoveChild(building);

				
				
			}
			
		}
		
	}

}
