using UnityEngine;
using System.Collections.Generic;

public static class AStarPathfinder
{
    private class Node
    {
        public Vector2Int cell;
        public Node parent;
        public float g;
        public float h;
        public float f => g + h;

        public Node(Vector2Int cell, Node parent, float g, float h)
        {
            this.cell = cell;
            this.parent = parent;
            this.g = g;
            this.h = h;
        }
    }

    public static List<Vector3> FindPath(
        Vector3 startWorld,
        Vector3 endWorld,
        Building[,] grid,
        Vector3 gridOrigin,
        float cellSize,
        Vector2Int gridSize)
    {
        Vector2Int startCell = WorldToCell(startWorld, gridOrigin, cellSize, gridSize);
        Vector2Int endCell = WorldToCell(endWorld, gridOrigin, cellSize, gridSize);

        endCell = FindNearestFreeCell(endCell, grid, gridSize);

        if (startCell == endCell)
            return new List<Vector3> { endWorld };

        var openSet = new List<Node>();
        var closedSet = new HashSet<Vector2Int>();

        openSet.Add(new Node(startCell, null, 0, Heuristic(startCell, endCell)));

        while (openSet.Count > 0)
        {
            Node current = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
                if (openSet[i].f < current.f) current = openSet[i];

            if (current.cell == endCell)
                return BuildPath(current, gridOrigin, cellSize);

            openSet.Remove(current);
            closedSet.Add(current.cell);

            foreach (var neighbor in GetNeighbors(current.cell, gridSize))
            {
                if (closedSet.Contains(neighbor)) continue;
                if (!IsCellFree(neighbor, grid, gridSize))
                {
                    closedSet.Add(neighbor);
                    continue;
                }

                bool isDiagonal = neighbor.x != current.cell.x && neighbor.y != current.cell.y;

                if (isDiagonal)
                {
                    var side1 = new Vector2Int(current.cell.x, neighbor.y);
                    var side2 = new Vector2Int(neighbor.x, current.cell.y);
                    if (!IsCellFree(side1, grid, gridSize) || !IsCellFree(side2, grid, gridSize))
                        continue;
                }

                float moveCost = isDiagonal ? 1.414f : 1f;
                float g = current.g + moveCost;
                float h = Heuristic(neighbor, endCell);

                Node existing = openSet.Find(n => n.cell == neighbor);
                if (existing == null)
                    openSet.Add(new Node(neighbor, current, g, h));
                else if (g < existing.g)
                {
                    existing.parent = current;
                    existing.g = g;
                }
            }
        }

        return new List<Vector3> { endWorld };
    }

    public static bool HasLineOfSight(
        Vector3 startWorld,
        Vector3 endWorld,
        Building[,] grid,
        Vector3 gridOrigin,
        float cellSize,
        Vector2Int gridSize)
    {
        Vector2Int startCell = WorldToCell(startWorld, gridOrigin, cellSize, gridSize);
        Vector2Int endCell = WorldToCell(endWorld, gridOrigin, cellSize, gridSize);

        int x = startCell.x;
        int y = startCell.y;
        int dx = Mathf.Abs(endCell.x - startCell.x);
        int dy = Mathf.Abs(endCell.y - startCell.y);
        int sx = startCell.x < endCell.x ? 1 : -1;
        int sy = startCell.y < endCell.y ? 1 : -1;
        int err = dx - dy;

        int maxSteps = dx + dy + 1;

        for (int i = 0; i < maxSteps; i++)
        {
            if (!IsCellFree(new Vector2Int(x, y), grid, gridSize))
                return false;

            if (x == endCell.x && y == endCell.y)
                return true;

            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x += sx; }
            if (e2 < dx) { err += dx; y += sy; }
        }

        return true;
    }

    static List<Vector3> BuildPath(Node end, Vector3 gridOrigin, float cellSize)
    {
        var path = new List<Vector3>();
        var node = end;

        while (node != null)
        {
            path.Add(CellToWorld(node.cell, gridOrigin, cellSize));
            node = node.parent;
        }

        path.Reverse();
        return path;
    }

    static IEnumerable<Vector2Int> GetNeighbors(Vector2Int cell, Vector2Int gridSize)
    {
        var dirs = new Vector2Int[]
        {
            new Vector2Int( 1,  1),
            new Vector2Int(-1,  1),
            new Vector2Int( 1, -1),
            new Vector2Int(-1, -1),
            new Vector2Int( 0,  1),
            new Vector2Int( 0, -1),
            new Vector2Int( 1,  0),
            new Vector2Int(-1,  0),
        };

        foreach (var dir in dirs)
        {
            var neighbor = cell + dir;
            if (neighbor.x >= 0 && neighbor.x < gridSize.x &&
                neighbor.y >= 0 && neighbor.y < gridSize.y)
                yield return neighbor;
        }
    }

    static bool IsCellFree(Vector2Int cell, Building[,] grid, Vector2Int gridSize)
    {
        if (cell.x < 0 || cell.x >= gridSize.x) return false;
        if (cell.y < 0 || cell.y >= gridSize.y) return false;
        return grid[cell.x, cell.y] == null;
    }

    static Vector2Int FindNearestFreeCell(Vector2Int target, Building[,] grid, Vector2Int gridSize)
    {
        if (IsCellFree(target, grid, gridSize)) return target;

        for (int radius = 1; radius < 5; radius++)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    var candidate = new Vector2Int(target.x + dx, target.y + dy);
                    if (IsCellFree(candidate, grid, gridSize)) return candidate;
                }
            }
        }

        return target;
    }

    static float Heuristic(Vector2Int a, Vector2Int b)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);
        return 1f * (dx + dy) + (1.414f - 2f) * Mathf.Min(dx, dy);
    }

    static Vector2Int WorldToCell(Vector3 world, Vector3 gridOrigin, float cellSize, Vector2Int gridSize)
    {
        int x = Mathf.FloorToInt((world.x - gridOrigin.x) / cellSize);
        int y = Mathf.FloorToInt((world.z - gridOrigin.z) / cellSize);
        x = Mathf.Clamp(x, 0, gridSize.x - 1);
        y = Mathf.Clamp(y, 0, gridSize.y - 1);
        return new Vector2Int(x, y);
    }

    static Vector3 CellToWorld(Vector2Int cell, Vector3 gridOrigin, float cellSize)
    {
        return gridOrigin + new Vector3(
            (cell.x + 0.5f) * cellSize,
            0,
            (cell.y + 0.5f) * cellSize
        );
    }
}