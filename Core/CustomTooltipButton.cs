using Godot;
using System;

public partial class CustomTooltipButton : Button
{
   private PackedScene customTooltipScene;

   public override void _Ready()
   {
      customTooltipScene = GD.Load<PackedScene>("res://Core/tooltip_scene.tscn");
   }

   public override Control _MakeCustomTooltip(string forText)
   {
      RichTextLabel tooltip = customTooltipScene.Instantiate<RichTextLabel>();
      tooltip.Text = forText;
      return tooltip;
   }
}
