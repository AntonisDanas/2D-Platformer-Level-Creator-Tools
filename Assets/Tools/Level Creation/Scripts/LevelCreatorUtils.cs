/*
 * This script contains general utils for the level creator tools
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class LevelCreatorUtils {

    /// <summary>
    /// Creates a new empty scene. It asks the user if they want to save the previous one.
    /// </summary>
    private static void NewScene() {
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
    }

    /// <summary>
    /// Automates the creation of a custom scene. Called by the tool menu.
    /// </summary>
    public static void NewLevel() {
        NewScene();
        GameObject level = new GameObject("Level");
        level.transform.position = Vector3.zero;
        level.AddComponent<Level>();
    }

    /// <summary>
    /// Get all the assets with the specified script if any
    /// </summary>
    /// <typeparam name="T">The reference script</typeparam>
    /// <param name="path">Path to search</param>
    /// <returns>List of all the assets</returns>
    public static List<T> GetAssetsWithScript<T> (string path) where T : MonoBehaviour{
        T tmp;
        string assetPath;
        GameObject asset;

        List<T> assetList = new List<T>();
        // Find all the assets in the given path
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new string[] { path });
        for (int i = 0; i < guids.Length; i++) {
            // Get the path of the asset based on th GUID
            assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
            // Get the asset at that path
            asset = AssetDatabase.LoadAssetAtPath(assetPath,typeof(GameObject)) as GameObject;
            // If the asset has the reference script we want we add it to the list we return
            tmp = asset.GetComponent<T>();
            if (tmp != null) {
                assetList.Add(tmp);
            }
        }

        return assetList;
    }

    /// <summary>
    /// Get a list from an enumeration
    /// </summary>
    /// <typeparam name="T">The enumeration type</typeparam>
    /// <returns>A list of the values of the enumeration</returns>
    public static List<T> GetListFromEnum<T>() {
        List<T> enumList = new List<T>();

        //Retrieves an array of the values of the constants in a specified enumeration.
        System.Array enums = System.Enum.GetValues(typeof(T));
        //Add all values to a list and return it
        foreach (T e in enums) {
            enumList.Add(e);
        }
        return enumList;
    }

}
