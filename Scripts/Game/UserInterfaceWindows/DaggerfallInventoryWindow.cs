﻿// Project:         Daggerfall Tools For Unity
// Copyright:       Copyright (C) 2009-2015 Daggerfall Workshop
// Web Site:        http://www.dfworkshop.net
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/Interkarma/daggerfall-unity
// Original Author: Gavin Clayton (interkarma@dfworkshop.net)
// Contributors:    
// 
// Notes:
//

using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DaggerfallConnect.Utility;
using DaggerfallConnect.FallExe;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Utility;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.Player;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Items;

namespace DaggerfallWorkshop.Game.UserInterfaceWindows
{
    /// <summary>
    /// Implements inventory window.
    /// </summary>
    public class DaggerfallInventoryWindow : DaggerfallPopupWindow
    {
        #region UI Rects

        Rect weaponsAndArmorRect = new Rect(0, 0, 92, 10);
        Rect magicItemsRect = new Rect(93, 0, 69, 10);
        Rect clothingAndMiscRect = new Rect(163, 0, 91, 10);
        Rect ingredientsRect = new Rect(255, 0, 65, 10);

        Rect localItemsUpButtonRect = new Rect(163, 48, 9, 16);
        Rect localItemsDownButtonRect = new Rect(163, 184, 9, 16);
        Rect remoteItemsUpButtonRect = new Rect(261, 48, 9, 16);
        Rect remoteItemsDownButtonRect = new Rect(261, 184, 9, 16);

        Rect localItemsListPanelRect = new Rect(172, 48, 50, 152);
        Rect remoteItemsListPanelRect = new Rect(270, 48, 50, 152);
        Rect[] itemsButtonRects = new Rect[]
        {
            new Rect(0, 0, 50, 38),
            new Rect(0, 38, 50, 38),
            new Rect(0, 76, 50, 38),
            new Rect(0, 114, 50, 38)
        };

        Rect upArrowRect = new Rect(0, 0, 9, 16);
        Rect downArrowRect = new Rect(0, 136, 9, 16);

        Rect wagonButtonRect = new Rect(226, 14, 31, 14);
        Rect infoButtonRect = new Rect(226, 36, 31, 14);
        Rect equipButtonRect = new Rect(226, 58, 31, 14);
        Rect removeButtonRect = new Rect(226, 80, 31, 14);
        Rect useButtonRect = new Rect(226, 103, 31, 14);
        Rect goldButtonRect = new Rect(226, 126, 31, 14);

        Rect localTargetIconRect = new Rect(164, 11, 57, 36);
        Rect remoteTargetIconRect = new Rect(262, 11, 57, 36);

        #endregion

        #region UI Controls

        Button weaponsAndArmorButton;
        Button magicItemsButton;
        Button clothingAndMiscButton;
        Button ingredientsButton;

        Button wagonButton;
        Button infoButton;
        Button equipButton;
        Button removeButton;
        Button useButton;
        Button goldButton;

        Button localItemsUpButton;
        Button localItemsDownButton;
        Button remoteItemsUpButton;
        Button remoteItemsDownButton;
        VerticalScrollBar localItemsScrollBar;
        VerticalScrollBar remoteItemsScrollBar;

        Button[] localItemsButtons = new Button[listDisplayUnits];
        Panel[] localItemsIconPanels = new Panel[listDisplayUnits];
        TextLabel[] localItemsStackLabels = new TextLabel[listDisplayUnits];
        Button[] remoteItemsButtons = new Button[listDisplayUnits];
        Panel[] remoteItemsIconPanels = new Panel[listDisplayUnits];
        TextLabel[] remoteItemsStackLabels = new TextLabel[listDisplayUnits];

        Button[] accessoryButtons = new Button[accessoryCount];
        Panel[] accessoryIconPanels = new Panel[accessoryCount];

        PaperDoll paperDoll = new PaperDoll();
        Panel localTargetIconPanel;
        Panel remoteTargetIconPanel;

        #endregion

        #region UI Textures

        Texture2D baseTexture;
        Texture2D goldTexture;

        Texture2D weaponsAndArmorNotSelected;
        Texture2D magicItemsNotSelected;
        Texture2D clothingAndMiscNotSelected;
        Texture2D ingredientsNotSelected;
        Texture2D weaponsAndArmorSelected;
        Texture2D magicItemsSelected;
        Texture2D clothingAndMiscSelected;
        Texture2D ingredientsSelected;

        Texture2D wagonNotSelected;
        Texture2D infoNotSelected;
        Texture2D equipNotSelected;
        Texture2D removeNotSelected;
        Texture2D useNotSelected;
        Texture2D wagonSelected;
        Texture2D infoSelected;
        Texture2D equipSelected;
        Texture2D removeSelected;
        Texture2D useSelected;

