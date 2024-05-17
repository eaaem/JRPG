using Godot;
using System;

public partial class ShopMenuManager : Node
{
   [Export]
   private PackedScene itemButtonPrefab;

   private CanvasGroup shopCanvasGroup;
   private CharacterController controller;
   private PartyManager partyManager;
   private DialogueManager dialogueManager;

   private ShopItem currentShopItem;
   private InventoryItem currentItemInTransaction;

   private VBoxContainer itemContainer;
   private OptionButton bulkButton;
   private Button buyButton;
   private Button sellButton;
   private Button exitButton;
   private RichTextLabel goldLabel;

   private Panel notificationBackground;
   private RichTextLabel notificationText;
   private Button notificationYesButton;
   private Button notificationNoButton;

   private int currentBulk = 1;

   public bool IsBuying { get; set; }

   public override void _Ready()
   {
      shopCanvasGroup = GetParent<CanvasGroup>();
      itemContainer = GetNode<VBoxContainer>("ScrollContainer/ItemContainer");
      partyManager = GetNode<PartyManager>("/root/BaseNode/PartyManagerObj");
      dialogueManager = GetNode<DialogueManager>("/root/BaseNode/UI/DialogueScreen/Back");

      controller = GetNode<CharacterController>("/root/BaseNode/PartyMembers/Member1");
      bulkButton = GetNode<OptionButton>("Buttons/Bulk");
      buyButton = GetNode<Button>("Buttons/Buy");
      sellButton = GetNode<Button>("Buttons/Sell");
      exitButton = GetNode<Button>("Buttons/Exit");
      goldLabel = GetNode<RichTextLabel>("Buttons/GoldDisplay");

      notificationBackground = GetNode<Panel>("Notification");
      notificationText = notificationBackground.GetNode<RichTextLabel>("Body");
      notificationYesButton = notificationBackground.GetNode<Button>("YesButton");
      notificationNoButton = notificationBackground.GetNode<Button>("NoButton");
   }

   public void OpenShop(ShopItem shopItem)
   {
      currentShopItem = shopItem;
      controller.DisableMovement = true;
      bulkButton.Select(0);
      currentBulk = 1;
      IsBuying = true;

      buyButton.Disabled = true;
      sellButton.Disabled = false;

      Input.MouseMode = Input.MouseModeEnum.Visible;
      shopCanvasGroup.Visible = true;

      UpdateGoldLabel();
      ClearItemContainer();
      LoadBuyingItems();
   }

   void OnBuyButtonDown()
   {
      buyButton.Disabled = true;
      sellButton.Disabled = false;
      ClearItemContainer();
      LoadBuyingItems();
      IsBuying = true;
   }

   void OnSellButtonDown()
   {
      buyButton.Disabled = false;
      sellButton.Disabled = true;
      ClearItemContainer();
      LoadSellingItems();
      IsBuying = false;
   }

   void ClearItemContainer()
   {
      foreach (Node child in itemContainer.GetChildren())
      {
         itemContainer.RemoveChild(child);
         child.QueueFree();
      }
   }

   void LoadBuyingItems()
   {
      for (int i = 0; i < currentShopItem.selection.Length; i++)
      {
         //if (currentShopItem.actualSelection[i].inStock > 0)
         //{
            Panel itemButton = itemButtonPrefab.Instantiate<Panel>();

            //int priceToUse = GetMaxQuantity(currentShopItem.actualSelection[i].inStock, currentBulk) * currentShopItem.actualSelection[i].item.price;
         
            itemButton.GetNode<Button>("Button").Text = currentShopItem.selection[i].name + " (" + currentShopItem.selection[i].price * currentBulk + " g)";
            itemButton.GetNode<Label>("InStock").Visible = false;
            itemButton.GetNode<Button>("Button").TooltipText = currentShopItem.selection[i].description;

            itemButton.GetNode<ShopItemHolder>("ItemHolder").item = currentShopItem.selection[i];
            itemButton.GetNode<ShopItemHolder>("ItemHolder").quantity = currentBulk;

            itemContainer.AddChild(itemButton);
         //}
      }
   }

