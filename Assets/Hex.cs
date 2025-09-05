using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Hex
{
    // Variables
    public Hex tl, l, bl, tr, r, br; // Neighbors
    public Vector3Int cellPos;
    public Tilemap tileMap;

    public enum PheromoneType
    {
        Exploration,
        Forage,
    }

    private Dictionary<PheromoneType, int> pheromones = new Dictionary<PheromoneType, int>();

    // Constructor
    public Hex()
    {
        // initialize all pheromone types with 0
        foreach (PheromoneType type in System.Enum.GetValues(typeof(PheromoneType)))
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
        float red = Math.Min(GetPheromone(PheromoneType.Exploration) / 400f, 1);
        red = Math.Max(red, 0.3f);
        tileMap.SetColor(cellPos, new Color(red, 0.3f, 0.3f));
    }

    public Vector3 GetWorldPos()
    {
        return tileMap.CellToWorld(cellPos);
    }
}