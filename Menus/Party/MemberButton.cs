using Godot;
using System;

public partial class MemberButton : Node
{
	private PartyMenuManager partyMenuManager;
   private ItemMenuManager itemMenuManager;

   string memberName;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
      partyMenuManager = GetNode<PartyMenuManager>("/root/BaseNode/UI/PartyMenuLayer/PartyMenu/MenuContainer/Party");
      itemMenuManager = GetNode<ItemMenuManager>("/root/BaseNode/UI/PartyMenuLayer/PartyMenu/MenuContainer/Items");
      GetParent<Button>().ButtonDown += OnMemberDown;
      memberName = GetNode<RichTextLabel>("../Title").Text;
	}

   void OnMemberDown()
   {
      if (partyMenuManager.isActive) // Party menu; swap party members or switch screens
      {
         if (!partyMenuManager.isSwapping)
         {
            partyMenuManager.LoadNewPartyScreen(memberName);
         }
         else
         {
            if (partyMenuManager.firstSwap == null)
            {
               partyMenuManager.firstSwap = memberName;
               GetNode<Panel>("../SwapHighlight").Visible = true;
            }
            else
            {
               partyMenuManager.CompleteSwap(memberName);
            }
         }
      }
      else // Item menu; apply items
      {
         itemMenuManager.ProcessMemberButtonClick(memberName);
      }
   }

   public override void _ExitTree()
   {
      GetParent<Button>().ButtonDown -= OnMemberDown;
   }
}
