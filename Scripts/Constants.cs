
namespace Lotten.Scripts;

public static class Constants
{
	public const string ViewPortHeightSettingPath = "display/window/size/viewport_height";
	public const string ViewPortWidthSettingPath = "display/window/size/viewport_width";
	public const string CanPlaceSeeds = "can_place_seeds";
	public const string CanPlaceDirt = "can_place_dirt";
	public const string NeedsWatering = "needs_watering";
	public const string IsFinalLevel = "is_final_level";
	public const string IsPond = "is_pond";
	public const string IsShopItem = "is_shop_item";
	public const int GrassLayer = 0;
	public const int DirtLayer = 1;
	public const int WaterLayer = 2;
	public const int SeedLayer = 3;
	public const int TileSize = 16;

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
