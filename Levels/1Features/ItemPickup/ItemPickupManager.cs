using Godot;
using System;

public partial class ItemPickupManager : Node
{
   private Node3D baseNode;
   private Camera3D camera;
   [Export]
   private ManagerReferenceHolder managers;

   public CanvasGroup itemPickupContainer;
   private Label itemPickupText;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
      baseNode = GetNode<Node3D>("/root/BaseNode");
      camera = managers.Controller.GetNode<Camera3D>("CameraTarget/SpringArm3D/PlayerCamera");

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
                  managers.Controller.DisableMovement = true;
                  managers.Controller.DisableCamera = true;

                  string plural = itemHolder.quantity > 1 ? "s" : "";
                  itemPickupText.Text = "Picked up " + itemHolder.quantity + " " + itemHolder.heldItem.name + plural + "!";
                  itemPickupContainer.Visible = true;

                  managers.PartyManager.AddItem(new InventoryItem(itemHolder.heldItem, itemHolder.quantity));

                  managers.LevelManager.LocationDatas[managers.LevelManager.ActiveLocationDataID].pickedUpItems[itemHolder.id.ToString()] = true;

                  collided.GetParent().QueueFree();
               } 
            }
         }
         else
         {
            managers.Controller.DisableMovement = false;
            managers.Controller.DisableCamera = false;
            itemPickupContainer.Visible = false;
         }
      }
   }
}
