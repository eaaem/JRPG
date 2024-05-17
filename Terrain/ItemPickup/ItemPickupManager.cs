using Godot;
using System;

public partial class ItemPickupManager : Node
{
   private Node3D baseNode;
   private Camera3D camera;
   private PartyManager partyManager;
   private CharacterController controller;
   private LevelManager levelManager;

   public CanvasGroup itemPickupContainer;
   private Label itemPickupText;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
      baseNode = GetNode<Node3D>("/root/BaseNode");
      controller = baseNode.GetNode<CharacterController>("PartyMembers/Member1");
      camera = controller.GetNode<Camera3D>("CameraTarget/PlayerCamera");
      partyManager = baseNode.GetNode<PartyManager>("PartyManagerObj");
      levelManager = baseNode.GetNode<LevelManager>("LevelManager");

      itemPickupContainer = GetParent<CanvasGroup>();
      itemPickupText = GetNode<Label>("Text");
	}

   public override void _Input(InputEvent @event)
   {
      if (@event.IsActionPressed("interact"))
      {
         if (!itemPickupContainer.Visible)
         {
            PhysicsDirectSpaceState3D spaceState = baseNode.GetWorld3D().DirectSpaceState;
            Vector2 mousePosition = GetViewport().GetMousePosition();

            Vector3 origin = camera.ProjectRayOrigin(mousePosition);
            Vector3 end = origin + camera.ProjectRayNormal(mousePosition) * 10;
            // 16 = layer 5, which is where items rest
            PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(origin, end, 16);
            query.CollideWithAreas = true;

            var result = spaceState.IntersectRay(query);

            if (result.Count > 0)
            {
               StaticBody3D collided = (StaticBody3D)result["collider"];

               if (collided.GetParent().HasNode("ItemHolder"))
               {
                  ItemHolder itemHolder = collided.GetParent().GetNode<ItemHolder>("ItemHolder");
                  controller.DisableMovement = true;

                  string plural = itemHolder.quantity > 1 ? "s" : "";
                  itemPickupText.Text = "Picked up " + itemHolder.quantity + " " + itemHolder.heldItem.name + plural + "!";
                  itemPickupContainer.Visible = true;

                  partyManager.AddItem(new InventoryItem(itemHolder.heldItem, itemHolder.quantity));

                  levelManager.LocationDatas[levelManager.ActiveLocationDataID].pickedUpItems[itemHolder.id.ToString()] = true;

                  collided.GetParent().QueueFree();
               } 
            }
         }
         else
         {
            controller.DisableMovement = false;
            itemPickupContainer.Visible = false;
         }
      }
   }
}
