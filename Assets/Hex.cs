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
        Food,
    }

    static public Dictionary<PheromoneType, int> MAX_PHEROMONES = new Dictionary<PheromoneType, int>
    {
        { PheromoneType.Exploration, 300 },
        { PheromoneType.Forage, 1 },
        { PheromoneType.Food, 100 },
    };

    static public Dictionary<PheromoneType, float> SPREAD_PHEROMONES = new Dictionary<PheromoneType, float>
    {
        { PheromoneType.Exploration, 0.0005f },
        { PheromoneType.Forage, 0.001f },
        { PheromoneType.Food, 0.01f },
    };

    // Variables
    public Dictionary<Directions, Hex> neighbors = new();
    public Vector3Int cellPos;
    public Tilemap tileMap;

    private Dictionary<PheromoneType, int> pheromones = new Dictionary<PheromoneType, int>();
    public int foodValue;

    private Dictionary<PheromoneType, int> changes = new();

    // Constructor
    public Hex()
    {
        // initialize all pheromone types with 0
        foreach (PheromoneType type in Enum.GetValues(typeof(PheromoneType)))
        {
            pheromones[type] = 0;
            changes[type] = 0;
        }
    }

    // Methods
    public int GetPheromone(PheromoneType type)
    {
        return pheromones[type];
    }

    public void SetPheromone(PheromoneType type, int value)
    {
        changes[type] = value - pheromones[type];
    }

    public void AddPheromone(PheromoneType type, int amount)
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

    private void SpreadPheromones()
    {

        foreach (PheromoneType type in Enum.GetValues(typeof(PheromoneType)))
        {
            int spread = (int)Math.Floor(pheromones[type] * SPREAD_PHEROMONES[type]);

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
        green = Math.Max(Math.Min(green, 1), 0.3f);
        float blue = GetPheromone(PheromoneType.Forage) / (float)MAX_PHEROMONES[PheromoneType.Forage] * 0.75f;
        blue = Math.Max(Math.Min(blue, 1), 0.3f);

        tileMap.SetColor(cellPos, new Color(red, green, blue));
    }

    public void DoTick()
    {
        SpreadPheromones();
        HandleFood();
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
        
        }
        HandleColor();
    }
}