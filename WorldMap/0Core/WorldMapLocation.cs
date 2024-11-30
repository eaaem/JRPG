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
   [Export]
   private bool forceOpens;

   private WorldMapTracker worldMapTracker;

   public override void _Ready()
   {
      worldMapTracker = GetNode<WorldMapTracker>("/root/BaseNode/WorldMap/Player/Tracker");
   }

   public void OnBodyEntered(Node3D body)
   {
      if (body.Name == "Player")
      {
         if (!forceOpens)
         {
            worldMapTracker.ReceiveIntersectionData(textLabelName, locationName, entrancePointName);
         }
         else
         {
            worldMapTracker.ExitWorldMap(locationName, textLabelName, entrancePointName);
         }
      }
   }

   public void OnBodyExited(Node3D body)
   {
      if (body.Name == "Player")
      {
         worldMapTracker.HideIntersectionLabel();
      }
   }
}
