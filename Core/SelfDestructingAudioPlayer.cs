using Godot;
using System;

public partial class SelfDestructingAudioPlayer : Node
{
	void OnFinish()
   {
      GetParent().GetParent().GetParent().RemoveChild(GetParent().GetParent());
      GetParent().GetParent().QueueFree();
   }
}
