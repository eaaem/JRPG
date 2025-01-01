using Godot;
using System;

public partial class Popup : Label
{
   float duration;

   public void ReceiveInfo(float duration, string message)
   {
      Text = "  " + message + "  ";
      this.duration = duration;

      ZIndex = 5;
      Pop();
   }     

   async void Pop()
   {
      while (Modulate.A < 1)
      {
         Color newColor = new Color(Modulate.R, Modulate.G, Modulate.B, Modulate.A + 0.05f);
         Modulate = newColor;
         await ToSignal(GetTree().CreateTimer(0.01f), "timeout");
      }

      await ToSignal(GetTree().CreateTimer(duration), "timeout");

      while (Modulate.A > 0)
      {
         Color newColor = new Color(Modulate.R, Modulate.G, Modulate.B, Modulate.A - 0.1f);
         Modulate = newColor;
         await ToSignal(GetTree().CreateTimer(0.01f), "timeout");
      }

      GetParent().RemoveChild(this);
      QueueFree();
   }
}
