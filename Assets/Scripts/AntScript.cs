using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
using Directions = Hex.Directions;
using PheromoneType = Hex.PheromoneType;

public class AntScript : MonoBehaviour
{
    private Dictionary<Vector3Int, Hex> hexes;
    public float carrying_capacity;

    // State
    private Hex currHex;
    private Directions currDirection;
    private float foodCarried;
    private PheromoneType pheromoneMode;
    
    void Start()
    {
        currDirection = (Directions)Enum.GetValues(typeof(Directions))
            .GetValue(Random.Range(0, Enum.GetValues(typeof(Directions)).Length));

        foodCarried = 0;
    }

    public void Initialize(Dictionary<Vector3Int, Hex> dict, Hex spawnHex)
    {
        hexes = dict;
        currHex = spawnHex;
    }

    private float EvaluateHex(Hex hex)
    {
        if (hex == null)
        {
            return 0;
        }

        float score = 1;

        switch (pheromoneMode)
        {
            case PheromoneType.Exploration:
                // Don't go where you've already explored
                float explorationCount = Math.Max(hex.GetPheromone(PheromoneType.Exploration), 1);
                score *= Hex.MAX_PHEROMONES[PheromoneType.Exploration] / explorationCount;

                // Avoid home
                if (hex.isAnthill) score /= 10;

                // Go towards forage
                float forageCount = Math.Max(hex.GetPheromone(PheromoneType.Forage), 1);
                score *= (float)Math.Pow(forageCount, 2) / Hex.MAX_PHEROMONES[PheromoneType.Forage];

                // Do go towards food
                float foodCount = hex.GetPheromone(PheromoneType.Food) + 1;
                score *= (float)Math.Pow(foodCount, 5) / Hex.MAX_PHEROMONES[PheromoneType.Food];

                float isFood = hex.GetFood() + 1;
                score *= (float)Math.Pow(isFood, 5) / Hex.MAX_PHEROMONES[PheromoneType.Food];
                break;
            
            case PheromoneType.Forage:
                // Follow your path home
                explorationCount = Math.Max(hex.GetPheromone(PheromoneType.Exploration), 1);
                score *= explorationCount / Hex.MAX_PHEROMONES[PheromoneType.Exploration];

                // Go home if available
                float hillCount = hex.GetPheromone(PheromoneType.Hill) + 1;
                score *= (float)Math.Pow(hillCount, 2) / Hex.MAX_PHEROMONES[PheromoneType.Hill];
                if (hex.isAnthill) score *= 10000;

                break;

            default:
                break;
        }

        // Boost Score for generally moving forwards
        int length = Enum.GetValues(typeof(Directions)).Length;
        Directions leftTurn = (Directions)(((int)currDirection - 1 + length) % length);
        Directions rightTurn = (Directions)(((int)currDirection + 1 + length) % length);
        if (currHex.neighbors[currDirection] == hex)
            score *= 10f;
        else if (currHex.neighbors[leftTurn] == hex || currHex.neighbors[rightTurn] == hex)
            score *= 2f;
        else
            score /= 10;

        return score;
    }

    private Directions ChooseDirection()
    {
        Dictionary<Directions, float> options =
            ((Directions[])Enum.GetValues(typeof(Directions)))
            .ToDictionary(d => d, d => 1f);

        float totalScore = 0;
        foreach (Directions dir in (Directions[])Enum.GetValues(typeof(Directions)))
        {
            options[dir] = EvaluateHex(currHex.neighbors[dir]);
            totalScore += options[dir];
        }

        float chosenDirFloat = Random.Range(0, totalScore);
        Directions chosenDirection = Directions.Top_Right;
        float scoreCount = 0;
        foreach (Directions dir in (Directions[])Enum.GetValues(typeof(Directions)))
        {
            scoreCount += options[dir];
            if (scoreCount > chosenDirFloat)
            {
                chosenDirection = dir;
                break;
            }
        }

        // Debug.Log("Options: " + string.Join(", ", options.Select(kvp => $"{kvp.Key}:{kvp.Value}")));
        // Debug.Log("Chosen: " + chosenDirFloat + ", " + chosenDirection);

        return chosenDirection;
    }

    private void Move(Directions dir)
    {
        if (currHex == null)
            return;

        Hex targetHex = currHex.neighbors[dir];

        if (targetHex == null)
        {
            currDirection = dir;
        }
        else
        {
            currHex = targetHex;
            transform.position = targetHex.GetWorldPos();
            currDirection = dir;
        }
    }

    public void DoTick()
    {
        if (hexes == null)
        {
            return;
        }

        Move(ChooseDirection());
        transform.rotation = Quaternion.Euler(0, 0, -30 - (int)currDirection * 60);
        currHex.SetPheromone(pheromoneMode, Hex.MAX_PHEROMONES[pheromoneMode]);

        if (pheromoneMode == PheromoneType.Exploration && currHex.GetFood() > 0)
        {
            foodCarried = (currHex.GetFood() >= carrying_capacity) ? carrying_capacity : currHex.GetFood();
            currHex.AddFood(-foodCarried);
            pheromoneMode = PheromoneType.Forage;
        }
        else if (pheromoneMode == PheromoneType.Forage && currHex.isAnthill)
        {
            foodCarried = 0;
            pheromoneMode = PheromoneType.Exploration;
        }

    }
}
