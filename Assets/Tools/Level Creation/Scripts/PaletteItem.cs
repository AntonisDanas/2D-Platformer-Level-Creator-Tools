/*
 * This class is a container that holds all the important info of
 * the level piece that is to be displayed at the palette window.
 * Every level piece that is to be displayed there must have this script
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaletteItem : MonoBehaviour {

#if UNITY_EDITOR
    public enum Category {
        Misc,
        Player,
        Collectables,
        Enemies,
        Terrain,
    }

    public Category category = Category.Misc;
    public string itemName = "";
    // This uses a script as a reference for the object. It must contain the
    // main script of the level piece
    public Object inspectedScript; 
#endif

}
