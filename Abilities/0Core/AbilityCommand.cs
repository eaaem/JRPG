#if TOOLS
using Godot;
using Godot.Collections;

/// <summary>
/// A command for graphical actions during ability usage.
/// </summary>
public enum AbilityCommandType
{
   /// <summary>
   /// A blank command. This should not be used.
   /// </summary>
   None,
   /// <summary>
   /// Creates a node. This can be an empty node, for targets and parents, or an effect node.
   /// <br></br><br></br>
   /// <c>NodeName</c> (string) : the name of the new node
   /// <br></br>
   /// <c>Path</c> (string) : the path to the parent of the new node
   /// <br></br>
   /// <c>SpecialCodeOverride</c> (SpecialCodeOverride) : overrides the absolute path with a specially defined path
   /// <br></br>
   /// <c>Target</c> (Vector3) : the location of the node, relative to its parent
   /// <br></br>
   /// <c>PathToScene</c> (string) : the path to the packed scene to instantiate from; leave blank if creating an empty node
   /// </summary>
   CreateNode,
   /// <summary>
   /// Sets the camera's target. Unless otherwise specified, all positions in other commands will be relative to the target.
   /// <br></br><br></br>
   /// <c>Path</c> (string) : the path to the target
   /// <br></br>
   /// <c>SpecialCodeOverride</c> (SpecialCodeOverride) : overrides the absolute path with a specially defined path
   /// </summary>
   CameraSetTarget,
   /// <summary>
   /// Sets the parent of the camera.
   /// <br></br><br></br>
   /// <c>Path</c> (string) : the path to the parent
   /// <br></br>
   /// <c>SpecialCodeOverride</c> (SpecialCodeOverride) : overrides the absolute path with a specially defined path
   /// </summary>
   CameraSetParent,
   /// <summary>
   /// Places the camera at a certain position.
   /// <br></br><br></br>
   /// <c>Target</c> (Vector3) : the position
   /// <br></br>
   /// <c>UseLocal</c> (bool) : whether to use a position local to the parent or a global position
   /// </summary>
   CameraPlace,
   /// <summary>
   /// Immediately rotates the camera.
   /// <br></br><br></br>
   /// <c>Target</c> (Vector3) : the target rotation vector, in degrees
   /// </summary>
   CameraRotateInstantly,
   /// <summary>
   /// Smoothly pans the camera to a certain position. This changes the camera's local position, not the position of the target.
   /// <br></br><br></br>
   /// <c>Target</c> (Vector3) : the target position
   /// <br></br>
   /// <c>Speed</c> (float) : the speed of the pan
   /// </summary>
   CameraPan,
   /// <summary>
   /// Smoothly orbits the camera around its parent.
   /// <br></br><br></br>
   /// <c>Target</c> (Vector3) : the amount to orbit along the 3 axes, in degrees
   /// <br></br>
   /// <c>Speed</c> (float) : the speed of the orbit
   /// </summary>
   CameraOrbit,
   /// <summary>
   /// Turns the camera to track the currently set camera target. The camera will continue tracking until a non-pause command executes.
   /// <br></br><br></br>
   /// <c>LookImmediately</c> (bool) : whether to immediately snap to looking at the target or slowly pan over
   /// <br></br>
   /// <c>Speed</c> (float) : the speed to rotate
   /// </summary>
   CameraLookAtTarget,
   /// <summary>
   /// Calls a method to trigger an effect. This method should be found in the corresponding ability script.
   /// <br></br><br></br>
   /// <c>Method</c> (string) : the name of the method to call
   /// </summary>
   TriggerEffect,
   /// <summary>
   /// Pauses all commands for a certain amount of time.
   /// <br></br><br></br>
   /// <c>Amount</c> (float) : the amount of time to pause
   /// <br></br>
   /// <c>PauseAnimation</c> (string) : pauses for a WaitTimeEvent according to the currently played animation of a fighter; this is added to Amount
   /// <br></br>
   /// <c>Path</c> (string) : the path to a fighter/node that has an AnimationPreferences as a child; applies only when PauseAnimation is not empty
   /// <br></br>
   /// <c>SpecialCodeOverride</c> (SpecialCodeOverride) : overrides the involved path with a special code; applies only when PauseAnimation is not empty
   /// </summary>
   Pause,
   /// <summary>
   /// Emits the ShowDamage signal, telling the combat manager to display damage text and play hit animations.
   /// </summary>
   ShowDamage,
   /// <summary>
   /// Makes one or more fighters play an animation.
   /// <br></br><br></br>
   /// <c>InvolvedFighter</c> (string) : the fighter(s) to play the animation
   /// <br></br>
   /// <c>SpecialCodeOverride</c> (SpecialCodeOverride) : overrides the absolute fighter name with a defined fighter
   /// <br></br>
   /// <c>TargetName</c> (float) : the name of the animation to play
   /// </summary>
   PlayAnimation,
   /// <summary>
   /// Makes one or more fighters run to a target, pause for a certain amount of time, and then run back to their original position. Only one target is supported,
   /// but there can be several runners.
   /// <br></br><br></br>
   /// <c>InvolvedFighter</c> (string) : the fighter(s) to run
   /// <br></br>
   /// <c>SpecialCodeOverride</c> (SpecialCodeOverride) : overrides the involved fighter with a defined fighter
   /// <br></br>
   /// <c>TargetName</c> (string) : the fighter to target
   /// <br></br>
   /// <c>TargetCodeOverride</c> (SpecialCodeOverride) : overrides the targeted fighter with a defined fighter
   /// <br></br>
   /// <c>Amount</c> (float) : the amount of time to pause
   /// <br></br>
   /// <c>PauseAnimation</c> (string) : pauses for a WaitTimeEvent according to the currently played animation; this is added to Amount
   /// </summary>
   RunToFighter,
   /// <summary>
   /// Used with RunToFighter to pause until either the runner reaches their target or returns to their original position. If there are several runners, runners
   /// will wait until all other runners are finished with the current goal.
   /// <br></br><br></br>
   /// <c>PauseUntilTargetReached</c> (bool) : whether to pause commands until the fighter reaches their target; if false, the pause is until the fighter returns to
   /// their origin
   /// </summary>
   PauseDuringRun,
   /// <summary>
   /// Rotates a fighter by a certain quantity or to look at something.
   /// <br></br><br></br>
   /// <c>InvolvedFighter</c> (string) : the fighter(s) to rotate
   /// <br></br>
   /// <c>SpecialCodeOverride</c> (SpecialCodeOverride) : overrides the involved fighter with a defined fighter
   /// <br></br>
   /// <c>Target</c> (Vector3) : the amount to orbit along the 3 axes, in degrees
   /// <br></br>
   /// <c>TargetName</c> (string) : the node/fighter to target; leave blank if looking at a target isn't desired
   /// <br></br>
   /// <c>TargetCodeOverride</c> (SpecialCodeOverride) : overrides the target
   /// <br></br>
   /// <c>Amount</c> (float) : how fast to rotate
   /// <br></br>
   /// <c>RotateImmediately</c> (bool) : whether to snap the rotation rather than rotate smoothly
   /// </summary>
   RotateFighter,
   /// <summary>
   /// Pauses all commands until rotating fighters are done rotating.
   /// </summary>
   PauseDuringRotate,
   /// <summary>
   /// Resets the camera to the combat manager's default position and rotation. This also resets the camera's target and parent. 
   /// </summary>
   Reset
}

