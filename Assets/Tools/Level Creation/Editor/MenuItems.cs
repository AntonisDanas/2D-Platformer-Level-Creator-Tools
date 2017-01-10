/*
 * This script contains all the menu item definitions
 */

using UnityEngine;
using UnityEditor;

public static class MenuItems {

    // Menu item for creating a new custom level
	[MenuItem ("Tools/Level Creator/New Level Scene")]
    private static void NewLevel() {
        LevelCreatorUtils.NewLevel();
    }

    // Menu item to open the Level Piece palette 
    [MenuItem ("Tools/Level Creator/Level Palette")]
    private static void ShowPalette() {
        PaletteWindow.ShowPalette();
    }

}
