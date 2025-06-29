using UnityEngine;
using System.Collections.Generic;

public class MazeGenerator : MonoBehaviour
{
    private MazeData mazeData;

    public MazeData GenerateEmpty(int rows, int columns)
    {
        mazeData = new MazeData
        {
            mode = "Relax",
            rows = rows,
            columns = columns,
            cells = new MazeData.CellData[rows, columns],
            start = Vector2Int.zero,
            end = new Vector2Int(0, columns - 1),
            elements = new List<MazeData.ElementData>()
        };

        // Initialize all cells with walls
        for (int x = 0; x < rows; x++)
        {
            for (int y = 0; y < columns; y++)
            {
                mazeData.cells[x, y] = new MazeData.CellData
                {
                    WallRight = true,
                    WallFront = true,
                    WallLeft = true,
                    WallBack = true,
                    IsVisited = false,
                    IsGoal = false,
                    IsStart = false
                };
            }
        }

        // Set start point in a random corner and end point on the opposite side
        SetStartAndEndPoints();

        return mazeData;
    }

    public MazeData GenerateRandom(int rows, int columns)
    {
        mazeData = new MazeData
        {
            mode="Relax",
            rows = rows,
            columns = columns,
            cells = new MazeData.CellData[rows, columns],
            start = Vector2Int.zero,
            end = new Vector2Int(0, columns - 1),
            elements = new List<MazeData.ElementData>()
        };

        // Initialize all cells with walls
        for (int x = 0; x < rows; x++)
        {
            for (int y = 0; y < columns; y++)
            {
                mazeData.cells[x, y] = new MazeData.CellData
                {
                    WallRight = true,
                    WallFront = true,
                    WallLeft = true,
                    WallBack = true,
                    IsVisited = false,
                    IsGoal = false,
                    IsStart = false
                };
            }
        }

        // Generate maze using Kruskal's algorithm
        GenerateMazeKruskal(rows, columns);

        // Set start point in a random corner and end point on the opposite side
        SetStartAndEndPoints();

        return mazeData;
    }

    private void GenerateMazeKruskal(int rows, int columns)
    {
        // List of all walls (edges)
        List<(Vector2Int cell1, Vector2Int cell2, string direction)> walls = new List<(Vector2Int, Vector2Int, string)>();

        // Collect all walls
        for (int x = 0; x < rows; x++)
        {
            for (int y = 0; y < columns; y++)
            {
                // Right wall (except last column)
                if (y < columns - 1)
                {
                    walls.Add((new Vector2Int(x, y), new Vector2Int(x, y + 1), "right"));
                }
                // Front (bottom) wall (except last row)
                if (x < rows - 1)
                {
                    walls.Add((new Vector2Int(x, y), new Vector2Int(x + 1, y), "front"));
                }
            }
        }

        // Shuffle walls
        for (int i = 0; i < walls.Count; i++)
        {
            int randomIndex = Random.Range(i, walls.Count);
            var temp = walls[i];
            walls[i] = walls[randomIndex];
            walls[randomIndex] = temp;
        }

        // Initialize union-find
        int[] parent = new int[rows * columns];
        int[] rank = new int[rows * columns];
        for (int i = 0; i < rows * columns; i++)
        {
            parent[i] = i;
            rank[i] = 0;
        }

        // Process walls
        foreach (var wall in walls)
        {
            Vector2Int cell1 = wall.cell1;
            Vector2Int cell2 = wall.cell2;
            int id1 = cell1.x * columns + cell1.y;
            int id2 = cell2.x * columns + cell2.y;

            if (Find(id1, parent) != Find(id2, parent))
            {
                // Remove wall
                if (wall.direction == "right")
                {
                    mazeData.cells[cell1.x, cell1.y].WallRight = false;
                    mazeData.cells[cell2.x, cell2.y].WallLeft = false;
                }
                else if (wall.direction == "front")
                {
                    mazeData.cells[cell1.x, cell1.y].WallFront = false;
                    mazeData.cells[cell2.x, cell2.y].WallBack = false;
                }

                // Merge components
                Union(id1, id2, parent, rank);
            }
        }
    }

    private int Find(int x, int[] parent)
    {
        if (parent[x] != x)
        {
            parent[x] = Find(parent[x], parent); // Path compression
        }
        return parent[x];
    }

    private void Union(int x, int y, int[] parent, int[] rank)
    {
        int rootX = Find(x, parent);
        int rootY = Find(y, parent);
        if (rootX != rootY)
        {
            // Union by rank
            if (rank[rootX] < rank[rootY])
            {
                parent[rootX] = rootY;
            }
            else if (rank[rootX] > rank[rootY])
            {
                parent[rootY] = rootX;
            }
            else
            {
                parent[rootY] = rootX;
                rank[rootX]++;
            }
        }
    }

    private void SetStartAndEndPoints()
    {
        // Define the four corners
        Vector2Int[] corners = new Vector2Int[]
        {
            new Vector2Int(0, 0),                    // Top-left
            new Vector2Int(0, mazeData.columns - 1), // Top-right
            new Vector2Int(mazeData.rows - 1, 0),    // Bottom-left
            new Vector2Int(mazeData.rows - 1, mazeData.columns - 1) // Bottom-right
        };

        // Randomly select a corner for the start point
        int startCornerIndex = Random.Range(0, 4);
        mazeData.start = corners[startCornerIndex];
        mazeData.cells[mazeData.start.x, mazeData.start.y].IsStart = true;

        // Set the end point on the opposite side
        SetEndPointOppositeStart();
    }

    public void SetEndPointOppositeStart()
    {
        // Clear the previous end point
        for (int x = 0; x < mazeData.rows; x++)
        {
            for (int y = 0; y < mazeData.columns; y++)
            {
                mazeData.cells[x, y].IsGoal = false;
            }
        }

        // Set the end point exactly opposite the start point
        if (mazeData.start == new Vector2Int(0, 0)) // Top-left
        {
            // End at bottom-right
            mazeData.end = new Vector2Int(mazeData.rows - 1, mazeData.columns - 1);
        }
        else if (mazeData.start == new Vector2Int(0, mazeData.columns - 1)) // Top-right
        {
            // End at bottom-left
            mazeData.end = new Vector2Int(mazeData.rows - 1, 0);
        }
        else if (mazeData.start == new Vector2Int(mazeData.rows - 1, 0)) // Bottom-left
        {
            // End at top-right
            mazeData.end = new Vector2Int(0, mazeData.columns - 1);
        }
        else if (mazeData.start == new Vector2Int(mazeData.rows - 1, mazeData.columns - 1)) // Bottom-right
        {
            // End at top-left
            mazeData.end = new Vector2Int(0, 0);
        }

        mazeData.cells[mazeData.end.x, mazeData.end.y].IsGoal = true;
    }
}