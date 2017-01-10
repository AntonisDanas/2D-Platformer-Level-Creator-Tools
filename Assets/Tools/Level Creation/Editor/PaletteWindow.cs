/*
 * Script that defines the use of the Level Piece palette window
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PaletteWindow : EditorWindow {

    public static PaletteWindow instance; // the window instance

    private List<PaletteItem.Category> categories; // all the categories
    private List<string> categoryLabels; // the categories' names
    private PaletteItem.Category categorySelected; // the current selected category

    private string path = "Assets/Prefabs/LevelPieces"; // the path to search for the items
    private List<PaletteItem> items; // all the items
    private Dictionary<PaletteItem.Category, List<PaletteItem>> categorizedItems; // the items in categories
    private Dictionary<PaletteItem, Texture2D> previews; // previews of each items
    private Vector2 scrollPosition;
    private const float ButtonWidth = 80;
    private const float ButtonHeight = 90;

    // event system for item selection
    public delegate void itemSelectedDelegate(PaletteItem item, Texture2D preview);
    public static event itemSelectedDelegate ItemSelectedEvent;

    /// <summary>
    /// Instantiates the palette window
    /// </summary>
    public static void ShowPalette() {
        instance = (PaletteWindow)GetWindow<PaletteWindow>();
        instance.titleContent = new GUIContent("Level Palette");
        instance.minSize = new Vector2(500f, 300f);
    }

    private void OnEnable() {
        // if there are no categories try and instantiate them
        if (categories == null) {
            InitCategories();
        }
        // if there are no categorized items try and instantiate them
        if (categorizedItems == null) {
            InitContent();
        }
    }

    private void OnInspectorUpdate() {
        // if the asset count with the PaletteItem script is different 
        // from the current item count then reinstantiate items
        if (LevelCreatorUtils.GetAssetsWithScript<PaletteItem>(path).Count != items.Count) {
            InitContent();
        }
        // if the preview count is different from the item count
        // reinstantiate previews
        if (previews.Count != items.Count) {
            GeneratePreviews();
        }
    }

    private void OnGUI() {
        // draw the toolbar
        DrawTabs();
        // draw the scrolling
        DrawScroll();
    }

    /// <summary>
    /// Draws the category toolbar of the window
    /// </summary>
    private void DrawTabs() {
        // get the selected category
        int index = (int)categorySelected;
        // instantiate the toolbar with the category selected
        index = GUILayout.Toolbar(index, categoryLabels.ToArray());
        // get the selected category
        categorySelected = categories[index];
    }

    /// <summary>
    /// Draws the scrolling part of the window
    /// </summary>
    private void DrawScroll() {
        // if there are no items in the category print a helpbox
        if (categorizedItems[categorySelected].Count == 0) {
            EditorGUILayout.HelpBox("This category is empty!", MessageType.Info);
            return;
        }
        int rowCapacity = Mathf.FloorToInt(position.width / (ButtonWidth));
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        int selectionGridIndex = -1;
        selectionGridIndex = GUILayout.SelectionGrid(selectionGridIndex, 
                                                    GetGUIContentsFromItems(),
                                                    rowCapacity,
                                                    GetGUIStyle());
        GetSelectedItem(selectionGridIndex);
        GUILayout.EndScrollView();
    }

    /// <summary>
    /// Get the GUI contents from all the category selected items
    /// </summary>
    /// <returns>An array of the GUI contents</returns>
    private GUIContent[] GetGUIContentsFromItems() {
        // a list of all the GUI contents
        List<GUIContent> guiContents = new List<GUIContent>();
        // if the previews are the same as the items get the contents
        if (previews.Count == items.Count) {
            int totalItems = categorizedItems[categorySelected].Count;
            for (int i = 0; i < totalItems; i++) {
                // get the text and image from the item of the selected category 
                // and add it to the list
                GUIContent guiContent = new GUIContent();
                guiContent.text = categorizedItems[categorySelected][i].itemName;
                guiContent.image = previews[categorizedItems[categorySelected][i]];
                guiContents.Add(guiContent);
            }
        }
        // return the array of the contents
        return guiContents.ToArray();
    }

    /// <summary>
    /// Get the GUI style
    /// </summary>
    /// <returns></returns>
    private GUIStyle GetGUIStyle() {
        GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
        guiStyle.alignment = TextAnchor.LowerCenter;
        guiStyle.imagePosition = ImagePosition.ImageAbove;
        guiStyle.fixedWidth = ButtonWidth;
        guiStyle.fixedHeight = ButtonHeight;
        return guiStyle;
    }

    /// <summary>
    /// Get the item the user selected
    /// </summary>
    /// <param name="index">The index of the item</param>
    private void GetSelectedItem(int index) {
        if (index != -1) {
            // get the selected item
            PaletteItem selectedItem = categorizedItems[categorySelected][index];
            Debug.Log("Selected Item is: " + selectedItem.itemName);
            // activate everything subscribed to the event
            if (ItemSelectedEvent != null) {
                ItemSelectedEvent(selectedItem, previews[selectedItem]);
            }
        }      
    }

    /// <summary>
    /// Initialize the categories' GUI
    /// </summary>
    private void InitCategories() {
        Debug.Log("Init categories");

        // Get a list of all the categories
        categories = LevelCreatorUtils.GetListFromEnum<PaletteItem.Category>();
        // Create a list of all the categories' names
        categoryLabels = new List<string>();
        foreach (PaletteItem.Category item in categories) {
            categoryLabels.Add(item.ToString());
        }
    }

    /// <summary>
    /// Initialize the contents' GUI
    /// </summary>
    private void InitContent() {
        Debug.Log("InitContent called...");

        // Get all the items with the PaletteItem script
        items = LevelCreatorUtils.GetAssetsWithScript<PaletteItem>(path);
        // Create a Dictionary with all the categories and a list of all the items of each category
        categorizedItems = new Dictionary<PaletteItem.Category,List<PaletteItem>>();
        // Create a preview list of all the palette items 
        previews = new Dictionary<PaletteItem, Texture2D>();
        // Add the categories
        foreach (PaletteItem.Category category in categories) {
            categorizedItems.Add(category, new List<PaletteItem>());
        }
        // For each cateogry populate the list of the respective items
        foreach (PaletteItem item in items) {
            categorizedItems[item.category].Add(item);
        }
    }

    /// <summary>
    /// Generate the previews
    /// </summary>
    private void GeneratePreviews() {
        // for each item try and get the asset's preview
        foreach (PaletteItem item in items) {
            if (!previews.ContainsKey(item)) {
                Texture2D preview = AssetPreview.GetAssetPreview(item.transform.GetChild(0).gameObject);
                // if there is a preview add it to the list
                if (preview != null) {
                    previews.Add(item, preview);
                }
            }    
        }
    }

}
