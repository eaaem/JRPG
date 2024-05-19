using Godot;
using System;
using System.Collections.Generic;

public partial class InspectorPlugin : EditorInspectorPlugin
{
   public override bool _CanHandle(GodotObject @object)
   {
      //GD.Print(@object.GetPropertyList());
      return true;
   }
}