        Texture2D redUpArrow;
        Texture2D greenUpArrow;
        Texture2D redDownArrow;
        Texture2D greenDownArrow;

        #endregion

        #region Fields

        const string baseTextureName = "INVE00I0.IMG";
        const string goldTextureName = "INVE01I0.IMG";
        const string greenArrowsTextureName = "INVE06I0.IMG";           // Green up/down arrows when more items available
        const string redArrowsTextureName = "INVE07I0.IMG";             // Red up/down arrows when no more items available
        const int listDisplayUnits = 4;                                 // Number of items displayed in scrolling areas
        const int accessoryCount = 12;                                  // Number of accessory slots
        const int itemButtonMarginSize = 2;                             // Margin of item buttons
        const int accessoryButtonMarginSize = 1;                        // Margin of accessory buttons

        PlayerEntity playerEntity;

        TabPages selectedTabPage = TabPages.WeaponsAndArmor;
        ActionModes selectedActionMode = ActionModes.Equip;
        ItemTargets remoteTarget = ItemTargets.None;

        EntityItems localItems = null;
        EntityItems remoteItems = null;
        List<DaggerfallUnityItem> localItemsFiltered = new List<DaggerfallUnityItem>();
        List<DaggerfallUnityItem> remoteItemsFiltered = new List<DaggerfallUnityItem>();

        int lastMouseOverPaperDollEquipIndex = -1;

        #endregion

        #region Enums

        enum TabPages
        {
            WeaponsAndArmor,
            MagicItems,
            ClothingAndMisc,
            Ingredients,
        }

        enum ItemTargets
        {
            None,
            Player,
            Wagon,
        }

        enum ActionModes
        {
            Info,
            Equip,
            Remove,
            Use,
        }

        #endregion

        #region Properties

        public PlayerEntity PlayerEntity
        {
            get { return (playerEntity != null) ? playerEntity : playerEntity = GameManager.Instance.PlayerEntity; }
        }

        #endregion

        #region Constructors

        public DaggerfallInventoryWindow(IUserInterfaceManager uiManager)
            : base(uiManager)
        {
        }

        #endregion

        #region Setup Methods

        protected override void Setup()
        {
            // Load all the textures used by inventory system
            LoadTextures();

            // Always dim background
            ParentPanel.BackgroundColor = ScreenDimColor;

            // Setup native panel background
            NativePanel.BackgroundTexture = baseTexture;

            // Character portrait
            NativePanel.Components.Add(paperDoll);
            paperDoll.Position = new Vector2(49, 13);
            paperDoll.OnMouseMove += PaperDoll_OnMouseMove;
            paperDoll.ToolTip = defaultToolTip;
            paperDoll.Refresh();

            // Setup UI
            SetupTabPageButtons();
            SetupActionButtons();
            SetupScrollBars();
            SetupScrollButtons();
            SetupLocalItemsElements();
            SetupRemoteItemsElements();
            SetupAccessoryElements();

            // Setup local and remote target icon panels
            localTargetIconPanel = DaggerfallUI.AddPanel(localTargetIconRect, NativePanel);
            remoteTargetIconPanel = DaggerfallUI.AddPanel(remoteTargetIconRect, NativePanel);

            // Set initial state
            SelectTabPage(TabPages.WeaponsAndArmor);
            SelectActionMode(ActionModes.Equip);
            SetLocalTarget(ItemTargets.Player);
            SetRemoteTarget(ItemTargets.Wagon);

            // Update item lists
            FilterLocalItems();
            FilterRemoteItems();
            UpdateLocalItemsDisplay();
            UpdateRemoteItemsDisplay();
            UpdateAccessoryItemsDisplay();
        }

        void SetupTabPageButtons()
        {
            weaponsAndArmorButton = DaggerfallUI.AddButton(weaponsAndArmorRect, NativePanel);
            weaponsAndArmorButton.OnMouseClick += WeaponsAndArmor_OnMouseClick;

            magicItemsButton = DaggerfallUI.AddButton(magicItemsRect, NativePanel);
            magicItemsButton.OnMouseClick += MagicItems_OnMouseClick;

            clothingAndMiscButton = DaggerfallUI.AddButton(clothingAndMiscRect, NativePanel);
            clothingAndMiscButton.OnMouseClick += ClothingAndMisc_OnMouseClick;

            ingredientsButton = DaggerfallUI.AddButton(ingredientsRect, NativePanel);
            ingredientsButton.OnMouseClick += Ingredients_OnMouseClick;
        }

