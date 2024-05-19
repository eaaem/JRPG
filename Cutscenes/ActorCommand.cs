#if TOOLS
using Godot;
using Godot.Collections;

public enum CommandType
{
   /// <summary>
   /// A blank command. This should not be selected.
   /// </summary>
   None,
   /// <summary>
   /// Moves an actor to a vector3 position.
   /// <br></br><br></br>
   /// <c>ActorName</c> (string) : the name of the actor to move
   /// <br></br>
   /// <c>Destination</c> (Vector3) : the destination of the actor
   /// </summary>
   Move,
   /// <summary>
   /// Rotates an actor's y-rotation to a new y-rotation.
   /// <br></br><br></br>
   /// <c>ActorName</c> (string) : the name of the actor to rotate
   /// <br></br>
   /// <c>YRotation</c> (float) : the new y-rotation of the actor
   /// </summary>
   Rotate,
   /// <summary>
   /// Immediately rotates an actor's y-rotation to a new y-rotation. Used for staging before the cutscene begins.
   /// <br></br><br></br>
   /// <c>ActorName</c> (string) : the name of the actor to rotate
   /// <br></br>
   /// <c>YRotation</c> (float) : the new y-rotation of the actor
   /// </summary>
   QuickRotate,
   /// <summary>
   /// Shows or hides the dialogue box.
   /// <br></br><br></br>
   /// <c>Hide</c> (bool) : true will hide the box, false will show the box
   /// </summary>
   ChangeDialogueVisibility,
   /// <summary>
   /// Shows or hides an actor's weapon.
   /// <br></br><br></br>
   /// <c>Hide</c> (bool) : true will hide the weapon, false will show the weapon
   /// </summary>
   ChangeWeaponVisibility,
   /// <summary>
   /// Locks or unlocks dialogue input. Used to prevent the player from progressing dialogue.
   /// <br></br><br></br>
   /// <c>MakeLocked</c> (bool) : true will lock the dialogue, false will unlock the dialogue
   /// </summary>
   ChangeDialogueLock,
   /// <summary>
   /// Forces the next dialogue object to fire.
   /// </summary>
   SpeakNext,
   /// <summary>
   /// Sets an actor's idle animation.
   /// <br></br><br></br>
   /// <c>ActorName</c> (string) : the name of the actor
   /// <br></br>
   /// <c>AnimationName</c> (string) : the name of the new idle animation
   /// </summary>
   SetIdleAnimation,
   /// <summary>
   /// Sets an actor's walk animation.
   /// <br></br><br></br>
   /// <c>ActorName</c> (string) : the name of the actor
   /// <br></br>
   /// <c>AnimationName</c> (string) : the name of the new walk animation
   /// </summary>
   SetWalkAnimation,
   /// <summary>
   /// Pauses all commands for the given amount of seconds.
   /// <br></br><br></br>
   /// <c>Pause</c> (float) : the amount of time to wait
   /// </summary>
   Pause,
   /// <summary>
   /// Places an actor at a vector3 position. Used for staging before the cutscene begins.
   /// <br></br><br></br>
   /// <c>ActorName</c> (string) : the name of the actor to rotate
   /// <br></br>
   /// <c>Destination</c> (Vector3) : the point to place the actor at
   /// </summary>
   Place,
   /// <summary>
   /// Makes an actor play an animation. Once finished, the actor will return to their idle or walk animation, depending on their current movement.
   /// <br></br><br></br>
   /// <c>ActorName</c> (string) : the name of the actor
   /// <br></br>
   /// <c>AnimationName</c> (string) : the name of the animation
   /// </summary>
   PlayAnimation
}

/// <summary>
/// Represents a cutscene command to be given to an actor.
/// </summary>
[GlobalClass, Tool]
public partial class ActorCommand : Resource
{
   CommandType commandType;
   public CommandType CommandType
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

