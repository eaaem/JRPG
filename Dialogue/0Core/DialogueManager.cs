using Godot;
using System;
using System.Collections.Generic;

/* A brief description of how dialogue branches work:
In the inspector, dialogue lists have a dialogue option (the name of the branch) and have a list of their own branching dialogues.
This program also keeps track of every single previously accessed dialogue list. Going back accesses the last previously accessed dialogue,
then deletes the last previously accessed dialogue.
*/

public partial class DialogueManager : Node
{
   [Export]
   private int dialogueWidth = 1700;
   [Export]
   private int dialogueHeight = 300;
   [Export]
   private int titleHeight = 65;
   [Export]
   private int spriteSize = 210;
   [Export]
   private int buffer = 5;
   [Export]
   private float textSpeed = 0.01f;
   [Export]
   public InspectorDialogueInteraction[] dialoguesInThisRegion = new InspectorDialogueInteraction[5];

   public CanvasGroup DialogueContainer { get; set; }
   private Sprite2D sprite;
   private RichTextLabel body;
   private RichTextLabel title;

   [Export]
   private PackedScene branchingButton;
   private VBoxContainer branchingDialogueContainer;

   public DialogueInteraction CurrentInteraction { get; set; }
   private DialogueList currentDialogueList;

   [Export]
   private ManagerReferenceHolder managers;

   private bool currentObjectFinished;
   public int CurrentIndex { get; set; }
   private bool validToSkip;

   public bool DialogueIsActive { get; set; }

   private StaticBody3D currentCollider;

   List<DialogueList> previousDialogueLists = new List<DialogueList>();

   private Camera3D camera;
   private Node3D playerModel;

   private bool isCurrentlyCutscene;
   public bool CutsceneReadyForNextDialogue { get; set; }
   public bool LockInput { get; set; }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
      DialogueContainer = GetParent<CanvasGroup>();
      sprite = GetNode<Sprite2D>("Sprite");
      body = GetNode<RichTextLabel>("Body");
      title = GetNode<RichTextLabel>("Title");

      playerModel = managers.Controller.GetNode<Node3D>("Model");
      camera = managers.Controller.GetNode<Camera3D>("CameraTarget/PlayerCamera");
      