   void LoadSellingItems()
   {
      for (int i = 0; i < partyManager.Items.Count; i++)
      {
         Panel itemButton = itemButtonPrefab.Instantiate<Panel>();
         
         itemButton.GetNode<Button>("Button").Text = partyManager.Items[i].item.name + " (" + (partyManager.Items[i].item.price * currentBulk) + " g)";
         itemButton.GetNode<Label>("InStock").Text = "x" + partyManager.Items[i].quantity;

         itemButton.GetNode<ShopItemHolder>("ItemHolder").item = partyManager.Items[i].item;
         itemButton.GetNode<ShopItemHolder>("ItemHolder").quantity = partyManager.Items[i].quantity;

         itemContainer.AddChild(itemButton);
      }
   }

   void OnSelectBulk(int index)
   {
      currentBulk = int.Parse(bulkButton.GetItemText(index));

      for (int i = 0; i < itemContainer.GetChildCount(); i++)
      {
         Panel child = itemContainer.GetChild<Panel>(i);

         InventoryItem inventoryItem = IsBuying ? new InventoryItem(currentShopItem.selection[i], currentBulk) 
                                       : partyManager.Items[i];

         int priceToUse = GetMaxQuantity(inventoryItem.quantity, currentBulk) * inventoryItem.item.price;

         child.GetNode<Button>("Button").Text = inventoryItem.item.name + " (" + priceToUse + " g)";
      }
   }

   public void OnSelectItem(InventoryItem inventoryItem)
   {
      int quantity = GetMaxQuantity(inventoryItem.quantity, currentBulk);

      if (IsBuying && partyManager.Gold < quantity * inventoryItem.item.price)
      {
         return;
      }

      DisableAll();

      notificationText.Text = " [center]Are you sure you want to " + (IsBuying ? "buy" : "sell") + " " + quantity + " " + inventoryItem.item.name
                            + (currentBulk > 1 ? "s" : "") + " for " + quantity * inventoryItem.item.price + " g?[/center]";
      
      currentItemInTransaction = inventoryItem;
      notificationBackground.Visible = true;
   }

   void OnYesButtonDown()
   {
      notificationBackground.Visible = false;
      int quantity = GetMaxQuantity(currentItemInTransaction.quantity, currentBulk);
      ClearItemContainer();

      if (IsBuying)
      {
         partyManager.Gold -= quantity * currentItemInTransaction.item.price;
         partyManager.AddItem(new InventoryItem(currentItemInTransaction.item, quantity));
         //currentShopItem.RemoveItemFromSelection(currentItemInTransaction.item, quantity);
         LoadBuyingItems();
      }
      else
      {
         partyManager.Gold += quantity * currentItemInTransaction.item.price;
         partyManager.RemoveItem(new InventoryItem(currentItemInTransaction.item, quantity));
         //currentShopItem.AddItemToSelection(currentItemInTransaction.item, quantity);
         LoadSellingItems();
      }

      UpdateGoldLabel();

      EnableAll();
   }

   void UpdateGoldLabel()
   {
      goldLabel.Text = "[b]Gold: [/b]" + partyManager.Gold + " g";
   }

   void OnNoButtonDown()
   {
      notificationBackground.Visible = false;
      EnableAll();
   }

   void DisableAll()
   {
      foreach (Panel child in itemContainer.GetChildren())
      {
         child.GetNode<Button>("Button").Disabled = true;
      }

      bulkButton.Disabled = true;
      buyButton.Disabled = true;
      sellButton.Disabled = true;
      exitButton.Disabled = true;
   }

   int GetMaxQuantity(int currentQuantity, int desiredQuantity)
   {
      return currentQuantity >= desiredQuantity ? desiredQuantity : currentQuantity;
   }

   void EnableAll()
   {
      foreach (Panel child in itemContainer.GetChildren())
      {
         child.GetNode<Button>("Button").Disabled = false;
      }

      bulkButton.Disabled = false;

      if (IsBuying)
      {
         buyButton.Disabled = true;
         sellButton.Disabled = false;
      }
      else
      {
         buyButton.Disabled = false;
         sellButton.Disabled = true;
      }
      
      exitButton.Disabled = false;
   }

   void OnExitButtonDown()
   {
      shopCanvasGroup.Visible = false;
      dialogueManager.InitiateDialogue(dialogueManager.CurrentInteraction, false, true);
   }
}
