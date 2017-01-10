using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour {

    #region variables

    [SerializeField]
    private int columnSize = 20;
    [SerializeField]
    private int rowSize = 10;

    // This array holds info for all the pieces in the grid.
    [SerializeField]
    private LevelPiece[] pieces;

    // This depends on the size of the texture and the the pixels per unit
    // for this example i have 128X128 and 100 respectively.
    public const float gridSize = 1.28f;

    private readonly Color normalColor = Color.gray;
    private readonly Color selectedColor = Color.yellow;

    public int ColumnSize {
        get { return columnSize; }
        set { columnSize = value; }
    }

    public int RowSize {
        get { return rowSize; }
        set { rowSize = value; }
    }

    public LevelPiece[] Pieces {
        get { return pieces; }
        set { pieces = value; }
    }

    #endregion

    #region grid gizmos

    private void OnDrawGizmos() {
        // Save the values to be changes so that they can be restored afterwards
        // (good tactic since Gizmo variables are statics and their value should not be changed)
        Color oldColor = Gizmos.color;
        Matrix4x4 oldMatrix = Gizmos.matrix;
        // Move the Gizmo's origin to the position of the game object
        Gizmos.matrix = transform.localToWorldMatrix;
        // Draw the gizmos
        Gizmos.color = normalColor;
        GridGizmo(columnSize, rowSize);
        GridFrameGizmo(columnSize, rowSize);
        // Restore the values
        Gizmos.color = oldColor;
        Gizmos.matrix = oldMatrix;
    }

    private void OnDrawGizmosSelected() {
        // Save the values to be changes so that they can be restored afterwards
        // (good tactic since Gizmo variables are statics and their value should not be changed)
        Color oldColor = Gizmos.color;
        Matrix4x4 oldMatrix = Gizmos.matrix;
        // Move the Gizmo's origin to the position of the game object
        Gizmos.matrix = transform.localToWorldMatrix;
        // Draw the gizmos
        Gizmos.color = selectedColor;
        GridFrameGizmo(columnSize, rowSize);
        // Restore the values
        Gizmos.color = oldColor;
        Gizmos.matrix = oldMatrix;
    }

    /// <summary>
    /// Draw the frame of the grid
    /// </summary>
    /// <param name="cols"></param>
    /// <param name="rows"></param>
    private void GridFrameGizmo(int cols, int rows) {
        Gizmos.DrawLine(new Vector3(0, 0, 0), new Vector3(0, rows *
        gridSize, 0));
        Gizmos.DrawLine(new Vector3(0, 0, 0), new Vector3(cols *
        gridSize, 0, 0));
        Gizmos.DrawLine(new Vector3(cols * gridSize, 0, 0), new
        Vector3(cols * gridSize, rows * gridSize, 0));
        Gizmos.DrawLine(new Vector3(0, rows * gridSize, 0), new
        Vector3(cols * gridSize, rows * gridSize, 0));
    }

    /// <summary>
    /// Draw the main grid
    /// </summary>
    /// <param name="cols"></param>
    /// <param name="rows"></param>
    private void GridGizmo(int cols, int rows) {
        for (int i = 1; i < cols; i++) {
            Gizmos.DrawLine(new Vector3(i * gridSize, 0, 0), new Vector3(i
            * gridSize, rows * gridSize, 0));
        }
        for (int j = 1; j < rows; j++) {
            Gizmos.DrawLine(new Vector3(0, j * gridSize, 0), new
            Vector3(cols * gridSize, j * gridSize, 0));
        }
    }

    #endregion

    #region snap to grid methods

    /// <summary>
    /// Convert a world point to a grid point
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public Vector3 WorldToGridCoordinates(Vector3 point) {
        Vector3 gridPoint = new Vector3(
        (int)((point.x - transform.position.x) / gridSize),
        (int)((point.y - transform.position.y) / gridSize), 0.0f);
        return gridPoint;
    }

    /// <summary>
    /// Convert a grid point to a world point
    /// </summary>
    /// <param name="col"></param>
    /// <param name="row"></param>
    /// <returns></returns>
    public Vector3 GridToWorldCoordinates(int col, int row) {
        Vector3 worldPoint = new Vector3(
        transform.position.x + (col * gridSize + gridSize / 2.0f),
        transform.position.y + (row * gridSize + gridSize / 2.0f),
        0.0f);
        return worldPoint;
    }

    /// <summary>
    /// Check if vector point is inside the grid
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public bool IsInsideGridBounds(Vector3 point) {
        float minX = transform.position.x;
        float maxX = minX + columnSize * gridSize;
        float minY = transform.position.y;
        float maxY = minY + rowSize * gridSize;
        return (point.x >= minX && point.x <= maxX && point.y >= minY && point.y <= maxY);
    }

    /// <summary>
    /// Check if given column and row are inside the grid
    /// </summary>
    /// <param name="col"></param>
    /// <param name="row"></param>
    /// <returns></returns>
    public bool IsInsideGridBounds(int col, int row) {
        return (col >= 0 && col < columnSize && row >= 0 && row < rowSize);
    }

    #endregion

}