      branchingDialogueContainer = GetNode<VBoxContainer>("Branches");
	}

	public void InitiateDialogue(DialogueInteraction interaction, bool isCutscene, bool isShopExit = false)
   {
      if (!DialogueIsActive)
      {
         isCurrentlyCutscene = isCutscene;

         CurrentInteraction = interaction;
         DialogueContainer.Visible = true;
         managers.Controller.DisableMovement = true;
         Input.MouseMode = Input.MouseModeEnum.Visible;
         DialogueIsActive = true;

         currentDialogueList = CurrentInteraction.dialogueList;
         previousDialogueLists.Clear();

         if (isShopExit)
         {
            previousDialogueLists.Add(currentDialogueList);
            currentDialogueList = interaction.exitShopDialogue;
         }
         
         NextDialogue(0);
      }
   }

   public async void NextDialogue(int index)
   {
      DialogueObject currentObject = currentDialogueList.dialogues[index];

      if (currentObject.SubstituteDialogues.Length > 0)
      {
         // This is a substitute dialogue for a speaker that is not a party member (since there are no specifics for the initiator)
         if (currentObject.Initiator != CharacterType.None)
         {
            if (currentObject.Initiator != managers.PartyManager.Party[0].characterType)
            {
               for (int i = 0; i < currentObject.SubstituteDialogues.Length; i++)
               {
                  if (currentObject.SubstituteDialogues[i].Initiator == managers.PartyManager.Party[0].characterType)
                  {
                     currentObject = currentObject.SubstituteDialogues[i];
                  }
               }
            }
         }
         else // This is a substitute dialogue for a party member
         {
            if (currentObject.Initiator != managers.PartyManager.Party[0].characterType)
            {
               for (int i = 0; i < currentObject.SubstituteDialogues.Length; i++)
               {
                  if (currentObject.SubstituteDialogues[i].Initiator == managers.PartyManager.Party[0].characterType)
                  {
                     currentObject = currentObject.SubstituteDialogues[i];
                  }
               }
            }
         }
      }

      // Used to skip objects, in case of situations where a few playable characters may have dialogue options yet others don't
      if (currentObject.content == "EMPTY")
      {
         NextDialogue(index + 1);
      }

      if (currentObject.opensShop)
      {
         DialogueContainer.Visible = false;
         managers.ShopMenuManager.OpenShop(currentCollider.GetParent().GetNode<ShopItem>("ShopHolder"));
         DialogueIsActive = false;
      }

      CutsceneReadyForNextDialogue = false;

      CurrentIndex = index;
      currentObjectFinished = false;

      sprite.Visible = false;

      if (currentObject.characterType != CharacterType.None)
      {
        // body.Size = new Vector2(dialogueWidth - spriteSize - buffer, dialogueHeight - titleHeight);
         //body.Position = new Vector2(spriteSize + buffer, titleHeight);
         //title.Size = new Vector2(dialogueWidth - spriteSize - buffer, titleHeight);
         //title.Position = new Vector2(spriteSize + buffer, 0);
         title.Text = currentObject.characterType + "";

         if (currentObject.emotion != Emotion.None)
         {
            Texture2D spriteTexture = GD.Load<Texture2D>("res://Dialogue/Sprites/" + currentObject.characterType + "/" + currentObject.emotion + ".png");
            sprite.Texture = spriteTexture;
            sprite.Visible = true;
         }
      }
      else
      {
       //  body.Size = new Vector2(dialogueWidth, dialogueHeight - titleHeight);
         //body.Position = new Vector2(0, titleHeight);
        // title.Size = new Vector2(dialogueWidth, titleHeight);
        // title.Position = Vector2.Zero;
         title.Text = currentObject.speaker;
      }

      body.Size = new Vector2(dialogueWidth, dialogueHeight - titleHeight);
      body.Position = new Vector2(0, titleHeight);
      title.Size = new Vector2(dialogueWidth, titleHeight);
      title.Position = Vector2.Zero;
  
      body.Text = "";

      if (currentDialogueList.branchingDialogues.Length > 0)
      {
         ClearBranches();
         GenerateBranches();
         branchingDialogueContainer.Visible = true;
      }

      for (int i = 0; i < currentObject.content.Length; i++)
      {
         await ToSignal(GetTree().CreateTimer(textSpeed), "timeout");

         if (currentObjectFinished)
         {
            break;
         }

         validToSkip = true;

         body.Text += currentObject.content[i];

         // Add BBcode tags in blocks; otherwise, they'll be entered in awkwardly one character at a time
         if (currentObject.content[i] == '[')
         {
            while (currentObject.content[i] != ']')
            {
               i++;
               body.Text += currentObject.content[i];
            }
         }
         
      }

      currentObjectFinished = true;
   }

   void GenerateBranches()
   {
      PackedScene buttonScene = GD.Load<PackedScene>(branchingButton.ResourcePath);
      for (int i = 0; i < currentDialogueList.branchingDialogues.Length; i++)
      {
         Button currentBranchButton = buttonScene.Instantiate<Button>();
         currentBranchButton.Text = currentDialogueList.branchingDialogues[i].branchName;
         
         branchingDialogueContainer.AddChild(currentBranchButton);
      }
   }

   void ClearBranches()
   {
      foreach (Button child in branchingDialogueContainer.GetChildren())
      {
         branchingDialogueContainer.RemoveChild(child);
         child.QueueFree();
      }
   }

   public void ReceiveBranchDown(string branchingName)
   {
      if (branchingName == "Back")
      {
         NavigateToPreviousDialogue();
         NextDialogue(0);
         return;
      }

      previousDialogueLists.Add(currentDialogueList);

      for (int i = 0; i < currentDialogueList.branchingDialogues.Length; i++)
      {
         if (currentDialogueList.branchingDialogues[i].branchName == branchingName)
         {
            currentDialogueList = currentDialogueList.branchingDialogues[i];
            branchingDialogueContainer.Visible = false;
            NextDialogue(0);
            return;
         }
      }
   }

   void NavigateToPreviousDialogue()
   {
      currentDialogueList = previousDialogueLists[previousDialogueLists.Count - 1];
      previousDialogueLists.RemoveAt(previousDialogueLists.Count - 1);
   }

   public void NextCutsceneDialogue()
   {
      if (isCurrentlyCutscene)
      {
         managers.CutsceneManager.ProgressCutscene(CurrentIndex + 1);
      }

      NextDialogue(CurrentIndex + 1);
   }

   public override void _Input(InputEvent @event)
   {
      if (@event.IsActionPressed("interact") && !LockInput)
      {
         if (DialogueIsActive)
         {
            if (currentObjectFinished)
            {
               if (CurrentIndex == currentDialogueList.dialogues.Length - 1)
               {
                  if (currentDialogueList != CurrentInteraction.dialogueList)
                  {
                     NavigateToPreviousDialogue();

                     NextDialogue(0);
                  }
                  else
                  {
                     ExitDialogue();

                     if (isCurrentlyCutscene)
                     {
                        managers.CutsceneManager.EndCutscene();
                        isCurrentlyCutscene = false;
                     }
                  }
               }
               else
               {
                  NextCutsceneDialogue();
               }
            }
            else if (validToSkip)
            {
               // Complete the content immediately if the player hits interact while it's still being typed in
               body.Text = currentDialogueList.dialogues[CurrentIndex].content;
               currentObjectFinished = true;
            }
         }
         else
         {
            PhysicsDirectSpaceState3D spaceState = GetNode<Node3D>("/root/BaseNode").GetWorld3D().DirectSpaceState;
            Vector2 mousePosition = GetViewport().GetMousePosition();

            Vector3 origin = camera.ProjectRayOrigin(mousePosition);
            Vector3 end = origin + camera.ProjectRayNormal(mousePosition) * 5;
            // 4 = layer 3, which is where dialogue objects rest
            PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(origin, end, 4);
            query.CollideWithAreas = true;

            var result = spaceState.IntersectRay(query);

            if (result.Count > 0)
            {
               StaticBody3D collided = (StaticBody3D)result["collider"];

               currentCollider = collided;

               if (collided.GetParent().HasNode("DialogueHolder"))
               {
                  DialogueInteraction interaction = collided.GetParent().GetNode<DialogueInteraction>("DialogueHolder");

                  if (!DialogueIsActive)
                  {
                     InitiateDialogue(interaction, false);
                  }
               } 
            }
         }
      }
   }

   void ExitDialogue()
   {
      DialogueContainer.Visible = false;
      managers.Controller.DisableMovement = false;
      Input.MouseMode = Input.MouseModeEnum.Captured;
      DialogueIsActive = false;
   }
}