        void SetupActionButtons()
        {
            wagonButton = DaggerfallUI.AddButton(wagonButtonRect, NativePanel);
            wagonButton.OnMouseClick += WagonButton_OnMouseClick;

            infoButton = DaggerfallUI.AddButton(infoButtonRect, NativePanel);
            infoButton.OnMouseClick += InfoButton_OnMouseClick;

            equipButton = DaggerfallUI.AddButton(equipButtonRect, NativePanel);
            equipButton.OnMouseClick += EquipButton_OnMouseClick;

            removeButton = DaggerfallUI.AddButton(removeButtonRect, NativePanel);
            removeButton.OnMouseClick += RemoveButton_OnMouseClick;

            useButton = DaggerfallUI.AddButton(useButtonRect, NativePanel);
            useButton.OnMouseClick += UseButton_OnMouseClick;

            goldButton = DaggerfallUI.AddButton(goldButtonRect, NativePanel);
            goldButton.BackgroundColor = new Color(1, 0, 0, 0.5f);
        }

        void SetupScrollBars()
        {
            // Local items list scroll bar (e.g. items in character inventory)
            localItemsScrollBar = new VerticalScrollBar();
            localItemsScrollBar.Position = new Vector2(164, 66);
            localItemsScrollBar.Size = new Vector2(6, 117);
            localItemsScrollBar.DisplayUnits = listDisplayUnits;
            localItemsScrollBar.OnScroll += LocalItemsScrollBar_OnScroll;
            NativePanel.Components.Add(localItemsScrollBar);

            // Remote items list scroll bar (e.g. wagon, shop, loot pile, etc.)
            remoteItemsScrollBar = new VerticalScrollBar();
            remoteItemsScrollBar.Position = new Vector2(262, 66);
            remoteItemsScrollBar.Size = new Vector2(6, 117);
            remoteItemsScrollBar.DisplayUnits = listDisplayUnits;
            remoteItemsScrollBar.OnScroll += RemoteItemsScrollBar_OnScroll;
            NativePanel.Components.Add(remoteItemsScrollBar);
        }

        void SetupScrollButtons()
        {
            localItemsUpButton = DaggerfallUI.AddButton(localItemsUpButtonRect, NativePanel);
            localItemsUpButton.BackgroundTexture = redUpArrow;
            localItemsUpButton.OnMouseClick += LocalItemsUpButton_OnMouseClick;

            localItemsDownButton = DaggerfallUI.AddButton(localItemsDownButtonRect, NativePanel);
            localItemsDownButton.BackgroundTexture = redDownArrow;
            localItemsDownButton.OnMouseClick += LocalItemsDownButton_OnMouseClick;

            remoteItemsUpButton = DaggerfallUI.AddButton(remoteItemsUpButtonRect, NativePanel);
            remoteItemsUpButton.BackgroundTexture = redUpArrow;
            remoteItemsUpButton.OnMouseClick += RemoteItemsUpButton_OnMouseClick;

            remoteItemsDownButton = DaggerfallUI.AddButton(remoteItemsDownButtonRect, NativePanel);
            remoteItemsDownButton.BackgroundTexture = redDownArrow;
            remoteItemsDownButton.OnMouseClick += RemoteItemsDownButton_OnMouseClick;
        }

        void SetupLocalItemsElements()
        {
            // List panel for scrolling behaviour
            Panel localItemsListPanel = DaggerfallUI.AddPanel(localItemsListPanelRect, NativePanel);
            localItemsListPanel.OnMouseScrollUp += MyItemsListPanel_OnMouseScrollUp;
            localItemsListPanel.OnMouseScrollDown += MyItemsListPanel_OnMouseScrollDown;

            // Setup buttons
            for (int i = 0; i < listDisplayUnits; i++)
            {
                // Button
                localItemsButtons[i] = DaggerfallUI.AddButton(itemsButtonRects[i], localItemsListPanel);
                localItemsButtons[i].SetMargins(Margins.All, itemButtonMarginSize);
                localItemsButtons[i].ToolTip = defaultToolTip;
                localItemsButtons[i].Tag = i;
                localItemsButtons[i].OnMouseClick += LocalItemsButton_OnMouseClick;

                // Icon image panel
                localItemsIconPanels[i] = DaggerfallUI.AddPanel(localItemsButtons[i], AutoSizeModes.ScaleToFit);
                localItemsIconPanels[i].HorizontalAlignment = HorizontalAlignment.Center;
                localItemsIconPanels[i].VerticalAlignment = VerticalAlignment.Middle;
                localItemsIconPanels[i].MaxAutoScale = 1f;

                // Stack labels
                localItemsStackLabels[i] = DaggerfallUI.AddTextLabel(DaggerfallUI.Instance.Font4, Vector2.zero, string.Empty, localItemsButtons[i]);
                localItemsStackLabels[i].HorizontalAlignment = HorizontalAlignment.Right;
                localItemsStackLabels[i].VerticalAlignment = VerticalAlignment.Bottom;
                localItemsStackLabels[i].ShadowPosition = Vector2.zero;
                localItemsStackLabels[i].TextColor = DaggerfallUI.DaggerfallUnityDefaultToolTipTextColor;
            }
        }

