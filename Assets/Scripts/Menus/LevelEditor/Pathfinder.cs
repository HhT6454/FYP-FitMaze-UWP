using UnityEngine;
using System.Collections.Generic;

public class Pathfinder : MonoBehaviour
{
    public (List<Vector2Int> path, int pathLength, int turns) FindPath(MazeData mazeData)
    {
        if (mazeData == null || mazeData.cells == null || mazeData.start == null || mazeData.end == null)
        {
            Debug.LogWarning("Cannot find path: Invalid maze data or start/end points.");
            return (null, 0, 0);
        }

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> parent = new Dictionary<Vector2Int, Vector2Int>();
        Vector2Int start = mazeData.start;
        Vector2Int end = mazeData.end;

        queue.Enqueue(start);
        visited.Add(start);
        parent[start] = start; // Start has no parent

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            if (current == end) break;

            foreach (Vector2Int neighbor in GetValidNeighbors(mazeData, current))
            {
                if (!visited.Contains(neighbor))
                {
                    queue.Enqueue(neighbor);
                    visited.Add(neighbor);
                    parent[neighbor] = current;
                }
            }
        }

        if (!visited.Contains(end))
        {
            return (null, 0, 0); // No path found
        }

        // Reconstruct path
        List<Vector2Int> path = new List<Vector2Int>();
        Vector2Int pathCurrent = end; // Renamed to avoid conflict
        while (pathCurrent != start)
        {
            path.Add(pathCurrent);
            pathCurrent = parent[pathCurrent];
        }
        path.Add(start);
        path.Reverse();

        int pathLength = path.Count;
        int turns = 0;
        // Count turns by checking direction changes
        for (int i = 1; i < pathLength - 1; i++)
        {
            Vector2Int prev = path[i - 1];
            Vector2Int curr = path[i];
            Vector2Int next = path[i + 1];
            Vector2Int dir1 = curr - prev;
            Vector2Int dir2 = next - curr;
            if (dir1 != dir2) turns++;
        }

        return (path, pathLength, turns);
    }

    public bool CheckAccessibleCells(MazeData mazeData)
    {
        if (mazeData == null || mazeData.cells == null || mazeData.start == null)
        {
            Debug.LogWarning("Cannot check accessibility: Invalid maze data or start point.");
            return false;
        }

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        Vector2Int start = mazeData.start;

        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            foreach (Vector2Int neighbor in GetValidNeighbors(mazeData, current))
            {
                if (!visited.Contains(neighbor))
                {
                    queue.Enqueue(neighbor);
                    visited.Add(neighbor);
                }
            }
        }

        int totalCells = mazeData.rows * mazeData.columns;
        return visited.Count == totalCells;
    }

    private List<Vector2Int> GetValidNeighbors(MazeData mazeData, Vector2Int cell)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        int x = cell.x, y = cell.y;

        // Up (Back)
        if (x > 0 && !mazeData.cells[x, y].WallBack)
        {
            neighbors.Add(new Vector2Int(x - 1, y));
        }
        // Right
        if (y < mazeData.columns - 1 && !mazeData.cells[x, y].WallRight)
        {
            neighbors.Add(new Vector2Int(x, y + 1));
        }
        // Down (Front)
        if (x < mazeData.rows - 1 && !mazeData.cells[x, y].WallFront)
        {
            neighbors.Add(new Vector2Int(x + 1, y));
        }
        // Left
        if (y > 0 && !mazeData.cells[x, y].WallLeft)
        {
            neighbors.Add(new Vector2Int(x, y - 1));
        }

        return neighbors;
    }
}