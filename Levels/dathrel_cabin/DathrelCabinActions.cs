using Godot;
using System;

public partial class DathrelCabinActions : Node
{
   public void DisplayVaktholAxe()
   {
      GetNode<Node3D>("../vakthol_axe").Visible = true;
   }

	public void HideVaktholAxe()
   {
      GetNode<Node3D>("../vakthol_axe").Visible = false;
   }
}
