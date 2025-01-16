
namespace Lotten.Scripts;

public static class Constants
{
	public const string ViewPortHeightSettingPath = "display/window/size/viewport_height";
	public const string ViewPortWidthSettingPath = "display/window/size/viewport_width";
	public const int TileSize = 32;
	public const int LandTiles = 4;
	public const int LandSize = TileSize * LandTiles;

}
public static class CustomData
{
	public const string Price = "Price";
	public const string Name = "Name";
	public const string SellPrice = "SellPrice";
	public const string GrowthTime = "GrowthTime";
	
} 

public static class Inputs
{
	public const string LeftClick = "left_click";
	public const string RightClick = "right_click";
	public const string ZoomIn = "zoom_in";
	public const string ZoomOut = "zoom_out";
	public const string CameraPan = "camera_pan";
}

public static class Signals
{
	public const string Pressed = "pressed";
	public const string ItemSelected = "item_selected";
}
