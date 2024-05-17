using Godot;
using System;

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
   Avoidant
}

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
}
