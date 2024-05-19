#if TOOLS
using Godot;
using System;

[Tool]
public partial class Cutscenes : EditorPlugin
{
   InspectorPlugin plugin;

	public override void _EnterTree()
	{
      plugin = new InspectorPlugin();
      AddInspectorPlugin(plugin);

      AddCustomType("Cutscene", "Cutscene", GD.Load<CSharpScript>("res://addons/cutscenes/Cutscene.cs"), new Texture2D());
	}

	public override void _ExitTree()
	{
		RemoveInspectorPlugin(plugin);
      RemoveCustomType("Cutscene");
	}
}
#endif
