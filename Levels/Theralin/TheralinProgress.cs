using Godot;
using System;

enum TheralinProgressMarkers
{
   FirstTheralinCutscene
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

   public void FirstCutsceneInitiated()
   {
      FirstTheralinCutscene();

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

      GetNode<CharacterController>("/root/BaseNode/PartyMembers/Member1").GlobalPosition = new Vector3(1.168f, 0, 68.9f);
      
      progress++;
   }

   /// <summary>
   /// Arlitha/Thalria cutscene (adds Thalria, moves Arlitha, deletes the block preventing access to the Athlia cutscene)
   /// </summary>
   void FirstTheralinCutscene()
   {
      GetNode<CharacterBody3D>("/root/BaseNode/Level/People/arlitha").Visible = false;
      GetNode<CharacterBody3D>("/root/BaseNode/Level/People/thalria").Visible = false;

      Node3D block = GetNode<Node3D>("/root/BaseNode/Level/cow_block");
      GetNode<Node3D>("/root/BaseNode/Level").CallDeferred(Node3D.MethodName.RemoveChild, block);
      // If RemoveChild is being called through CallDeferred, QueueFree must also be called through CallDeferred, or else QueueFree will execute before
      // RemoveChild, making the child null and thus causing an error
      block.CallDeferred(Node3D.MethodName.QueueFree);
   }
}
