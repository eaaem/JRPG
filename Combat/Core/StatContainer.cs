using Godot;
using System;

[GlobalClass]
public partial class StatContainer : Resource
{
   [Export]
	public Stat[] stats = new Stat[10];
}