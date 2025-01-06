using Godot;
using System;

enum TheralinProgressMarkers
{
   FirstTheralinCutscene,
   SecondTheralinCutscene,
   ThirdTheralinCutscene
}

public partial class TheralinProgress : LevelProgession
{
   PackedScene tutorialPopupScene;

   System.Collections.Generic.List<Member> oldParty;

   public override void LoadLevel()
   {
      progress = managers.LevelManager.LocationDatas[managers.LevelManager.ActiveLocationDataID].levelProgress;
      for (int i = 0; i < (progress < 4 ? progress : 4); i++)
      {
         Call(((TheralinProgressMarkers)i).ToString());
      }

      RandomDistortion();
   }

   async void RandomDistortion()
   {
      ShaderMaterial material = (ShaderMaterial)GetNode<ColorRect>("/root/BaseNode/UI/Overlay/Distortion").Material;

      while (progress < 4)
      {
         await ToSignal(GetTree().CreateTimer(GD.RandRange(20, 60)), "timeout");
         material.SetShaderParameter("scale", 0.0015);
         await ToSignal(GetTree().CreateTimer(GD.RandRange(1, 5)), "timeout");
         material.SetShaderParameter("scale", 0);
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
   /// Thelren/Athlia/Arthon cutscene (removes Thelren and Athlia from world, adds Athlia, deletes the block preventing access to the Olren cutscene)
   /// </summary>
   public void SecondCutsceneInitiated()
   {
      SecondTheralinCutscene();

      GetNode<AnimatedBeing>("/root/BaseNode/Level/arthon").Visible = false;

      string[] athliaEquipment = new string[6];

      for (int i = 0; i < managers.PartyManager.AthliaItems.Length; i++)
      {
         athliaEquipment[i] = managers.PartyManager.AthliaItems[i].ResourcePath;
      }

      for (int i = managers.PartyManager.AthliaItems.Length; i < 6; i++)
      {
         athliaEquipment[i] = "";
      }

      for (int i = 0; i < managers.PartyManager.Party.Count; i++)
      {
         if (managers.PartyManager.Party[i].characterType == CharacterType.Vakthol)
         {
            managers.PartyManager.Party[i].model.GlobalPosition = new Vector3(-11.899f, 0, 14.437f);
         }
         else
         {
            managers.PartyManager.Party[i].model.GlobalPosition = new Vector3(-10.411f, 0, 13.643f);
         }
      }

      Member athlia = managers.PartyManager.LoadPartyMember((int)CharacterType.Athlia, 0, 0, athliaEquipment, 0, true, 1, 1);
      athlia.currentHealth = athlia.GetMaxHealth();
      athlia.currentMana = athlia.GetMaxMana();

      athlia.model.GlobalPosition = new Vector3(-14.393f, 0, 12.987f);

      progress++;
   }

   public void ThirdCutsceneInitiated()
   {
      ThirdTheralinCutscene();

      string[] olrenEquipment = new string[6];

      for (int i = 0; i < managers.PartyManager.OlrenItems.Length; i++)
      {
         olrenEquipment[i] = managers.PartyManager.OlrenItems[i].ResourcePath;
      }

      for (int i = managers.PartyManager.OlrenItems.Length; i < 6; i++)
      {
         olrenEquipment[i] = "";
      }

      for (int i = 0; i < managers.PartyManager.Party.Count; i++)
      {
         if (managers.PartyManager.Party[i].characterType == CharacterType.Vakthol)
         {
            managers.PartyManager.Party[i].model.GlobalPosition = new Vector3(-11.899f, 0, 14.437f);
         }
         else
         {
            managers.PartyManager.Party[i].model.GlobalPosition = new Vector3(-10.411f, 0, 13.643f);
         }
      }

      Member olren = managers.PartyManager.LoadPartyMember((int)CharacterType.Olren, 0, 0, olrenEquipment, 0, true, 1, 1);
      olren.currentHealth = olren.GetMaxHealth();
      olren.currentMana = olren.GetMaxMana();

      olren.model.GlobalPosition = new Vector3(-14.393f, 0, 12.987f);
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

   void SecondTheralinCutscene()
   {
      Node3D items = GetNode<Node3D>("/root/BaseNode/Level/cutscene4_items");
      GetNode<Node3D>("/root/BaseNode/Level").CallDeferred(Node3D.MethodName.RemoveChild, items);
      items.CallDeferred(Node3D.MethodName.QueueFree);

      Node3D block = GetNode<Node3D>("/root/BaseNode/Level/argument_block");
      GetNode<Node3D>("/root/BaseNode/Level").CallDeferred(Node3D.MethodName.RemoveChild, block);
      block.CallDeferred(Node3D.MethodName.QueueFree);
   }

   void ThirdTheralinCutscene()
   {
      Node3D items = GetNode<Node3D>("/root/BaseNode/Level/cutscene5_items");
      GetNode<Node3D>("/root/BaseNode/Level").CallDeferred(Node3D.MethodName.RemoveChild, items);
      items.CallDeferred(Node3D.MethodName.QueueFree);

      StartDistortionTimer();
   }

   public void ShopTutorial()
   {
      tutorialPopupScene = GD.Load<PackedScene>("res://Menus/Other/tutorial_popup.tscn");
      managers.PartyManager.AddItem(new InventoryItem(GD.Load<ItemResource>("res://Items/Consumables/ManaPotion/small_mana_potion.tres"), 1));

      ShopItem shopItem = new ShopItem();
      shopItem.selection = new ItemResource[2];
      shopItem.selection[0] = GD.Load<ItemResource>("res://Items/Other/tertha_stathrin.tres");
      shopItem.selection[1] = GD.Load<ItemResource>("res://Items/Other/marlana_stathrin.tres");

      GetNode<Button>("/root/BaseNode/UI/Shop/Background/Buttons/Exit").Disabled = true;
      Node dummyButton = GetNode<Button>("/root/BaseNode/UI/Shop/Background/Buttons/Exit").Duplicate();
      dummyButton.Name = "Dummy";
      GetNode<Node2D>("/root/BaseNode/UI/Shop/Background/Buttons").AddChild(dummyButton);
      GetNode<Button>("/root/BaseNode/UI/Shop/Background/Buttons/Exit").Visible = false;

      managers.ShopMenuManager.OpenShop(shopItem);

      GetNode<Button>("/root/BaseNode/UI/Shop/Background/Notification/YesButton").ButtonDown += OnPurchase;
      GetNode<Button>("/root/BaseNode/UI/Shop/Background/Buttons/Exit").ButtonDown += OnShopExit;
   }

   void OnPurchase()
   {
      if (managers.ShopMenuManager.IsBuying)
      {
         Button dummy = GetNode<Button>("/root/BaseNode/UI/Shop/Background/Buttons/Dummy");
         GetNode<Node2D>("/root/BaseNode/UI/Shop/Background/Buttons").RemoveChild(dummy);
         dummy.QueueFree();
         GetNode<Button>("/root/BaseNode/UI/Shop/Background/Buttons/Exit").Visible = true;

         GetNode<Button>("/root/BaseNode/UI/Shop/Background/Buttons/Exit").Disabled = false;
         GetNode<Button>("/root/BaseNode/UI/Shop/Background/Notification/YesButton").ButtonDown -= OnPurchase;

         if (GetNode<RichTextLabel>("/root/BaseNode/UI/Shop/Background/Notification/Body").Text.Contains("Tertha"))
         {
            managers.CutsceneManager.InsertCutsceneItems(GetNode<CutsceneTrigger>("Cutscene4Tertha").cutsceneObject, managers.DialogueManager.CurrentIndex + 1);
         }
         else
         {
            managers.CutsceneManager.InsertCutsceneItems(GetNode<CutsceneTrigger>("Cutscene4Marlana").cutsceneObject, managers.DialogueManager.CurrentIndex + 1);
         }
      }
   }
   
   void OnShopExit()
   {
      managers.DialogueManager.NextCutsceneDialogue();
      GetNode<Button>("/root/BaseNode/UI/Shop/Background/Buttons/Exit").ButtonDown -= OnShopExit;
   }

   public void ShowArthon()
   {
      GetNode<AnimatedBeing>("/root/BaseNode/Level/arthon").Visible = true;
   }

   public void TutorialBattle()
   {
      WorldEnemy enemy = GetNode<WorldEnemy>("TutorialEnemyData");

      oldParty = new System.Collections.Generic.List<Member>(managers.PartyManager.Party);

      Member vakthol = null;

      for (int i = 0; i < managers.PartyManager.Party.Count; i++)
      {
         if (managers.PartyManager.Party[i].characterType == CharacterType.Vakthol)
         {
            vakthol = managers.PartyManager.Party[i];
         }
      }

      managers.PartyManager.Party.Clear();
      managers.PartyManager.Party.Add(vakthol);

      GetNode<CombatManager>("/root/BaseNode/CombatManager").SetupCombat(enemy.enemies, Vector3.Zero, Vector3.Zero, enemy);
      GetNode<Node3D>("/root/BaseNode/PartyMembers").Visible = true;
      GetNode<CombatManager>("/root/BaseNode/CombatManager").BattleEnd += EndOfTutorialBattle;
   }

   void EndOfTutorialBattle()
   {
      GetNode<CombatManager>("/root/BaseNode/CombatManager").BattleEnd -= EndOfTutorialBattle;

      GetNode<Node3D>("/root/BaseNode/PartyMembers").Visible = false;
      GetNode<Camera3D>("/root/BaseNode/CutsceneCamera").MakeCurrent();
      managers.Controller.DisableMovement = true;
      managers.Controller.DisableCamera = true;

      managers.PartyManager.Party = new System.Collections.Generic.List<Member>(oldParty);
      managers.DialogueManager.NextCutsceneDialogue();

      for (int i = 0; i < managers.PartyManager.Party.Count; i++)
      {
         if (managers.PartyManager.Party[i].characterType == CharacterType.Vakthol)
         {
            managers.PartyManager.Party[i].model.GlobalPosition = new Vector3(49.43f, 3.24f, 53f);
         }
         else if (managers.PartyManager.Party[i].characterType == CharacterType.Thalria)
         {
            managers.PartyManager.Party[i].model.GlobalPosition = new Vector3(47f, 3.24f, 53f);
         }
         else if (managers.PartyManager.Party[i].characterType == CharacterType.Athlia)
         {
            managers.PartyManager.Party[i].model.GlobalPosition = new Vector3(46.5f, 3.24f, 51.6f);
         }
         else
         {
            managers.PartyManager.Party[i].model.GlobalPosition = new Vector3(48f, 3.24f, 50f);
         }
      }
   }

   public async void StartDistortionTimer()
   {
      while (managers.Controller.isInCutscene)
      {
         await ToSignal(GetTree().CreateTimer(10f), "timeout");
      }

      ShaderMaterial material = (ShaderMaterial)GetNode<ColorRect>("/root/BaseNode/UI/Overlay/Distortion").Material;

      while (progress < 63)
      {
         await ToSignal(GetTree().CreateTimer(1f), "timeout");
         progress++;
      }

      if (progress <= 63)
      {
         material.SetShaderParameter("scale", 0.002);
         await ToSignal(GetTree().CreateTimer(2f), "timeout");
         material.SetShaderParameter("scale", 0);
      }

      while (progress < 123)
      {
         await ToSignal(GetTree().CreateTimer(1f), "timeout");
         progress++;
      }

      if (progress <= 123)
      {
         material.SetShaderParameter("scale", 0.005);
         managers.Controller.RegularSpeed = 3.5f;
         managers.Controller.SprintSpeed = 7f;
         await ToSignal(GetTree().CreateTimer(6f), "timeout");
         material.SetShaderParameter("scale", 0);
         managers.Controller.RegularSpeed = 5f;
         managers.Controller.SprintSpeed = 10f;
      }

      while (progress < 153)
      {
         await ToSignal(GetTree().CreateTimer(1f), "timeout");
         progress++;
      }

      if (progress <= 153)
      {
         material.SetShaderParameter("scale", 0.007);
         managers.Controller.RegularSpeed = 1.5f;
         managers.Controller.SprintSpeed = 3f;
         await ToSignal(GetTree().CreateTimer(10f), "timeout");
         material.SetShaderParameter("scale", 0);
         managers.Controller.RegularSpeed = 5f;
         managers.Controller.SprintSpeed = 10f;
      }
   
      while (progress < 183)
      {
         await ToSignal(GetTree().CreateTimer(1f), "timeout");
         progress++;
      }

      if (progress <= 183)
      {
         material.SetShaderParameter("scale", 0.01);
         managers.Controller.RegularSpeed = 1f;
         managers.Controller.SprintSpeed = 2f;
         await ToSignal(GetTree().CreateTimer(10f), "timeout");

         managers.Controller.DisableMovement = true;
         managers.Controller.DisableCamera = true;
         Tween tween = CreateTween();
         managers.MenuManager.FadeToBlack(tween);

         managers.Controller.RegularSpeed = 5f;
         managers.Controller.SprintSpeed = 10f;

         await ToSignal(tween, Tween.SignalName.Finished);

         material.SetShaderParameter("scale", 0);

         for (int i = 0; i < managers.PartyManager.Party.Count; i++)
         {
            if (managers.PartyManager.Party[i].characterType == CharacterType.Vakthol)
            {
               managers.PartyManager.Party[i].model.GlobalPosition = new Vector3(-1.591f, 0, 71.755f);
               managers.PartyManager.Party[i].model.GetNode<Node3D>("Model").GlobalRotation = new Vector3(0f, Mathf.DegToRad(-162.4f), 0f);
            }
            else if (managers.PartyManager.Party[i].characterType == CharacterType.Thalria)
            {
               managers.PartyManager.Party[i].model.GlobalPosition = new Vector3(-0.152f, 0, 72.25f);
               managers.PartyManager.Party[i].model.GetNode<Node3D>("Model").GlobalRotation = new Vector3(0f, Mathf.DegToRad(-94.6f), 0f);
            }
            else if (managers.PartyManager.Party[i].characterType == CharacterType.Athlia)
            {
               managers.PartyManager.Party[i].model.GlobalPosition = new Vector3(-1.576f, 0f, 73.312f);
               managers.PartyManager.Party[i].model.GetNode<Node3D>("Model").GlobalRotation = new Vector3(0f, Mathf.DegToRad(-63.6f), 0f);
            }
            else
            {
               managers.PartyManager.Party[i].model.GlobalPosition = new Vector3(-3.144f, 0f, 72.6f);
               managers.PartyManager.Party[i].model.GetNode<Node3D>("Model").GlobalRotation = new Vector3(0f, Mathf.DegToRad(-97f), 0f);
            }
         }

         GetNode<Node3D>("/root/BaseNode/Level/cutscene6_objects").Visible = true;

         await ToSignal(GetTree().CreateTimer(5f), "timeout");

         managers.MenuManager.FadeFromBlack();
         managers.Controller.DisableCamera = false;
         CutsceneTrigger trigger = GetNode<CutsceneTrigger>("cutscene6");
         managers.CutsceneManager.InitiateCutscene(trigger.cutsceneObject, trigger.id);
      }
   }

   public void HideObjects()
   {
      GetNode<Node3D>("/root/BaseNode/Level/cutscene6_objects").Visible = false;
   }
}
