using Godot;

/// <summary>
/// Keeps all managers in a tidy place. Reference this to gain access to managers instead of directly accessing them.
/// </summary>
public partial class ManagerReferenceHolder : Node
{
   [Export]
	public PartyManager PartyManager { get; set; }
   [Export]
   public CharacterController Controller { get; set; }
   [Export]
   public LevelManager LevelManager { get; set; }
   [Export]
   public SaveManager SaveManager { get; set; }
   [Export]
   public MenuManager MenuManager { get; set; }
   [Export]
   public ItemPickupManager ItemPickupManager { get; set; }
   [Export]
   public DialogueManager DialogueManager { get; set; }
   [Export]
   public CutsceneManager CutsceneManager { get; set; }
   [Export]
   public ShopMenuManager ShopMenuManager { get; set; }
   [Export]
   public PartyMenuManager PartyMenuManager { get; set;}
}