   public string ActorName { get; set; }
   public Vector3 Destination { get; set; }
   public float YRotation { get; set; }

   public float Pause { get; set; }

   public bool Hide { get; set; }
   public bool MakeLocked { get; set; }

   public string AnimationName { get; set; }
   public string TargetAnimation { get; set; }

   public override Array<Dictionary> _GetPropertyList()
   {
      Array<Dictionary> result = new Array<Dictionary>();

      result.Add(new Dictionary()
      {
         { "name", $"CommandType" },
         { "type", (int)Variant.Type.Int },
         { "hint", (int)PropertyHint.Enum },
         { "hint_string", "None,Move,Rotate,QuickRotate,ChangeDialogueVisibility,ChangeWeaponVisibility,ChangeDialogueLock,SpeakNext,SetIdleAnimation," + 
                          "SetWalkAnimation,Pause,Place,PlayAnimation" }
      });

      switch (commandType)
      {
         case CommandType.Move:
            result.Add(new Dictionary()
            {
               { "name", $"ActorName" },
               { "type", (int)Variant.Type.String }
            });

            result.Add(new Dictionary()
            {
               { "name", $"Destination" },
               { "type", (int)Variant.Type.Vector3 }
            });
            break;
         case CommandType.Rotate:
            result.Add(new Dictionary()
            {
               { "name", $"ActorName" },
               { "type", (int)Variant.Type.String }
            });

            result.Add(new Dictionary()
            {
               { "name", $"YRotation" },
               { "type", (int)Variant.Type.Float }
            });
            break;
         case CommandType.QuickRotate:
            result.Add(new Dictionary()
            {
               { "name", $"ActorName" },
               { "type", (int)Variant.Type.String }
            });

            result.Add(new Dictionary()
            {
               { "name", $"YRotation" },
               { "type", (int)Variant.Type.Float }
            });
            break;
         case CommandType.ChangeDialogueVisibility:
            result.Add(new Dictionary()
            {
               { "name", $"Hide" },
               { "type", (int)Variant.Type.Bool }
            });
            break;
         case CommandType.ChangeDialogueLock:
            result.Add(new Dictionary()
            {
               { "name", $"MakeLocked" },
               { "type", (int)Variant.Type.Bool }
            });
            break;
         case CommandType.SetIdleAnimation:
            result.Add(new Dictionary()
            {
               { "name", $"ActorName" },
               { "type", (int)Variant.Type.String }
            });

            result.Add(new Dictionary()
            {
               { "name", $"AnimationName" },
               { "type", (int)Variant.Type.String }
            });
            break;
         case CommandType.SetWalkAnimation:
            result.Add(new Dictionary()
            {
               { "name", $"ActorName" },
               { "type", (int)Variant.Type.String }
            });

            result.Add(new Dictionary()
            {
               { "name", $"AnimationName" },
               { "type", (int)Variant.Type.String }
            });
            break;
         case CommandType.Pause:
            result.Add(new Dictionary()
            {
               { "name", $"Pause" },
               { "type", (int)Variant.Type.Float }
            });
            break;
         case CommandType.Place:
            result.Add(new Dictionary()
            {
               { "name", $"ActorName" },
               { "type", (int)Variant.Type.String }
            });

            result.Add(new Dictionary()
            {
               { "name", $"Destination" },
               { "type", (int)Variant.Type.Vector3 }
            });
            break;
         case CommandType.PlayAnimation:
            result.Add(new Dictionary()
            {
               { "name", $"ActorName" },
               { "type", (int)Variant.Type.String }
            });

            result.Add(new Dictionary()
            {
               { "name", $"AnimationName" },
               { "type", (int)Variant.Type.String }
            });
            break;
         case CommandType.ChangeWeaponVisibility:
            result.Add(new Dictionary()
            {
               { "name", $"Hide" },
               { "type", (int)Variant.Type.Bool }
            });
            break;
      }

      return result;
   }
}
#endif