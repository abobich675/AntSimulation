using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using UnityEngine.Tilemaps;
using Directions = Hex.Directions;

public class SimulationControllerScript : MonoBehaviour
{
    public int NUM_ANTS;
    public float SPAWN_DELAY;
    public float TICK_RATE;
    public float CARRYING_CAPACITY;
    public int ANTHILL_SIZE;


    public GameObject AntPrefab;
    public Tile anthillTile;
    public Tilemap foodMap;
    public Tilemap anthillMap;
    public Tilemap tileMap;

    private Dictionary<Vector3Int, Hex> hexes = new();
    private List<AntScript> ants = new();
    private List<Hex> anthillHexes = new();

    // Create a Hex object for every filled tile in the tileMap. Store in hexes
    private void BuildGridFromTilemap()
    {
        hexes.Clear();

        // 1. Scan all cells in the tilemap bounds
        BoundsInt bounds = tileMap.cellBounds;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cellPosition = new Vector3Int(x, y, 0);
                TileBase tile = tileMap.GetTile(cellPosition);

                if (tile != null) // Only add if a tile exists at this position
                {
                    // Check for food
                    TileBase foodTile = foodMap.GetTile(cellPosition);
                    float foodValue = (foodTile != null) ? Hex.MAX_PHEROMONES[Hex.PheromoneType.Food] : 0;

                    // Create Hex
                    Hex hex = new Hex(tileMap, cellPosition, foodValue);
                    hexes[cellPosition] = hex;
                }
            }
        }

        // 2. Hook up neighbors
        foreach (var kvp in hexes)
        {
            Vector3Int pos = kvp.Key;
            Hex hex = kvp.Value;

            if (tileMap.cellLayout == GridLayout.CellLayout.Hexagon)
            {
                bool oddRow = (pos.y & 1) != 0;

                Vector3Int[] offsets = oddRow
                    ? new[]
                    {
                        new Vector3Int(1, 1, 0),  // TR
                        new Vector3Int(1, 0, 0),  // R
                        new Vector3Int(1, -1, 0),  // BR
                        new Vector3Int(0, -1, 0), // BL
                        new Vector3Int(-1, 0, 0), // L
                        new Vector3Int(0, 1, 0), // TL
                    }
                    : new[]
                    {
                        new Vector3Int(0, 1, 0),  // TR
                        new Vector3Int(1, 0, 0), // R
                        new Vector3Int(0, -1, 0),  // BR
                        new Vector3Int(-1, -1, 0), // BL
                        new Vector3Int(-1, 0, 0), // L
                        new Vector3Int(-1, 1, 0), // TL
                    };

                Directions[] dirs = (Directions[])System.Enum.GetValues(typeof(Directions));

                for (int i = 0; i < dirs.Length; i++)
                {
                    if (hexes.TryGetValue(pos + offsets[i], out Hex neighbor))
                    {
                        hex.neighbors[dirs[i]] = neighbor;
                    }
                }
            }
        }

        Debug.Log($"Built {hexes.Count} hexes from the tilemap");
    }

    private void PlaceAnthill()
    {
        Vector3Int[] keys = hexes.Keys.ToArray();
        Hex anthillHex = hexes[keys[Random.Range(0, keys.Length)]];
        anthillHex.isAnthill = true;
        anthillHexes.Add(anthillHex);
        anthillMap.SetTile(anthillHex.cellPos, anthillTile);

        // Expand anthill
        for (int i = 0; i < ANTHILL_SIZE; i++)
        {
            List<Hex> neighborsToAdd = new();
            foreach (Hex hex in anthillHexes)
            {
                foreach (var kvp in hex.neighbors)
                {
                    Hex neighbor = kvp.Value;
                    if (neighbor == null) continue;

                    if (!anthillHexes.Contains(neighbor) && !neighborsToAdd.Contains(neighbor))
                    {
                        neighbor.isAnthill = true;
                        anthillMap.SetTile(hex.cellPos, anthillTile);
                        neighborsToAdd.Add(neighbor);
                    }
                }
            }
            anthillHexes.AddRange(neighborsToAdd.Except(anthillHexes));
        }


    }

    // Spawn ants as spawn location up to global constant NUM_ANTS
    private void SpawnAnts()
    {
        StartCoroutine(SpawnAntsCoroutine());
    }

    private IEnumerator SpawnAntsCoroutine()
    {
        for (int i = 0; i < NUM_ANTS; i++)
        {
            Hex spawnHex = anthillHexes[Random.Range(0, anthillHexes.Count - 1)];
            GameObject newAnt = Instantiate(AntPrefab);
            newAnt.transform.position = spawnHex.GetWorldPos();
            AntScript script = newAnt.GetComponent<AntScript>();
            script.AttachDictionary(hexes);
            script.currHex = spawnHex;
            script.carrying_capacity = CARRYING_CAPACITY;
            ants.Add(script);

            yield return new WaitForSeconds(SPAWN_DELAY); // waits before next spawn
        }
    }

    // Decrease the amount of pheromone on a given tile by 1
    void PheromoneDecay()
    {
        foreach (var kvp in hexes)
        {
            Hex hex = kvp.Value;
            foreach (Hex.PheromoneType type in System.Enum.GetValues(typeof(Hex.PheromoneType)))
            {
                if (type == Hex.PheromoneType.Food) continue;
                hex.AddPheromone(type, -1);
            }
        }
    }
    
    // Move to next tick
    private IEnumerator SimulationLoop()
    {
        while (true)
        {
            Tick(); // your update logic
            yield return new WaitForSeconds(1/TICK_RATE);
        }
    }

    void Tick()
    {

        foreach (AntScript ant in ants)
        {
            ant.DoTick();
        }

        foreach (var kvp in hexes)
        {
            Hex hex = kvp.Value;
            hex.DoTick();
        }

        PheromoneDecay();

        foreach (var kvp in hexes)
        {
            Hex hex = kvp.Value;
            hex.ApplyTick();
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        BuildGridFromTilemap();
        PlaceAnthill();
        StartCoroutine(SimulationLoop());
        SpawnAnts();
    }
}
