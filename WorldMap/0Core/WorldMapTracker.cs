using Godot;
using System;

public partial class WorldMapTracker : Node
{
   private ManagerReferenceHolder managers;
   private RichTextLabel locationInfo;

   private string targetLocationName;
   private string externalLocationName;

   private string entrancePointName;

   public override void _Ready()
   {
      managers = GetNode<ManagerReferenceHolder>("/root/BaseNode/ManagerReferenceHolder");
      locationInfo = GetNode<RichTextLabel>("../../Control/LocationInfo");
   }

   public void ReceiveIntersectionData(string labelName, string locationName, string entrancePoint)
   {
      locationInfo.Text = "[center]" + labelName + "[/center]";
      targetLocationName = locationName;
      externalLocationName = labelName;
      locationInfo.Visible = true;
      entrancePointName = entrancePoint;
   }

   public void HideIntersectionLabel()
   {
      locationInfo.Visible = false;
   }

   public override void _Input(InputEvent @event)
   {
      if (@event.IsActionPressed("interact") && locationInfo.Visible)
      {
         ExitWorldMap(targetLocationName, externalLocationName, entrancePointName);
      }
   }

   public async void ExitWorldMap(string internalLocation, string externalLocation, string spawnPoint)
   {
      managers.MenuManager.FadeToBlack();

      while (!managers.MenuManager.BlackScreenIsVisible)
      {
         await ToSignal(GetTree().CreateTimer(0.01f), "timeout");
      }
 
      managers.LevelManager.TransitionLevels(internalLocation, externalLocation, spawnPoint);
   }
}
