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
    public Hex currHex;
    private Directions currDirection;
    public PheromoneType pheromoneMode;

    void Start()
    {
        currDirection = (Directions)Enum.GetValues(typeof(Directions))
            .GetValue(Random.Range(0, Enum.GetValues(typeof(Directions)).Length));
    }

    public void AttachDictionary(Dictionary<Vector3Int, Hex> dict)
    {
        hexes = dict;
    }

    private float EvaluateHex(Hex hex)
    {
        if (hex == null)
        {
            return 0;
        }

        float score = 1;

        if (pheromoneMode == PheromoneType.Exploration)
        {
            float explorationCount = hex.GetPheromone(PheromoneType.Exploration);
            score *= Hex.MAX_PHEROMONES[PheromoneType.Exploration] / (float)Math.Pow(Math.Max(explorationCount, 1), 2);
        }
        
        int length = Enum.GetValues(typeof(Directions)).Length;
        Directions leftTurn = (Directions)(((int)currDirection - 1 + length) % length);
        Directions rightTurn = (Directions)(((int)currDirection + 1 + length) % length);
        if (currHex.neighbors[leftTurn] == hex ||
            currHex.neighbors[rightTurn] == hex ||
            currHex.neighbors[currDirection] == hex)
        {
            score *= 1.5f;
        }

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
            // Move(ChooseDirection());
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
        currHex.SetPheromone(pheromoneMode, Hex.MAX_PHEROMONES[PheromoneType.Exploration]);

    }
}