/// <summary>
/// Overrides a path/fighter using a special, predefined code.
/// </summary>
public enum SpecialCodeOverride {
   None,
   /// <summary>
   /// Gets the placement node of the caster
   /// </summary>
   CasterPlacement,
   /// <summary>
   /// Gets the model of the caster
   /// </summary>
   CasterModel,
   /// <summary>
   /// Gets the placement nodes of all targets; for SetTarget and SetParent, this will pick the first placement node found, while for CreateNode, 
   /// a node will be created for each fighter
   /// </summary>
   TargetsPlacement,
   /// <summary>
   /// Gets the models of all targets; for SetTarget and SetParent, this will pick the first model found, while for CreateNode, 
   /// a node will be created for each fighter
   /// </summary>
   TargetsModel,
   /// <summary>
   /// Gets a node created by a previous command; put the name of the node in the string field that usually provides the path/fighter
   /// </summary>
   CreatedNode
}

[GlobalClass, Tool]
public partial class AbilityCommand : Resource
{
   AbilityCommandType commandType;
   public AbilityCommandType CommandType
   {
      get
      {
         return commandType;
      }
      set
      {
         commandType = value;
         NotifyPropertyListChanged();
      }
   }

   public Vector3 Target { get; set; }
   public string Path { get; set; }
   public string PathToScene { get; set; }
   public string NodeName { get; set; }
   public bool UseLocal { get; set; }
   public bool LookImmediately { get; set; }
   public string Method { get; set;}
   public float Amount { get; set; }
   public string PauseAnimation { get; set; }
   public float Speed { get; set; }
   public SpecialCodeOverride SpecialCodeOverride { get; set; }
   public SpecialCodeOverride TargetCodeOverride { get; set; }

