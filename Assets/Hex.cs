using System.Collections.Generic;
using UnityEngine;

public class Hex
{
    // Variables
    public Hex tl, l, bl, tr, r, br; // Neighbors
    public Vector3Int cellPos;

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

    // Functions
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
    }
}