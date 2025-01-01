using Godot;

/// <summary>
/// Determines the sprite to be used in a dialogue.
/// </summary>
public enum Emotion
{
   None,
   Neutral,
   Happy,
   Sad,
   Angry,
   Frightened,
   Annoyed,
   Thoughtful,
   Shocked,
   Curious,
   Tired,
   Dismayed,
   Smug,
   Avoidant,
   Exasperated,
   Concerned,
   Unhappy,
   Playful
}

/// <summary>
/// Represents a single item of a dialogue.
/// </summary>
[GlobalClass]
public partial class DialogueObject : Resource
{
   [Export]
   public string speaker;
   [Export]
   public CharacterType characterType;
   [Export]
   public Emotion emotion;
   [Export(PropertyHint.MultilineText)]
   public string content;
   [Export]
   public bool opensShop;

   /// <summary>
   /// Used to determine which substitute dialogue should be used (ex. whether this dialogue is for Member1 or Member2, depending on the 
   /// party member initiating the dialogue).
   /// <br></br>
   /// This field is only useful if substitute dialogues are available and this object is NOT being spoken by a party member. Otherwise, it should be set to None.
   /// </summary>
   [Export]
   public CharacterType Initiator { get; set; }
   /// <summary>
   /// A list of alternative dialogues that can substitute for the main one.
   /// <br></br>
   /// This is used when a dialogue can be initiated by different player characters (ex. Member1 may have their own unique dialogue for speaking with 
   /// Target, Member2 may have their own. Alternatively, Target may have different dialogues for Member1 and Member2)
   /// </summary>
   [Export]
   public DialogueObject[] SubstituteDialogues { get; set; } = new DialogueObject[0];
}
