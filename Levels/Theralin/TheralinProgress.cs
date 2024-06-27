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

   /// <summary>
   /// Arlitha/Thalria cutscene (adds Thalria, moves Arlitha)
   /// </summary>
   public void CutsceneEnding(Node node)
   {
      RemoveArlitha();

      string[] thalriaEquipment = new string[6];

      for (int i = 0; i < managers.PartyManager.ThalriaItems.Length; i++)
      {
         thalriaEquipment[i] = managers.PartyManager.ThalriaItems[i].ResourcePath;
      }

      for (int i = managers.PartyManager.ThalriaItems.Length; i < 6; i++)
      {
         thalriaEquipment[i] = "";
      }

      Member thalria = managers.PartyManager.LoadPartyMember((int)CharacterType.Thalria, 0, 0, thalriaEquipment, 0, true, 1, 1);
      thalria.currentHealth = thalria.GetMaxHealth();
      thalria.currentMana = thalria.GetMaxMana();

      thalria.model.GlobalPosition = new Vector3(3.179f, 0, 72.412f);
      
      progress++;
   }

   void RemoveArlitha()
   {
      GetNode<CharacterBody3D>("/root/BaseNode/Level/People/arlitha").Visible = false;
   }
}
