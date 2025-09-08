using System;
using System.Collections.Generic;
using Unity.VisualScripting;
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
        Food,
        Hill,
    }

    static public Dictionary<PheromoneType, float> MAX_PHEROMONES = new Dictionary<PheromoneType, float>
    {
        { PheromoneType.Exploration, 1000 },
        { PheromoneType.Forage, 1000 },
        { PheromoneType.Food, 100 },
        { PheromoneType.Hill, 100000 },
    };

    static public Dictionary<PheromoneType, float> SPREAD_PHEROMONES = new Dictionary<PheromoneType, float>
    {
        { PheromoneType.Exploration, 0.001f },
        { PheromoneType.Forage, 0.001f },
        { PheromoneType.Food, 0.1f },
        { PheromoneType.Hill, 0.15f },
    };

    // Variables
    public Dictionary<Directions, Hex> neighbors = new();
    public Vector3Int cellPos;
    public Tilemap tileMap;

    private Dictionary<PheromoneType, float> pheromones = new();
    public float foodValue;
    public bool isAnthill;

    private Dictionary<PheromoneType, float> changes = new();

    // Constructor
    public Hex(Tilemap tileMap, Vector3Int cellPos, float foodValue)
    {
        this.tileMap = tileMap;
        this.cellPos = cellPos;
        this.foodValue = foodValue;

        // Initialize all pheromone types with 0
        foreach (PheromoneType type in Enum.GetValues(typeof(PheromoneType)))
        {
            pheromones[type] = 0;
            changes[type] = 0;
        }

        // Initialize neighbors to null
        Directions[] dirs = (Directions[])System.Enum.GetValues(typeof(Directions));
        for (int i = 0; i < dirs.Length; i++)
        {
            neighbors[dirs[i]] = null;
        }
    }

    // Methods
    public float GetPheromone(PheromoneType type)
    {
        return pheromones[type];
    }

    public void SetPheromone(PheromoneType type, float value)
    {
        changes[type] = value - pheromones[type];
    }

    public void AddPheromone(PheromoneType type, float amount)
    {
        changes[type] += amount;
        if (!tileMap.HasTile(cellPos))
        {
            Debug.LogWarning($"No tile found at {cellPos}");
        }
    }

    public Vector3 GetWorldPos()
    {
        return tileMap.CellToWorld(cellPos);
    }

    private void HandleFood()
    {
        if (foodValue > 0)
        {
            SetPheromone(PheromoneType.Food, foodValue);
            AddPheromone(PheromoneType.Food, 1);
        }
    }

    private void HandleHill()
    {
        if (isAnthill)
        {
            SetPheromone(PheromoneType.Hill, MAX_PHEROMONES[PheromoneType.Hill]);
            AddPheromone(PheromoneType.Hill, 1);
        }
    } 

    private void SpreadPheromones()
    {

        foreach (PheromoneType type in Enum.GetValues(typeof(PheromoneType)))
        {
            float spread = pheromones[type] * SPREAD_PHEROMONES[type];

            foreach (var kvp in neighbors)
            {
                Hex hex = kvp.Value;
                if (hex == null)
                {
                    continue;
                }

                hex.AddPheromone(type, spread);
                AddPheromone(type, -spread);

            }
        }
    }

    private void HandleColor()
    {
        float red = GetPheromone(PheromoneType.Exploration) / (float)MAX_PHEROMONES[PheromoneType.Exploration] * 0.75f;
        red = Math.Max(Math.Min(red, 1), 0.3f);
        float green = GetPheromone(PheromoneType.Food) / (float)MAX_PHEROMONES[PheromoneType.Food] * 0.75f;
        green += GetPheromone(PheromoneType.Hill) / (float)MAX_PHEROMONES[PheromoneType.Hill] * 0.75f;
        green = Math.Max(Math.Min(green, 1), 0.3f);
        float blue = GetPheromone(PheromoneType.Forage) / (float)MAX_PHEROMONES[PheromoneType.Forage] * 0.75f;
        blue = Math.Max(Math.Min(blue, 1), 0.3f);

        tileMap.SetColor(cellPos, new Color(red, green, blue));
    }

    public void DoTick()
    {
        SpreadPheromones();
        HandleFood();
        HandleHill();
    }

    public void ApplyTick()
    {
        foreach (PheromoneType type in Enum.GetValues(typeof(PheromoneType)))
        {
            pheromones[type] += changes[type];
            if (pheromones[type] < 0)
            {
                pheromones[type] = 0;
            }

            if (pheromones[type] > MAX_PHEROMONES[type])
            {
                pheromones[type] = MAX_PHEROMONES[type];
            }

            changes[type] = 0;
        }
        HandleColor();
    }
}