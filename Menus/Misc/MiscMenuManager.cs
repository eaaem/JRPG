using Godot;
using System;

public partial class MiscMenuManager : Node
{
   private SaveManager saveManager;
   private CanvasGroup confirmationWindow;

   private MenuManager menuManager;

   public override void _Ready()
   {
      saveManager = GetNode<SaveManager>("/root/BaseNode/SaveManager");
      confirmationWindow = GetNode<CanvasGroup>("ConfirmationWindow");
      menuManager = GetNode<MenuManager>("../../MenuManager");
   }

   public void LoadMainMenu()
   {
      confirmationWindow.Visible = false;
   }

   void OnSaveQuitButtonDown()
   {
      saveManager.SaveGame(false, saveManager.currentSaveIndex);
      saveManager.ResetGameState();
      menuManager.menu.Visible = false;
   }

   void OnQuitNoSaveButtonDown()
   {
      menuManager.DisableTabs();
      confirmationWindow.Visible = true;
   }

   void OnConfirmButtonDown()
   {
      confirmationWindow.Visible = false;
      saveManager.ResetGameState();
      menuManager.menu.Visible = false;
      menuManager.EnableTabs();
   }

   void OnCancelButtonDown()
   {
      menuManager.EnableTabs();
      confirmationWindow.Visible = false;
   }
}