        void SetupRemoteItemsElements()
        {
            // List panel for scrolling behaviour
            Panel remoteItemsListPanel = DaggerfallUI.AddPanel(remoteItemsListPanelRect, NativePanel);
            remoteItemsListPanel.OnMouseScrollUp += RemoteItemsListPanel_OnMouseScrollUp;
            remoteItemsListPanel.OnMouseScrollDown += RemoteItemsListPanel_OnMouseScrollDown;

            // Setup buttons
            for (int i = 0; i < listDisplayUnits; i++)
            {
                // Button
                remoteItemsButtons[i] = DaggerfallUI.AddButton(itemsButtonRects[i], remoteItemsListPanel);
                remoteItemsButtons[i].SetMargins(Margins.All, itemButtonMarginSize);
                remoteItemsButtons[i].ToolTip = defaultToolTip;
                remoteItemsButtons[i].Tag = i;
                remoteItemsButtons[i].OnMouseClick += RemoteItemsButton_OnMouseClick;

                // Icon image panel
                remoteItemsIconPanels[i] = DaggerfallUI.AddPanel(remoteItemsButtons[i], AutoSizeModes.ScaleToFit);
                remoteItemsIconPanels[i].HorizontalAlignment = HorizontalAlignment.Center;
                remoteItemsIconPanels[i].VerticalAlignment = VerticalAlignment.Middle;
                remoteItemsIconPanels[i].MaxAutoScale = 1f;

                // Stack labels
                remoteItemsStackLabels[i] = DaggerfallUI.AddTextLabel(DaggerfallUI.Instance.Font4, Vector2.zero, string.Empty, remoteItemsButtons[i]);
                remoteItemsStackLabels[i].HorizontalAlignment = HorizontalAlignment.Right;
                remoteItemsStackLabels[i].VerticalAlignment = VerticalAlignment.Bottom;
                remoteItemsStackLabels[i].ShadowPosition = Vector2.zero;
                remoteItemsStackLabels[i].TextColor = DaggerfallUI.DaggerfallUnityDefaultToolTipTextColor;
            }
        }

        void SetupAccessoryElements()
        {
            // Starting layout
            Vector2 col0Pos = new Vector2(1, 11);
            Vector2 col1Pos = new Vector2(24, 11);
            Vector2 buttonSize = new Vector2(21, 20);
            int rowOffset = 31;
            bool col0 = true;

            // Follow same order as equip slots
            int minSlot = (int)EquipSlots.Amulet0;
            int maxSlot = (int)EquipSlots.Crystal1;
            for (int i = minSlot; i <= maxSlot; i++)
            {
                // Current button rect
                Rect rect;
                if (col0)
                    rect = new Rect(col0Pos.x, col0Pos.y, buttonSize.x, buttonSize.y);
                else
                    rect = new Rect(col1Pos.x, col1Pos.y, buttonSize.x, buttonSize.y);

                // Create item button
                Button button = DaggerfallUI.AddButton(rect, NativePanel);
                button.SetMargins(Margins.All, accessoryButtonMarginSize);
                button.ToolTip = defaultToolTip;
                button.Tag = i;
                button.OnMouseClick += Accessory_OnMouseClick;
                accessoryButtons[i] = button;

                // Create icon panel
                Panel panel = new Panel();
                panel.AutoSize = AutoSizeModes.ScaleToFit;
                panel.MaxAutoScale = 1f;
                panel.HorizontalAlignment = HorizontalAlignment.Center;
                panel.VerticalAlignment = VerticalAlignment.Middle;
                button.Components.Add(panel);
                accessoryIconPanels[i] = panel;

                // Move to next column, then drop down a row at end of second column
                if (col0)
                {
                    col0 = !col0;
                }
                else
                {
                    col0 = !col0;
                    col0Pos.y += rowOffset;
                    col1Pos.y += rowOffset;
                }
            }
        }

        public override void OnPush()
        {
            Refresh();
        }

