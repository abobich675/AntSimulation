using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using Directions = Hex.Directions;

public class SimulationControllerScript : MonoBehaviour
{
    public int NUM_ANTS;
    public float SPAWN_DELAY;
    public float TICK_RATE;


    public GameObject AntPrefab;
    public Tilemap tileMap;


    private Dictionary<Vector3Int, Hex> hexes = new Dictionary<Vector3Int, Hex>();
    private List<AntScript> ants = new List<AntScript>();

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
                    Hex hex = new Hex { cellPos = cellPosition, tileMap = tileMap };
                    hexes[cellPosition] = hex;

                    // Attach null neighbors
                    Directions[] dirs = (Directions[])System.Enum.GetValues(typeof(Directions));
                    for (int i = 0; i < dirs.Length; i++)
                    {
                        hex.neighbors[dirs[i]] = null;
                    }
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

    // Spawn ants as spawn location up to global constant NUM_ANTS
    private void SpawnAnts()
    {
        StartCoroutine(SpawnAntsCoroutine());
    }

    private IEnumerator SpawnAntsCoroutine()
    {
        Vector3Int[] keys = hexes.Keys.ToArray();
        Hex spawnHex = hexes[keys[Random.Range(0, keys.Length)]];

        for (int i = 0; i < NUM_ANTS; i++)
        {
            GameObject newAnt = Instantiate(AntPrefab);
            newAnt.transform.position = spawnHex.GetWorldPos();
            AntScript script = newAnt.GetComponent<AntScript>();
            script.AttachDictionary(hexes);
            script.currHex = spawnHex;
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
        PheromoneDecay();

        foreach (AntScript ant in ants)
        {
            ant.DoTick();
        }

        foreach (var kvp in hexes)
        {
            Hex hex = kvp.Value;
            hex.DoTick();
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        BuildGridFromTilemap();
        StartCoroutine(SimulationLoop());
        SpawnAnts();
    }
}