   public string InvolvedFighter { get; set; }
   public string TargetName { get; set; }
   public bool PauseUntilTargetReached { get; set; }

   public override Array<Dictionary> _GetPropertyList()
   {
      Array<Dictionary> result = new Array<Dictionary>();

      result.Add(new Dictionary()
      {
         { "name", $"CommandType" },
         { "type", (int)Variant.Type.Int },
         { "hint", (int)PropertyHint.Enum },
         { "hint_string", "None,CreateNode,CameraSetTarget,CameraSetParent,CameraPlace,CameraRotateInstantly,CameraPan,CameraOrbit,CameraLookAtTarget,"
                           + "TriggerEffect,Pause,ShowDamage,PlayAnimation,RunToFighter,PauseDuringRun,RotateFighter,PauseDuringRotate,Reset" }
      });

      switch (commandType)
      {
         case AbilityCommandType.CreateNode:
            result.Add(new Dictionary()
            {
               { "name", $"NodeName" },
               { "type", (int)Variant.Type.String }
            });

            result.Add(new Dictionary()
            {
               { "name", $"Path" },
               { "type", (int)Variant.Type.String }
            });

            result.Add(new Dictionary()
            {
               { "name", $"SpecialCodeOverride" },
               { "type", (int)Variant.Type.Int },
               { "hint", (int)PropertyHint.Enum },
               { "hint_string", "None,CasterPlacement,CasterModel,TargetsPlacement,TargetsModel,CreatedNode" }
            });

            result.Add(new Dictionary()
            {
               { "name", $"Target" },
               { "type", (int)Variant.Type.Vector3 }
            });

            result.Add(new Dictionary()
            {
               { "name", $"PathToScene" },
               { "type", (int)Variant.Type.String }
            });

            break;
         case AbilityCommandType.CameraSetTarget:
            result.Add(new Dictionary()
            {
               { "name", $"Path" },
               { "type", (int)Variant.Type.String }
            });

            result.Add(new Dictionary()
            {
               { "name", $"SpecialCodeOverride" },
               { "type", (int)Variant.Type.Int },
               { "hint", (int)PropertyHint.Enum },
               { "hint_string", "None,CasterPlacement,CasterModel,TargetsPlacement,TargetsModel,CreatedNode" }
            });

            break;
         case AbilityCommandType.CameraSetParent:
            result.Add(new Dictionary()
            {
               { "name", $"Path" },
               { "type", (int)Variant.Type.String }
            });

            result.Add(new Dictionary()
            {
               { "name", $"SpecialCodeOverride" },
               { "type", (int)Variant.Type.Int },
               { "hint", (int)PropertyHint.Enum },
               { "hint_string", "None,CasterPlacement,CasterModel,TargetsPlacement,TargetsModel,CreatedNode" }
            });

            break;
         case AbilityCommandType.CameraPlace:
            result.Add(new Dictionary()
            {
               { "name", $"Target" },
               { "type", (int)Variant.Type.Vector3 }
            });

            result.Add(new Dictionary()
            {
               { "name", $"UseLocal" },
               { "type", (int)Variant.Type.Bool }
            });

            break;
         case AbilityCommandType.CameraRotateInstantly:
            result.Add(new Dictionary()
            {
               { "name", $"Target" },
               { "type", (int)Variant.Type.Vector3 }
            });

            break;
         case AbilityCommandType.CameraPan:
            result.Add(new Dictionary()
            {
               { "name", $"Target" },
               { "type", (int)Variant.Type.Vector3 }
            });

            result.Add(new Dictionary()
            {
               { "name", $"Speed" },
               { "type", (int)Variant.Type.Float }
            });

            break;
         case AbilityCommandType.CameraOrbit:
            result.Add(new Dictionary()
            {
               { "name", $"Target" },
               { "type", (int)Variant.Type.Vector3 }
            });

            result.Add(new Dictionary()
            {
               { "name", $"Speed" },
               { "type", (int)Variant.Type.Float }
            });

            break;
         case AbilityCommandType.CameraLookAtTarget:
            result.Add(new Dictionary()
            {
               { "name", $"LookImmediately" },
               { "type", (int)Variant.Type.Bool }
            });

            result.Add(new Dictionary()
            {
               { "name", $"Speed" },
               { "type", (int)Variant.Type.Float }
            });

            break;
         case AbilityCommandType.TriggerEffect:
            result.Add(new Dictionary()
            {
               { "name", $"Method" },
               { "type", (int)Variant.Type.String }
            });

            break;
         case AbilityCommandType.Pause:
            result.Add(new Dictionary()
            {
               { "name", $"Amount" },
               { "type", (int)Variant.Type.Float }
            });

            result.Add(new Dictionary()
            {
               { "name", $"PauseAnimation" },
               { "type", (int)Variant.Type.String }
            });   

            result.Add(new Dictionary()
            {
               { "name", $"Path" },
               { "type", (int)Variant.Type.String }
            });

            result.Add(new Dictionary()
            {
               { "name", $"SpecialCodeOverride" },
               { "type", (int)Variant.Type.Int },
               { "hint", (int)PropertyHint.Enum },
               { "hint_string", "None,CasterPlacement,CasterModel,TargetsPlacement,TargetsModel,CreatedNode" }
            });

            break;
         case AbilityCommandType.PlayAnimation:
            result.Add(new Dictionary()
            {
               { "name", $"InvolvedFighter" },
               { "type", (int)Variant.Type.String }
            });

            result.Add(new Dictionary()
            {
               { "name", $"SpecialCodeOverride" },
               { "type", (int)Variant.Type.Int },
               { "hint", (int)PropertyHint.Enum },
               { "hint_string", "None,CasterPlacement,CasterModel,TargetsPlacement,TargetsModel,CreatedNode" }
            });

            result.Add(new Dictionary()
            {
               { "name", $"TargetName" },
               { "type", (int)Variant.Type.String }
            });
         
            break;
         case AbilityCommandType.RunToFighter:
            result.Add(new Dictionary()
            {
               { "name", $"InvolvedFighter" },
               { "type", (int)Variant.Type.String }
            });

            result.Add(new Dictionary()
            {
               { "name", $"SpecialCodeOverride" },
               { "type", (int)Variant.Type.Int },
               { "hint", (int)PropertyHint.Enum },
               { "hint_string", "None,CasterPlacement,CasterModel,TargetsPlacement,TargetsModel,CreatedNode" }
            });

            result.Add(new Dictionary()
            {
               { "name", $"TargetName" },
               { "type", (int)Variant.Type.String }
            });

            result.Add(new Dictionary()
            {
               { "name", $"TargetCodeOverride" },
               { "type", (int)Variant.Type.Int },
               { "hint", (int)PropertyHint.Enum },
               { "hint_string", "None,CasterPlacement,CasterModel,TargetsPlacement,TargetsModel,CreatedNode" }
            });

            result.Add(new Dictionary()
            {
               { "name", $"Amount" },
               { "type", (int)Variant.Type.Float }
            });

            result.Add(new Dictionary()
            {
               { "name", $"PauseAnimation" },
               { "type", (int)Variant.Type.String }
            });           
         
            break;
         case AbilityCommandType.PauseDuringRun:
            result.Add(new Dictionary()
            {
               { "name", $"PauseUntilTargetReached" },
               { "type", (int)Variant.Type.Bool }
            });
         
            break;
         case AbilityCommandType.RotateFighter:
            result.Add(new Dictionary()
            {
               { "name", $"InvolvedFighter" },
               { "type", (int)Variant.Type.String }
            });

            result.Add(new Dictionary()
            {
               { "name", $"SpecialCodeOverride" },
               { "type", (int)Variant.Type.Int },
               { "hint", (int)PropertyHint.Enum },
               { "hint_string", "None,CasterPlacement,CasterModel,TargetsPlacement,TargetsModel,CreatedNode" }
            });

            result.Add(new Dictionary()
            {
               { "name", $"Target" },
               { "type", (int)Variant.Type.Vector3 }
            });


            result.Add(new Dictionary()
            {
               { "name", $"TargetName" },
               { "type", (int)Variant.Type.String }
            });

            result.Add(new Dictionary()
            {
               { "name", $"TargetCodeOverride" },
               { "type", (int)Variant.Type.Int },
               { "hint", (int)PropertyHint.Enum },
               { "hint_string", "None,CasterPlacement,CasterModel,TargetsPlacement,TargetsModel,CreatedNode" }
            });

            result.Add(new Dictionary()
            {
               { "name", $"Amount" },
               { "type", (int)Variant.Type.Float }
            });

            result.Add(new Dictionary()
            {
               { "name", $"RotateImmediately" },
               { "type", (int)Variant.Type.Bool }
            });

            break;
      }

      return result;
   }
}
#endif