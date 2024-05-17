using Godot;
using System;

public partial class ShopItemButton : Button
{
	private ShopMenuManager shopMenuManager;
   private PackedScene customTooltipScene;

   public override void _Ready()
   {
      shopMenuManager = GetNode<ShopMenuManager>("/root/BaseNode/UI/Shop/Background");
      customTooltipScene = GD.Load<PackedScene>("res://BasicUI/tooltip_scene.tscn");
   }

   void OnButtonDown()
   {
      shopMenuManager.OnSelectItem(new InventoryItem(GetNode<ShopItemHolder>("../ItemHolder").item, GetNode<ShopItemHolder>("../ItemHolder").quantity));
   }

   public override Control _MakeCustomTooltip(string forText)
   {
      RichTextLabel tooltip = customTooltipScene.Instantiate<RichTextLabel>();
      tooltip.Text = forText;
      return tooltip;
   }
}
