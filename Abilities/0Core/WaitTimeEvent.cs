using Godot;
using System;

[GlobalClass]
public partial class WaitTimeEvent : Resource
{
   [Export]
   public string eventName;
   [Export]
   public float waitTime;
   [Export]
   public string projectilePath;
}
