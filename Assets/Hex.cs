using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Hex
{
    // Enums
    public enum Directions
    {
        Top_Right,
        Right,
        Bottom_Right,
        Bottom_Left,
        Left,
        Top_Left
    }

    public enum PheromoneType
    {
        Exploration,
        Forage,
    }

    static public Dictionary<PheromoneType, int> MAX_PHEROMONES = new Dictionary<PheromoneType, int>
        {
            { PheromoneType.Exploration, 400 },
            { PheromoneType.Forage, 1 },
        };

    // Variables
    public Dictionary<Directions, Hex> neighbors = new();
    public Vector3Int cellPos;
    public Tilemap tileMap;

    private Dictionary<PheromoneType, int> pheromones = new Dictionary<PheromoneType, int>();

    // Constructor
    public Hex()
    {
        // initialize all pheromone types with 0
        foreach (PheromoneType type in Enum.GetValues(typeof(PheromoneType)))
        {
            pheromones[type] = 0;
        }
    }

    // Methods
    public int GetPheromone(PheromoneType type)
    {
        return pheromones[type];
    }

    public void SetPheromone(PheromoneType type, int value)
    {
        pheromones[type] = value;
    }

    public void AddPheromone(PheromoneType type, int amount)
    {
        pheromones[type] += amount;
        if (pheromones[type] < 0)
        {
            pheromones[type] = 0;
        }

        if (!tileMap.HasTile(cellPos)) {
            Debug.LogWarning($"No tile found at {cellPos}");
        }
        float red = GetPheromone(PheromoneType.Exploration) / (float)MAX_PHEROMONES[PheromoneType.Exploration] * 0.75f;
        red = Math.Max( Math.Min(red, 1), 0.3f );
        tileMap.SetColor(cellPos, new Color(red, 0.3f, 0.3f));
    }

    public Vector3 GetWorldPos()
    {
        return tileMap.CellToWorld(cellPos);
    }
}