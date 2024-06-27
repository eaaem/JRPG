using Godot;
using System;

public partial class MiscMenuManager : Node
{
   [Export]
   private ManagerReferenceHolder managers;

   private CanvasGroup confirmationWindow;

   public override void _Ready()
   {
      confirmationWindow = GetNode<CanvasGroup>("ConfirmationWindow");
   }

   public void LoadMainMenu()
   {
      confirmationWindow.Visible = false;
   }

   void OnSaveQuitButtonDown()
   {
      managers.SaveManager.SaveGame(false, managers.SaveManager.currentSaveIndex);
      managers.LevelManager.ResetGameState();
      managers.MenuManager.menu.Visible = false;
   }

   void OnQuitNoSaveButtonDown()
   {
      managers.MenuManager.DisableTabs();
      confirmationWindow.Visible = true;
   }

   void OnConfirmButtonDown()
   {
      confirmationWindow.Visible = false;
      managers.LevelManager.ResetGameState();
      managers.MenuManager.menu.Visible = false;
      managers.MenuManager.EnableTabs();
   }

   void OnCancelButtonDown()
   {
      managers.MenuManager.EnableTabs();
      confirmationWindow.Visible = false;
   }
}
