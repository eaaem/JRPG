//#if TOOLS
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
   /// <br></br>
   /// <c>ActorName</c> (string) : the name of the actor
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
   /// Pauses all command execution for the given amount of seconds.
   /// <br></br><br></br>
   /// <c>WaitTime</c> (float) : the amount of time to pause
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
   /// <br></br>
   /// <c>Blend</c> (int) : the blend amount between the previous animation and this one (-1 is no blend, 1 is slowest possible blend)
   /// <br></br>
   /// <c>ExitBlend</c> (int) : the blend amount between this animation and the idle animation, after finishing (-1 is no blend, 1 is slowest possible blend)
   /// <br></br>
   /// <c>UseAnimationLength</c> (bool) : whether to wait the length of the animation before playing the default animation again or not
   /// <br></br>
   /// <c>WaitTime</c> (float) : if UseAnimationLength is false, the amount of time to wait before playing the default animation
   /// </summary>
   PlayAnimation,
   /// <summary>
   /// Makes the actor track the movement of another actor.
   /// <br></br><br></br>
   /// <c>ActorName</c> (string) : the name of the actor that tracks
   /// <br></br>
   /// <c>Target</c> (string) : the name of the actor to target
   /// </summary>
   Track,
   /// <summary>
   /// Stops the actor's tracking.
   /// <br></br><br></br>
   /// <c>ActorName</c> (string) : the name of the actor to stop tracking
   /// </summary>
   StopTrack,
   /// <summary>
   /// Fades to or from black.
   /// <br></br><br></br>
   /// <c>Fade</c> (bool) : whether to fade to or from black
   /// </summary>
   FadeBlack,
   /// <summary>
   /// Immediately places the camera at the given location.
   /// <br></br><br></br>
   /// <c>Destination</c> (Vector3) : the new location of the camera
   /// </summary>
   PlaceCamera,
   /// <summary>
   /// Immediately rotates the camera to the given angles.
   /// <br></br><br></br>
   /// <c>Destination</c> (Vector3) : the new rotation of the camera, in radians
   /// </summary>
   QuickRotateCamera,
   /// <summary>
   /// Calls a method on the LEVEL PROGRESSION script attached to the object at the given path. This works only for methods with no parameters.
   /// The script MUST inherit from the LevelProgression class.
   /// <br></br><br></br>
   /// <c>Method</c> (string) : the name of the method to call
   /// <br></br>
   /// <c>ObjectPath</c> (string) : the scene tree path that leads to the object with the script
   /// </summary>
   CallMethod,
   /// <summary>
   /// Turns one actor to look at another actor or an object in the scene. Functions identically to rotate, but with a certain target.
   /// <br></br><br></br>
   /// <c>ActorName</c> (string) : the actor to rotate
   /// <br></br>
   /// <c>Target</c> (string) : the name of the target (an actor or a node path from the root to the object)
   /// </summary>
   TurnToLookAt,
   /// <summary>
   /// Pauses the music track until ResumeMusic is executed.
   /// </summary>
   PauseMusic,
   /// <summary>
   /// Resumes the music track.
   /// </summary>
   ResumeMusic,
   /// <summary>
   /// Prematurely terminates the cutscene.
   /// </summary>
   EndCutscene
}

/// <summary>
/// Represents a single cutscene command.
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
   public string Target { get; set; }
   public Vector3 Destination { get; set; }
   public float YRotation { get; set; }
   public bool RotateToFace { get; set; }

   public bool UseAnimationLength { get; set; }
   public float WaitTime { get; set; }

   public bool Hide { get; set; }
   public bool MakeLocked { get; set; }
   public bool Fade { get; set; }

   public string AnimationName { get; set; }
   public string TargetAnimation { get; set; }

   public float Blend { get; set; }
   public float ExitBlend { get; set; }

   public string Method { get; set;}
   public string ObjectPath { get; set;}

   public override Array<Dictionary> _GetPropertyList()
   {
      Array<Dictionary> result = new Array<Dictionary>();

      result.Add(new Dictionary()
      {
         { "name", $"CommandType" },
         { "type", (int)Variant.Type.Int },
         { "hint", (int)PropertyHint.Enum },
         { "hint_string", "None,Move,Rotate,QuickRotate,ChangeDialogueVisibility,ChangeWeaponVisibility,ChangeDialogueLock,SpeakNext,SetIdleAnimation," + 
                          "SetWalkAnimation,Pause,Place,PlayAnimation,Track,StopTrack,FadeBlack,PlaceCamera,QuickRotateCamera,CallMethod,TurnToLookAt,PauseMusic," + 
                          "ResumeMusic,EndCutscene" }
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

            result.Add(new Dictionary()
            {
               { "name", $"RotateToFace" },
               { "type", (int)Variant.Type.Bool }
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
               { "name", $"WaitTime" },
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

            result.Add(new Dictionary()
            {
               { "name", $"Blend" },
               { "type", (int)Variant.Type.Float }
            });

            result.Add(new Dictionary()
            {
               { "name", $"ExitBlend" },
               { "type", (int)Variant.Type.Float }
            });

            result.Add(new Dictionary()
            {
               { "name", $"UseAnimationLength" },
               { "type", (int)Variant.Type.Bool }
            });

            if (!UseAnimationLength)
            {
               result.Add(new Dictionary()
               {
                  { "name", $"WaitTime" },
                  { "type", (int)Variant.Type.Float }
               });
            }

            break;
         case CommandType.ChangeWeaponVisibility:
            result.Add(new Dictionary()
            {
               { "name", $"Hide" },
               { "type", (int)Variant.Type.Bool }
            });

            result.Add(new Dictionary()
            {
               { "name", $"ActorName" },
               { "type", (int)Variant.Type.String }
            });

            break;
         case CommandType.Track:
            result.Add(new Dictionary()
            {
               { "name", $"ActorName" },
               { "type", (int)Variant.Type.String }
            });

            result.Add(new Dictionary()
            {
               { "name", $"Target" },
               { "type", (int)Variant.Type.String }
            });

            break;
         case CommandType.StopTrack:
            result.Add(new Dictionary()
            {
               { "name", $"ActorName" },
               { "type", (int)Variant.Type.String }
            });
            
            break;
         case CommandType.FadeBlack:
            result.Add(new Dictionary()
            {
               { "name", $"Fade" },
               { "type", (int)Variant.Type.Bool }
            });
            
            break;
         case CommandType.PlaceCamera:
            result.Add(new Dictionary()
            {
               { "name", $"Destination" },
               { "type", (int)Variant.Type.Vector3 }
            });
            
            break;
         case CommandType.QuickRotateCamera:
            result.Add(new Dictionary()
            {
               { "name", $"Destination" },
               { "type", (int)Variant.Type.Vector3 }
            });
            
            break;
         case CommandType.CallMethod:
            result.Add(new Dictionary()
            {
               { "name", $"Method" },
               { "type", (int)Variant.Type.String }
            });

            result.Add(new Dictionary()
            {
               { "name", $"ObjectPath" },
               { "type", (int)Variant.Type.String }
            });
            
            break;
         case CommandType.TurnToLookAt:
            result.Add(new Dictionary()
            {
               { "name", $"ActorName" },
               { "type", (int)Variant.Type.String }
            });

            result.Add(new Dictionary()
            {
               { "name", $"Target" },
               { "type", (int)Variant.Type.String }
            });

            break;
      }

      return result;
   }
}
//#endif