using Godot;
using System;

enum TheralinProgressMarkers
{
   None,
   RemoveArlitha
}

public partial class TheralinProgress : LevelProgession
{
   public override void LoadLevel()
   {
      for (int i = 0; i < managers.LevelManager.LocationDatas[managers.LevelManager.ActiveLocationDataID].levelProgress; i++)
      {
         Call(((TheralinProgressMarkers)i).ToString());
      }
   }

   public void CutsceneEnding(Node node)
   {
      RemoveArlitha();
      // add thalria
      progress++;
   }

   void RemoveArlitha()
   {
      GetNode<CharacterBody3D>("/root/BaseNode/Level/People/arlitha").Visible = false;
   }
}
