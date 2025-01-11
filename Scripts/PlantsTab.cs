using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Lotten.Scripts.Products;

namespace Lotten.Scripts;

public partial class PlantsTab : AbstractShopIconContainer
{
	public override void LoadTextures()
	{
		var atlasSource = (TileSetAtlasSource)GetNode<TileMapLayer>("TileMapLayer").TileSet.GetSource(0);
		var size = atlasSource.GetAtlasGridSize();
		for (var y = 0; y < size.Y; y++)
		{
			var plant = ResourceLoader.Load<PackedScene>("res://Scenes/plant.tscn").Instantiate<Plant>();
			AddChild(plant);
			for (var x = 0; x < size.X; x++)
			{

				var atlasCoords = new Vector2I(x, y);
				var tileData = atlasSource.GetTileData(atlasCoords, 0);
				if (tileData is not null)
				{
					/*if ((bool)ileData.GetCustomData(Constants.NeedsWatering))
					{
						waterLevels.Add(x);
					}*/
				}

				var img = atlasSource.Texture.GetImage().GetRegion(atlasSource.GetTileTextureRegion(atlasCoords));

				plant.Sprites.Add(ImageTexture.CreateFromImage(img));
			}
			plant.PlantSprite.Texture = plant.Sprites.Last();
			Products.Add(plant); 
			RemoveChild(plant);
		}
		
	}
}