        public override void OnPop()
        {
            // Update weapons in hands
            GameManager.Instance.WeaponManager.UpdateWeapons(playerEntity.ItemEquipTable);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Refresh character portrait and inventory.
        /// Called every time inventory is pushed to top of stack.
        /// </summary>
        public void Refresh()
        {
            playerEntity = GameManager.Instance.PlayerEntity;
            if (IsSetup)
            {
                FilterLocalItems();
                FilterRemoteItems();
                UpdateLocalItemsDisplay();
                UpdateRemoteItemsDisplay();
                UpdateAccessoryItemsDisplay();
                paperDoll.Refresh();
            }
        }

        #endregion

        #region Helper Methods

        // Clears all local list display elements
        void ClearLocalItemsElements()
        {
            for (int i = 0; i < listDisplayUnits; i++)
            {
                localItemsStackLabels[i].Text = string.Empty;
                localItemsButtons[i].ToolTipText = string.Empty;
                localItemsIconPanels[i].BackgroundTexture = null;
                localItemsUpButton.BackgroundTexture = redUpArrow;
                localItemsDownButton.BackgroundTexture = redDownArrow;
            }
        }

        // Clears all remote list display elements
        void ClearRemoteItemsElements()
        {
            for (int i = 0; i < listDisplayUnits; i++)
            {
                remoteItemsStackLabels[i].Text = string.Empty;
                remoteItemsButtons[i].ToolTipText = string.Empty;
                remoteItemsIconPanels[i].BackgroundTexture = null;
                remoteItemsUpButton.BackgroundTexture = redUpArrow;
                remoteItemsDownButton.BackgroundTexture = redDownArrow;
            }
        }

        // Updates red/green state of scroller buttons
        void UpdateListScrollerButtons(int index, int count, Button upButton, Button downButton)
        {
            // Update up button
            if (index > 0)
                upButton.BackgroundTexture = greenUpArrow;
            else
                upButton.BackgroundTexture = redUpArrow;

            // Update down button
            if (index < (count - listDisplayUnits))
                downButton.BackgroundTexture = greenDownArrow;
            else
                downButton.BackgroundTexture = redDownArrow;

            // No items above or below
            if (count <= listDisplayUnits)
            {
                localItemsUpButton.BackgroundTexture = redUpArrow;
                localItemsDownButton.BackgroundTexture = redDownArrow;
            }
        }

        // Gets inventory image
        ImageData GetInventoryImage(DaggerfallUnityItem item)
        {
            if (item.TemplateIndex == (int)Transportation.Small_cart)
            {
                // Handle small cart - the template image for this is not correct
                // Correct image actually in CIF files
                return DaggerfallUnity.ItemHelper.GetContainerImage(ContainerTypes.Wagon);
            }
            else
            {
                // Get inventory image
                return DaggerfallUnity.ItemHelper.GetItemImage(item, true);
            }
        }

        void SelectTabPage(TabPages tabPage)
        {
            // Select new tab page
            selectedTabPage = tabPage;

            // Reset scrollbar
            localItemsScrollBar.Reset(listDisplayUnits);

            // Clear all button selections
            weaponsAndArmorButton.BackgroundTexture = weaponsAndArmorNotSelected;
            magicItemsButton.BackgroundTexture = magicItemsNotSelected;
            clothingAndMiscButton.BackgroundTexture = clothingAndMiscNotSelected;
            ingredientsButton.BackgroundTexture = ingredientsNotSelected;

            // Set new button selection texture background
            switch (tabPage)
            {
                case TabPages.WeaponsAndArmor:
                    weaponsAndArmorButton.BackgroundTexture = weaponsAndArmorSelected;
                    break;
                case TabPages.MagicItems:
                    magicItemsButton.BackgroundTexture = magicItemsSelected;
                    break;
                case TabPages.ClothingAndMisc:
                    clothingAndMiscButton.BackgroundTexture = clothingAndMiscSelected;
                    break;
                case TabPages.Ingredients:
                    ingredientsButton.BackgroundTexture = ingredientsSelected;
                    break;
            }

            // Update filtered list
            FilterLocalItems();
            UpdateLocalItemsDisplay();
        }

        void SelectActionMode(ActionModes mode)
        {
            selectedActionMode = mode;

            // Clear all button selections
            infoButton.BackgroundTexture = infoNotSelected;
            equipButton.BackgroundTexture = equipNotSelected;
            removeButton.BackgroundTexture = removeNotSelected;
            useButton.BackgroundTexture = useNotSelected;

            // Set button selected texture
            switch(mode)
            {
                case ActionModes.Info:
                    infoButton.BackgroundTexture = infoSelected;
                    break;
                case ActionModes.Equip:
                    equipButton.BackgroundTexture = equipSelected;
                    break;
                case ActionModes.Remove:
                    removeButton.BackgroundTexture = removeSelected;
                    break;
                case ActionModes.Use:
                    useButton.BackgroundTexture = useSelected;
                    break;
            }
        }

        void SetLocalTarget(ItemTargets target)
        {
            // Only player supported for now
            if (target == ItemTargets.Player)
            {
                localItems = playerEntity.Items;
            }
        }

        void SetRemoteTarget(ItemTargets target)
        {
            remoteTarget = target;

            // Clear selections
            wagonButton.BackgroundTexture = wagonNotSelected;

            // Only wagon supported for now
            if (target == ItemTargets.Wagon)
            {
                // Show wagon icon
                ImageData containerImage = DaggerfallUnity.ItemHelper.GetContainerImage(ContainerTypes.Wagon);
                remoteTargetIconPanel.BackgroundTexture = containerImage.texture;

                // Highlight wagon button
                wagonButton.BackgroundTexture = wagonSelected;

                // Set remote items
                remoteItems = playerEntity.WagonItems;
            }
        }

        /// <summary>
        /// Creates filtered list of local items based on view state.
        /// </summary>
        void FilterLocalItems()
        {
            // Clear current references
            localItemsFiltered.Clear();

            // Do nothing if no items
            if (localItems == null || localItems.Count == 0)
                return;

            // Add items to list
            foreach (var kvp in localItems.Items)
            {
                DaggerfallUnityItem item = kvp.Value;

                // Reject if equipped
                if (item.IsEquipped)
                    continue;

                bool isWeaponOrArmor = (item.ItemGroup == ItemGroups.Weapons || item.ItemGroup == ItemGroups.Armor);

                // Add based on view
                if (selectedTabPage == TabPages.WeaponsAndArmor)
                {
                    // Weapons and armor
                    if (isWeaponOrArmor && !item.IsEnchanted)
                        localItemsFiltered.Add(item);
                }
                else if (selectedTabPage == TabPages.MagicItems)
                {
                    // Enchanted items
                    if (item.IsEnchanted)
                        localItemsFiltered.Add(item);
                }
                else if (selectedTabPage == TabPages.Ingredients)
                {
                    // Ingredients
                    if (item.IsIngredient && !item.IsEnchanted)
                        localItemsFiltered.Add(item);
                }
                else if (selectedTabPage == TabPages.ClothingAndMisc)
                {
                    // Everything else
                    if (!isWeaponOrArmor && !item.IsEnchanted && !item.IsIngredient)
                        localItemsFiltered.Add(item);
                }
            }
        }

        /// <summary>
        /// Creates filtered list of remote items.
        /// For now this just creates a flat list, as that is Daggerfall's behaviour.
        /// </summary>
        void FilterRemoteItems()
        {
            // Clear current references
            remoteItemsFiltered.Clear();

            // Do nothing if no items
            if (remoteItems == null || remoteItems.Count == 0)
                return;

            // Add items to list
            foreach (var kvp in remoteItems.Items)
            {
                DaggerfallUnityItem item = kvp.Value;
                remoteItemsFiltered.Add(item);
            }
        }

        /// <summary>
        /// Updates local items display.
        /// </summary>
        void UpdateLocalItemsDisplay()
        {
            // Clear list elements
            ClearLocalItemsElements();
            if (localItemsFiltered == null || localItemsFiltered.Count == 0)
                return;

            // Update images and tooltips
            int index = localItemsScrollBar.ScrollIndex;
            for (int i = 0; i < listDisplayUnits; i++)
            {
                // Skip if out of bounds
                if (index + i >= localItemsFiltered.Count)
                    continue;

                // Get item and image
                DaggerfallUnityItem item = localItemsFiltered[index + i];
                ImageData image = GetInventoryImage(item);                

                // Set image to button icon
                localItemsIconPanels[i].BackgroundTexture = image.texture;
                localItemsIconPanels[i].Size = new Vector2(image.texture.width, image.texture.height);

                // Set stack count
                if (item.stackCount > 1)
                    localItemsStackLabels[i].Text = item.stackCount.ToString();

                // Tooltip text
                string text = item.LongName;
                localItemsButtons[i].ToolTipText = text;
            }

            // Update scrollbar
            localItemsScrollBar.TotalUnits = localItemsFiltered.Count;
            UpdateListScrollerButtons(index, localItemsFiltered.Count, localItemsUpButton, localItemsDownButton);
        }

        /// <summary>
        /// Updates remote items display.
        /// </summary>
        void UpdateRemoteItemsDisplay()
        {
            // Clear list elements
            ClearRemoteItemsElements();
            if (remoteItems == null || remoteItems.Count == 0)
                return;

            // Update images and tooltips
            int index = remoteItemsScrollBar.ScrollIndex;
            for (int i = 0; i < listDisplayUnits; i++)
            {
                // Skip if out of bounds
                if (index + i >= remoteItemsFiltered.Count)
                    continue;

                // Get item and image
                DaggerfallUnityItem item = remoteItemsFiltered[index + i];
                ImageData image = GetInventoryImage(item);

                // Set image to button icon
                remoteItemsIconPanels[i].BackgroundTexture = image.texture;
                remoteItemsIconPanels[i].Size = new Vector2(image.texture.width, image.texture.height);

                // Set stack count
                if (item.stackCount > 1)
                    remoteItemsStackLabels[i].Text = item.stackCount.ToString();

                // Tooltip text
                string text = item.LongName;
                remoteItemsButtons[i].ToolTipText = text;
            }

            // Update scrollbar
            remoteItemsScrollBar.TotalUnits = remoteItemsFiltered.Count;
            UpdateListScrollerButtons(index, remoteItemsFiltered.Count, remoteItemsUpButton, remoteItemsDownButton);
        }

        /// <summary>
        /// Updates accessory items display.
        /// </summary>
        void UpdateAccessoryItemsDisplay()
        {
            // Follow same order as equip slots
            int minSlot = (int)EquipSlots.Amulet0;
            int maxSlot = (int)EquipSlots.Crystal1;
            for (int i = minSlot; i <= maxSlot; i++)
            {
                // Get button and panel for this slot
                Button button = accessoryButtons[i];
                Panel panel = accessoryIconPanels[i];
                if (button == null || panel == null)
                    return;

                // Get item at this equip index (if any)
                DaggerfallUnityItem item = playerEntity.ItemEquipTable.GetItem((EquipSlots)button.Tag);
                if (item == null)
                {
                    panel.BackgroundTexture = null;
                    button.ToolTipText = string.Empty;
                    continue;
                }

                // Update button and panel
                ImageData image = GetInventoryImage(item);
                panel.BackgroundTexture = image.texture;
                panel.Size = new Vector2(image.width, image.height);
                button.ToolTipText = item.LongName;
            }
        }

        #endregion

        #region Private Methods

        void LoadTextures()
        {
            // Load source textures
            baseTexture = ImageReader.GetTexture(baseTextureName);
            goldTexture = ImageReader.GetTexture(goldTextureName);

            // Cut out tab page not selected button textures
            weaponsAndArmorNotSelected = ImageReader.GetSubTexture(baseTexture, weaponsAndArmorRect);
            magicItemsNotSelected = ImageReader.GetSubTexture(baseTexture, magicItemsRect);
            clothingAndMiscNotSelected = ImageReader.GetSubTexture(baseTexture, clothingAndMiscRect);
            ingredientsNotSelected = ImageReader.GetSubTexture(baseTexture, ingredientsRect);

            // Cut out tab page selected button textures
            weaponsAndArmorSelected = ImageReader.GetSubTexture(goldTexture, weaponsAndArmorRect);
            magicItemsSelected = ImageReader.GetSubTexture(goldTexture, magicItemsRect);
            clothingAndMiscSelected = ImageReader.GetSubTexture(goldTexture, clothingAndMiscRect);
            ingredientsSelected = ImageReader.GetSubTexture(goldTexture, ingredientsRect);

            // Cut out red up/down arrows
            Texture2D redArrowsTexture = ImageReader.GetTexture(redArrowsTextureName);
            redUpArrow = ImageReader.GetSubTexture(redArrowsTexture, upArrowRect);
            redDownArrow = ImageReader.GetSubTexture(redArrowsTexture, downArrowRect);

            // Cut out green up/down arrows
            Texture2D greenArrowsTexture = ImageReader.GetTexture(greenArrowsTextureName);
            greenUpArrow = ImageReader.GetSubTexture(greenArrowsTexture, upArrowRect);
            greenDownArrow = ImageReader.GetSubTexture(greenArrowsTexture, downArrowRect);

            // Cut out action mode not selected buttons
            wagonNotSelected = ImageReader.GetSubTexture(baseTexture, wagonButtonRect);
            infoNotSelected = ImageReader.GetSubTexture(baseTexture, infoButtonRect);
            equipNotSelected = ImageReader.GetSubTexture(baseTexture, equipButtonRect);
            removeNotSelected = ImageReader.GetSubTexture(baseTexture, removeButtonRect);
            useNotSelected = ImageReader.GetSubTexture(baseTexture, useButtonRect);

            // Cut out action mode selected buttons
            wagonSelected = ImageReader.GetSubTexture(goldTexture, wagonButtonRect);
            infoSelected = ImageReader.GetSubTexture(goldTexture, infoButtonRect);
            equipSelected = ImageReader.GetSubTexture(goldTexture, equipButtonRect);
            removeSelected = ImageReader.GetSubTexture(goldTexture, removeButtonRect);
            useSelected = ImageReader.GetSubTexture(goldTexture, useButtonRect);
        }

        #endregion

        #region Tab Page Event Handlers

        private void WeaponsAndArmor_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            SelectTabPage(TabPages.WeaponsAndArmor);
        }

