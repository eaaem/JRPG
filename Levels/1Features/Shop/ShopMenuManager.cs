using Godot;
using System;

public partial class ShopMenuManager : Node
{
   [Export]
   private PackedScene itemButtonPrefab;

   private Control shopBack;
   [Export]
   private ManagerReferenceHolder managers;

   private ShopItem currentShopItem;
   public InventoryItem currentItemInTransaction;

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
      shopBack = GetParent<Control>();
      itemContainer = GetNode<VBoxContainer>("ScrollContainer/ItemContainer");

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
      managers.Controller.DisableMovement = true;
      managers.Controller.DisableCamera = true;
      bulkButton.Select(0);
      currentBulk = 1;
      IsBuying = true;

      buyButton.Disabled = true;
      buyButton.MouseFilter = Control.MouseFilterEnum.Ignore;
      sellButton.Disabled = false;
      sellButton.MouseFilter = Control.MouseFilterEnum.Stop;

      Input.MouseMode = Input.MouseModeEnum.Visible;
      shopBack.Visible = true;

      UpdateGoldLabel();
      ClearItemContainer();
      LoadBuyingItems();
   }

   void OnBuyButtonDown()
   {
      buyButton.Disabled = true;
      buyButton.MouseFilter = Control.MouseFilterEnum.Ignore;
      sellButton.Disabled = false;
      sellButton.MouseFilter = Control.MouseFilterEnum.Stop;
      ClearItemContainer();
      LoadBuyingItems();
      IsBuying = true;
   }

   void OnSellButtonDown()
   {
      buyButton.Disabled = false;
      buyButton.MouseFilter = Control.MouseFilterEnum.Stop;
      sellButton.Disabled = true;
      sellButton.MouseFilter = Control.MouseFilterEnum.Ignore;
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
         Panel itemButton = itemButtonPrefab.Instantiate<Panel>();

         itemButton.GetNode<Button>("Button").ButtonDown += managers.ButtonSoundManager.OnClick;
         itemButton.GetNode<Button>("Button").MouseEntered += managers.ButtonSoundManager.OnHoverOver;

         itemButton.GetNode<Button>("Button").Text = "   " + currentShopItem.selection[i].name + " (" + currentShopItem.selection[i].price * currentBulk + " g)";
         itemButton.GetNode<Label>("InStock").Visible = false;
         itemButton.GetNode<Button>("Button").TooltipText = currentShopItem.selection[i].description;

         itemButton.GetNode<ShopItemHolder>("ItemHolder").item = currentShopItem.selection[i];
         itemButton.GetNode<ShopItemHolder>("ItemHolder").quantity = currentBulk;

         if (currentShopItem.selection[i].itemType == ItemType.Special)
         {
            itemButton.GetNode<Button>("Button").AddThemeColorOverride("font_color", new Color(0.75f, 0.53f, 1f));
         }

         itemContainer.AddChild(itemButton);
      }
   }

   void LoadSellingItems()
   {
      for (int i = 0; i < managers.PartyManager.Items.Count; i++)
      {
         if (managers.PartyManager.Items[i].item.itemType != ItemType.Special)
         {
            Panel itemButton = itemButtonPrefab.Instantiate<Panel>();

            itemButton.GetNode<Button>("Button").ButtonDown += managers.ButtonSoundManager.OnClick;
            itemButton.GetNode<Button>("Button").MouseEntered += managers.ButtonSoundManager.OnHoverOver;
         
            itemButton.GetNode<Button>("Button").Text = "   " + managers.PartyManager.Items[i].item.name + " (" + (managers.PartyManager.Items[i].item.price * currentBulk) 
                                                            + " g)";
            itemButton.GetNode<Label>("InStock").Text = "x" + managers.PartyManager.Items[i].quantity;

            itemButton.GetNode<ShopItemHolder>("ItemHolder").item = managers.PartyManager.Items[i].item;
            itemButton.GetNode<ShopItemHolder>("ItemHolder").quantity = managers.PartyManager.Items[i].quantity;

            itemContainer.AddChild(itemButton);
         }
      }
   }

   void OnSelectBulk(int index)
   {
      currentBulk = int.Parse(bulkButton.GetItemText(index));

      for (int i = 0; i < itemContainer.GetChildCount(); i++)
      {
         Panel child = itemContainer.GetChild<Panel>(i);

         InventoryItem inventoryItem = IsBuying ? new InventoryItem(currentShopItem.selection[i], currentBulk) 
                                       : managers.PartyManager.Items[i];

         int priceToUse = inventoryItem.item.price;

         if (inventoryItem.item.itemType != ItemType.Special)
         {
            priceToUse = GetMaxQuantity(inventoryItem.quantity, currentBulk) * inventoryItem.item.price;
         }

         child.GetNode<Button>("Button").Text = "   " + inventoryItem.item.name + " (" + priceToUse + " g)";
      }
   }

   public void OnSelectItem(InventoryItem inventoryItem)
   {
      int quantity = 1;

      if (inventoryItem.item.itemType != ItemType.Special)
      {
         quantity = GetMaxQuantity(inventoryItem.quantity, currentBulk);
      }

      if (IsBuying && managers.PartyManager.Gold < quantity * inventoryItem.item.price)
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
         managers.PartyManager.Gold -= quantity * currentItemInTransaction.item.price;
         managers.PartyManager.AddItem(new InventoryItem(currentItemInTransaction.item, quantity));
         LoadBuyingItems();
      }
      else
      {
         managers.PartyManager.Gold += quantity * currentItemInTransaction.item.price;
         managers.PartyManager.RemoveItem(new InventoryItem(currentItemInTransaction.item, quantity));
         LoadSellingItems();
      }

      UpdateGoldLabel();

      EnableAll();
   }

   void UpdateGoldLabel()
   {
      goldLabel.Text = "[b]Gold: [/b]" + managers.PartyManager.Gold + " g";
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
         child.GetNode<Button>("Button").MouseFilter = Control.MouseFilterEnum.Ignore;
      }

      bulkButton.Disabled = true;
      buyButton.Disabled = true;
      sellButton.Disabled = true;
      exitButton.Disabled = true;

      bulkButton.MouseFilter = Control.MouseFilterEnum.Ignore;
      buyButton.MouseFilter = Control.MouseFilterEnum.Ignore;
      sellButton.MouseFilter = Control.MouseFilterEnum.Ignore;
      exitButton.MouseFilter = Control.MouseFilterEnum.Ignore;
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
         child.GetNode<Button>("Button").MouseFilter = Control.MouseFilterEnum.Stop;
      }

      bulkButton.Disabled = false;
      bulkButton.MouseFilter = Control.MouseFilterEnum.Stop;

      if (IsBuying)
      {
         buyButton.Disabled = true;
         buyButton.MouseFilter = Control.MouseFilterEnum.Ignore;
         sellButton.Disabled = false;
         sellButton.MouseFilter = Control.MouseFilterEnum.Stop;
      }
      else
      {
         buyButton.Disabled = false;
         buyButton.MouseFilter = Control.MouseFilterEnum.Stop;
         sellButton.Disabled = true;
         sellButton.MouseFilter = Control.MouseFilterEnum.Ignore;
      }
      
      exitButton.Disabled = false;
      exitButton.MouseFilter = Control.MouseFilterEnum.Stop;
   }

   void OnExitButtonDown()
   {
      shopBack.Visible = false;
      managers.DialogueManager.InitiateDialogue(managers.DialogueManager.CurrentInteraction, false, true);
   }
}
