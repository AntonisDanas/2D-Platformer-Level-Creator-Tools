/*
 * Script that defines the inspector of the level game object.
 * It creates the grid for the play area and also defines 
 * the scene actions (View, Paint, Edit, Erase)
 */

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Level))]
public class LevelInspector : Editor {

    private Level levelTarget; //the target for the inspector

    private int newColumnSize;
    private int newRowSize;

    private PaletteItem itemSelected; // the selected item
    private Texture2D itemPreview; // the preview of the item
    private LevelPiece pieceSelected; // the LevelPiece selected

    // the modes of the scene actions used for editing
    public enum Mode {
        View,
        Paint,
        Edit,
        Erase,
    }
    private Mode selectedMode; // the selected mode
    private Mode currentMode; // the current mode
    private PaletteItem itemInspected; // the selected item to be in the inspector

    // the original x and y positions of the item to be edited in the scene
    private int originalPosX;
    private int originalPosY;

    private void OnEnable() {
        levelTarget = (Level)target;
        InitLevel();
        ResetResizeValues();
        SubscribeEvents();
    }

    private void OnDisable() {
        UnsubscribeEvents();
    }

    private void OnSceneGUI() {
        DrawModeGUI();
        ModeHandler();
        EventHandler();
    }

    public override void OnInspectorGUI() {
        // For debugging reasons
        //DrawDefaultInspector();
        DrawGridData();
        DrawPieceSelectedGUI();
        DrawInspectedItemGUI();

        if (GUI.changed) {
            EditorUtility.SetDirty(levelTarget);
        }
    }