        private void MagicItems_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            SelectTabPage(TabPages.MagicItems);
        }

        private void ClothingAndMisc_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            SelectTabPage(TabPages.ClothingAndMisc);
        }

        private void Ingredients_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            SelectTabPage(TabPages.Ingredients);
        }

        #endregion

        #region Action Button Event Handlers

        private void WagonButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            // TODO:
            // Wagon currently locked open as ground loot piles not implemented
            // Later need to implement variable remote targets for item exchange (wagon, shop, loot, etc.)
        }

        private void InfoButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            SelectActionMode(ActionModes.Info);
        }

        private void EquipButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            SelectActionMode(ActionModes.Equip);
        }

        private void RemoveButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            SelectActionMode(ActionModes.Remove);
        }

        private void UseButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            SelectActionMode(ActionModes.Use);
        }

        #endregion

        #region ScrollBar Event Handlers

        private void LocalItemsScrollBar_OnScroll()
        {
            UpdateLocalItemsDisplay();
        }

        private void LocalItemsUpButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            localItemsScrollBar.ScrollIndex--;
        }

        private void LocalItemsDownButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            localItemsScrollBar.ScrollIndex++;
        }

        private void MyItemsListPanel_OnMouseScrollUp()
        {
            localItemsScrollBar.ScrollIndex--;
        }

        private void MyItemsListPanel_OnMouseScrollDown()
        {
            localItemsScrollBar.ScrollIndex++;
        }

        #endregion

        #region Remote Items List Events

        private void RemoteItemsScrollBar_OnScroll()
        {
            UpdateRemoteItemsDisplay();
        }

        private void RemoteItemsUpButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            remoteItemsScrollBar.ScrollIndex--;
            UpdateRemoteItemsDisplay();
        }

        private void RemoteItemsDownButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            remoteItemsScrollBar.ScrollIndex++;
            UpdateRemoteItemsDisplay();
        }

        private void RemoteItemsListPanel_OnMouseScrollUp()
        {
            remoteItemsScrollBar.ScrollIndex--;
            UpdateRemoteItemsDisplay();
        }

        private void RemoteItemsListPanel_OnMouseScrollDown()
        {
            remoteItemsScrollBar.ScrollIndex++;
            UpdateRemoteItemsDisplay();
        }

        private void PaperDoll_OnMouseMove(int x, int y)
        {
            byte value = paperDoll.GetEquipIndex(x, y);
            if (value != 0xff)
            {
                // Only update when index changed
                if (value == lastMouseOverPaperDollEquipIndex)
                    return;
                else
                    lastMouseOverPaperDollEquipIndex = value;

                // Test index is inside range
                string text = string.Empty;
                if (value >= 0 && value < ItemEquipTable.EquipTableLength)
                {
                    DaggerfallUnityItem item = playerEntity.ItemEquipTable.EquipTable[value];
                    if (item != null)
                        text = item.LongName;
                }

                // Update tooltip text
                paperDoll.ToolTipText = text;
            }
            else
            {
                // Clear tooltip text
                paperDoll.ToolTipText = string.Empty;
                lastMouseOverPaperDollEquipIndex = value;
            }
        }

        #endregion

        #region Click Event Handlers

        private void LocalItemsButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            //int index = localItemsScrollBar.ScrollIndex + (int)sender.Tag;

            //int index = scrollPositions[(int)selectedTabPage] + (int)sender.Tag;

            //// Get selected items list
            //List<DaggerfallUnityItem> items = SelectedMyItemsList();
            //if (items == null)
            //    return;

            //// Get item
            //DaggerfallUnityItem item = items[index];

            //// Equip item
            //playerEntity.ItemEquipTable.EquipItem(item);
            //paperDoll.Refresh();
        }

        private void RemoteItemsButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            //int index = remoteItemsScrollBar.ScrollIndex + (int)sender.Tag;
        }

        private void Accessory_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
        }

        #endregion
    }
}