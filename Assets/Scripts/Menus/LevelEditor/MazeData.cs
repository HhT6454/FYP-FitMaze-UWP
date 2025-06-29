using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MazeData
{
    public string mode;
    public int rows;
    public int columns;
    public CellData[,] cells;
    public List<SerializableCellData> cellsSerialized; // For serialization
    public Vector2Int start;
    public Vector2Int end;
    public List<ElementData> elements;

    // Convert 2D cells array to serializable list
    public void PrepareForSerialization()
    {
        cellsSerialized = new List<SerializableCellData>();
        for (int x = 0; x < rows; x++)
        {
            for (int y = 0; y < columns; y++)
            {
                cellsSerialized.Add(new SerializableCellData
                {
                    x = x,
                    y = y,
                    IsVisited = cells[x, y].IsVisited,
                    WallRight = cells[x, y].WallRight,
                    WallFront = cells[x, y].WallFront,
                    WallLeft = cells[x, y].WallLeft,
                    WallBack = cells[x, y].WallBack,
                    IsGoal = cells[x, y].IsGoal,
                    IsStart = cells[x, y].IsStart
                });
            }
        }
        cells = null; // Prevent JsonUtility from serializing the 2D array
    }

    // Convert serializable list back to 2D cells array
    public void RestoreAfterDeserialization()
    {
        cells = new CellData[rows, columns];
        foreach (var serializedCell in cellsSerialized)
        {
            cells[serializedCell.x, serializedCell.y] = new CellData
            {
                IsVisited = serializedCell.IsVisited,
                WallRight = serializedCell.WallRight,
                WallFront = serializedCell.WallFront,
                WallLeft = serializedCell.WallLeft,
                WallBack = serializedCell.WallBack,
                IsGoal = serializedCell.IsGoal,
                IsStart = serializedCell.IsStart
            };
        }
        cellsSerialized = null; // Clear to save memory
    }

    [Serializable]
    public class CellData
    {
        public bool IsVisited = false;
        public bool WallRight = true;
        public bool WallFront = true;
        public bool WallLeft = true;
        public bool WallBack = true;
        public bool IsGoal = false;
        public bool IsStart = false;
    }

    [Serializable]
    public class SerializableCellData
    {
        public int x;
        public int y;
        public bool IsVisited;
        public bool WallRight;
        public bool WallFront;
        public bool WallLeft;
        public bool WallBack;
        public bool IsGoal;
        public bool IsStart;
    }

    [Serializable]
    public class ElementData
    {
        public string type;
        public float detectionSize = 0f;
    }
}