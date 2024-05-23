using Godot;
using System;

public partial class WorldMapLocation : Node
{
   [Export]
   private string textLabelName;
   [Export]
   private string locationName;
   [Export]
   private string entrancePointName;

   private WorldMapController worldMapController;

   public override void _Ready()
   {
      worldMapController = GetNode<WorldMapController>("/root/BaseNode/WorldMap/Player");
   }

   public void OnBodyEntered(Node2D body)
   {
      if (body.Name == "Player")
      {
         worldMapController.ReceiveIntersectionData(textLabelName, locationName, entrancePointName);
      }
   }

   public void OnBodyExited(Node2D body)
   {
      if (body.Name == "Player")
      {
         worldMapController.HideIntersectionLabel();
      }
   }
}