    /// <summary>
    /// Draw the grid data to the inspector
    /// </summary>
    private void DrawGridData() {
        EditorGUILayout.LabelField("Grid Data", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal("box");

        EditorGUILayout.BeginVertical();
        newColumnSize = EditorGUILayout.IntField("Columns", Mathf.Max(1, newColumnSize));
        newRowSize = EditorGUILayout.IntField("Rows", Mathf.Max(1, newRowSize));
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical();
        // With this variable we can enable or disable the buttons
        // is the size of the inspector are different from the script 
        bool oldEnabled = GUI.enabled;
        GUI.enabled = (newColumnSize != levelTarget.ColumnSize || newRowSize != levelTarget.RowSize);
        bool buttonResize = GUILayout.Button("Resize", GUILayout.Height(2 * EditorGUIUtility.singleLineHeight));
        if (buttonResize) {
            if (EditorUtility.DisplayDialog(
                    "Level Creator",
                    "Are you sure you want to resize the level?\nThis action cannot be undone.",
                    "Yes",
                    "No")) {
                ResizeLevel();
            }
        }
        bool buttonReset = GUILayout.Button("Reset");
        if (buttonReset) {
            ResetResizeValues();
        }
        GUI.enabled = oldEnabled;
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();
    }

    /// <summary>
    /// Initialize the level
    /// </summary>
    private void InitLevel() {
        // if there aren't any level pieces instantiate the array
        if (levelTarget.Pieces == null || levelTarget.Pieces.Length == 0) {
            Debug.Log("Initializing the Pieces array...");
            levelTarget.Pieces = new LevelPiece[levelTarget.ColumnSize * levelTarget.RowSize];
        }
        // hide the transform ability of the level game object because
        // we don't want the user to be able to change it's position
        levelTarget.transform.hideFlags = HideFlags.NotEditable;
    }

    /// <summary>
    /// Reset the size values
    /// </summary>
    private void ResetResizeValues() {
        newColumnSize = levelTarget.ColumnSize;
        newRowSize = levelTarget.RowSize;
    }

    /// <summary>
    /// Resize the grid of the level
    /// </summary>
    private void ResizeLevel() {
        // create a new array for the pieces
        LevelPiece[] newPieces = new LevelPiece[newColumnSize * newRowSize];
        // we go through the old array and we transfer to the new one all
        // the pieces that are inside the new bounds
        for (int col = 0; col < levelTarget.ColumnSize; ++col) {
            for (int row = 0; row < levelTarget.RowSize; ++row) {
                if (col < newColumnSize && row < newRowSize) {
                    newPieces[col + row * newColumnSize] = levelTarget.Pieces[col + row * levelTarget.ColumnSize];
                }
                else {
                    LevelPiece piece = levelTarget.Pieces[col + row * levelTarget.ColumnSize];
                    if (piece != null) {
                        // we must to use DestroyImmediate in an Editor context
                        Object.DestroyImmediate(piece.gameObject);
                    }
                }
            }
        }
        levelTarget.Pieces = newPieces;
        levelTarget.ColumnSize = newColumnSize;
        levelTarget.RowSize = newRowSize;
    }

    private void SubscribeEvents() {
        PaletteWindow.ItemSelectedEvent += new PaletteWindow.itemSelectedDelegate(UpdateCurrentPieceInstance);
    }

    private void UnsubscribeEvents() {
        PaletteWindow.ItemSelectedEvent -= new PaletteWindow.itemSelectedDelegate(UpdateCurrentPieceInstance);
    }

    /// <summary>
    /// The subscribed method to the item selected event. It sets the selected item and preview
    /// and repaint
    /// </summary>
    /// <param name="item"></param>
    /// <param name="preview"></param>
    private void UpdateCurrentPieceInstance(PaletteItem item, Texture2D preview) {
        itemSelected = item;
        itemPreview = preview;
        pieceSelected = (LevelPiece)item.GetComponent<LevelPiece>();
        Repaint();
    }

    /// <summary>
    /// Draw the GUI for the selected piece if there is any
    /// </summary>
    private void DrawPieceSelectedGUI() {
        EditorGUILayout.LabelField("Piece Selected", EditorStyles.boldLabel);
        if (pieceSelected == null) {
            EditorGUILayout.HelpBox("No piece selected!", MessageType.Info);
        }
        else {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField(new GUIContent(itemPreview),
            GUILayout.Height(40));
            EditorGUILayout.LabelField(itemSelected.itemName);
            EditorGUILayout.EndVertical();
        }
    }

    /// <summary>
    /// Draw the GUI for the modes in the scene view
    /// </summary>
    private void DrawModeGUI() {
        // get a list of all the modes
        List<Mode> modes = LevelCreatorUtils.GetListFromEnum<Mode>();
        // get the names of the modes
        List<string> modeLabels = new List<string>();
        foreach (Mode mode in modes) {
            modeLabels.Add(mode.ToString());
        }

        Handles.BeginGUI();
        // create the toolbar for the modes
        GUILayout.BeginArea(new Rect(10f, 10f, 360, 40f));
        selectedMode = (Mode)GUILayout.Toolbar((int)currentMode,
                                                modeLabels.ToArray(),
                                                GUILayout.ExpandHeight(true));
        GUILayout.EndArea();

        Handles.EndGUI();
    }

    /// <summary>
    /// Handles the mode selection
    /// </summary>
    private void ModeHandler() {
        switch (selectedMode) {
            case Mode.Paint:
            case Mode.Edit:
            case Mode.Erase:
                Tools.current = Tool.None;
                break;
            case Mode.View:
            default:
                Tools.current = Tool.View;
                break;
        }
        // Detect Mode change
        if (selectedMode != currentMode) {
            currentMode = selectedMode;
            itemInspected = null;
            Repaint();
        }
        // Force 2D Mode!
        SceneView.currentDrawingSceneView.in2DMode = true;
    }

    /// <summary>
    /// Handles the events activated by the modes
    /// </summary>
    private void EventHandler() {
        // always keep the focus on the level unless the user chooses another game object
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

        // get the scene view camera (not live the game camera)
        Camera camera = SceneView.currentDrawingSceneView.camera;

        // get the current position of the mouse
        Vector3 mousePosition = Event.current.mousePosition;
        // Invert the mouse position so that top left corner is (0,9).
        // Without it the top left is considered as (0,0)
        mousePosition = new Vector2(mousePosition.x, camera.pixelHeight - mousePosition.y);
        // convert the world position from the mouse to a grid position
        Vector3 worldPos = camera.ScreenToWorldPoint(mousePosition);
        Vector3 gridPos = levelTarget.WorldToGridCoordinates(worldPos);
        int col = (int)gridPos.x;
        int row = (int)gridPos.y;
        // depending on the mode call the appropriate method
        switch (selectedMode) {
            case Mode.Paint:
                if (Event.current.type == EventType.MouseDown ||
                    Event.current.type == EventType.MouseDrag) {
                    Paint(col, row);
                }
                break;
            case Mode.Edit:
                if (Event.current.type == EventType.MouseDown) {
                    Edit(col, row);
                    originalPosX = col;
                    originalPosY = row;
                }
                // when the user releases the mouse move the item
                if (Event.current.type == EventType.MouseUp ||
                Event.current.type == EventType.Ignore) {
                    if (itemInspected != null) {
                        Move();
                    }
                }
                // while we have a selected item give it a free move handle
                if (itemInspected != null) {
                    itemInspected.transform.position =
                    Handles.FreeMoveHandle(itemInspected.transform.position,
                                            itemInspected.transform.rotation,
                                            Level.gridSize / 2,
                                            Level.gridSize / 2 * Vector3.one,
                                            Handles.RectangleCap);
                }
                break;
            case Mode.Erase:
                if (Event.current.type == EventType.MouseDown ||
                    Event.current.type == EventType.MouseDrag) {
                    Erase(col, row);
                }
                break;
            case Mode.View:
            default:
                Tools.current = Tool.View;
                break;
        }
    }

    /// <summary>
    /// Paint the given cell with the selected item
    /// </summary>
    /// <param name="col"></param>
    /// <param name="row"></param>
    private void Paint(int col, int row) {
        // check out of bounds and if we have a piece selected
        if (!levelTarget.IsInsideGridBounds(col, row) || pieceSelected == null) {
            return;
        }
        // check if we need to destroy a previous piece
        if (levelTarget.Pieces[col + row * levelTarget.ColumnSize] != null) {
            DestroyImmediate(levelTarget.Pieces[col + row * levelTarget.ColumnSize].gameObject);
        }
        // do paint !
        GameObject obj = PrefabUtility.InstantiatePrefab(pieceSelected.gameObject) as GameObject;
        obj.transform.parent = levelTarget.transform;
        obj.name = string.Format("[{0},{1}][{2}]", col, row, obj.name);
        obj.transform.position = levelTarget.GridToWorldCoordinates(col,row);
        // hide it from the hierarchy so that the user can't corrupt the item's position
        obj.hideFlags = HideFlags.HideInHierarchy;
        levelTarget.Pieces[col + row * levelTarget.ColumnSize] = obj.GetComponent<LevelPiece>();
    }

    /// <summary>
    /// Erase the item if the cell has one
    /// </summary>
    /// <param name="col"></param>
    /// <param name="row"></param>
    private void Erase(int col, int row) {
        // check out of bounds
        if (!levelTarget.IsInsideGridBounds(col, row)) {
            return;
        }
        // do Erase
        if (levelTarget.Pieces[col + row * levelTarget.ColumnSize] != null) {
            DestroyImmediate(levelTarget.Pieces[col + row * levelTarget.ColumnSize].gameObject);
        }
    }

    /// <summary>
    /// Edit the selected item if there is one
    /// </summary>
    /// <param name="col"></param>
    /// <param name="row"></param>
    private void Edit(int col, int row) {
        // check out of bounds
        if (!levelTarget.IsInsideGridBounds(col, row) ||
        levelTarget.Pieces[col + row * levelTarget.ColumnSize] == null) {
            itemInspected = null;
        }
        else {
            itemInspected = levelTarget.Pieces[col + row * levelTarget.ColumnSize].
                                    GetComponent<PaletteItem>() as PaletteItem;
        }
        Repaint();
    }

    /// <summary>
    /// Move the item
    /// </summary>
    private void Move() {
        Vector3 gridPoint = levelTarget.WorldToGridCoordinates(itemInspected.transform.position);
        int col = (int)gridPoint.x;
        int row = (int)gridPoint.y;
        if (col == originalPosX && row == originalPosY) {
            return;
        }
        if (!levelTarget.IsInsideGridBounds(col, row) ||
        levelTarget.Pieces[col + row * levelTarget.ColumnSize] != null) {
            itemInspected.transform.position =
            levelTarget.GridToWorldCoordinates(originalPosX,
            originalPosY);
        }
        else {
            levelTarget.Pieces[originalPosX + originalPosY *
            levelTarget.ColumnSize] = null;
            levelTarget.Pieces[col + row * levelTarget.ColumnSize] =
            itemInspected.GetComponent<LevelPiece>();
            levelTarget.Pieces[col + row *
            levelTarget.ColumnSize].transform.position =
            levelTarget.GridToWorldCoordinates(col, row);
        }
    }

    /// <summary>
    /// Draw the GUI for the inspected item
    /// </summary>
    private void DrawInspectedItemGUI() {
        // only show this GUI if we are in edit mode.
        if (currentMode != Mode.Edit) {
            return;
        }
        EditorGUILayout.LabelField("Piece Edited", EditorStyles.boldLabel);
        // if we can an item selected for inspection create an inspector for it
        // else print a helpbox
        if (itemInspected != null) {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Name: " + itemInspected.name);
            // draw a new inspector for the selected item 
            Editor.CreateEditor(itemInspected.inspectedScript).OnInspectorGUI();
            EditorGUILayout.EndVertical();
        }
        else {
            EditorGUILayout.HelpBox("No piece to edit!",MessageType.Info);
        }
    }

}
