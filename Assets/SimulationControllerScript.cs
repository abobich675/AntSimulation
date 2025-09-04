using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

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
        foreach (var pos in bounds.allPositionsWithin)
        {
            if (tileMap.HasTile(pos))
            {
                Hex hex = new Hex { cellPos = pos };
                hexes[pos] = hex;
            }
        }

        // 2. Hook up neighbors
        foreach (var kvp in hexes)
        {
            Vector3Int pos = kvp.Key;
            Hex hex = kvp.Value;

            // Offset coordinates depend on your hex layout
            if (tileMap.cellLayout == GridLayout.CellLayout.Hexagon)
            {
                bool oddRow = (pos.y & 1) != 0;

                Vector3Int[] neighborOffsets = oddRow
                    ? new[] {
                        new Vector3Int(-1,  1, 0), // tl
                        new Vector3Int(-1,  0, 0), // l
                        new Vector3Int(-1, -1, 0), // bl
                        new Vector3Int( 0,  1, 0), // tr
                        new Vector3Int( 1,  0, 0), // r
                        new Vector3Int( 0, -1, 0), // br
                    }
                    : new[] {
                        new Vector3Int( 0,  1, 0), // tl
                        new Vector3Int(-1,  0, 0), // l
                        new Vector3Int( 0, -1, 0), // bl
                        new Vector3Int( 1,  1, 0), // tr
                        new Vector3Int( 1,  0, 0), // r
                        new Vector3Int( 1, -1, 0), // br
                    };

                if (hexes.TryGetValue(pos + neighborOffsets[0], out var tl)) hex.tl = tl;
                if (hexes.TryGetValue(pos + neighborOffsets[1], out var l)) hex.l = l;
                if (hexes.TryGetValue(pos + neighborOffsets[2], out var bl)) hex.bl = bl;
                if (hexes.TryGetValue(pos + neighborOffsets[3], out var tr)) hex.tr = tr;
                if (hexes.TryGetValue(pos + neighborOffsets[4], out var r)) hex.r = r;
                if (hexes.TryGetValue(pos + neighborOffsets[5], out var br)) hex.br = br;
            }
        }

        Debug.Log($"Built {hexes.Count} hexes from the tilemap");
    }

    public Vector3 GetHexWorldPos(Hex hex)
    {
        return tileMap.CellToWorld(hex.cellPos);
    }

    // Spawn ants as spawn location up to global constant NUM_ANTS
    private void SpawnAnts()
    {
        StartCoroutine(SpawnAntsCoroutine());
    }

    private IEnumerator SpawnAntsCoroutine()
    {
        Vector3Int[] keys = hexes.Keys.ToArray();
        Vector3Int spawnPos = keys[Random.Range(0, keys.Length)];

        for (int i = 0; i < NUM_ANTS; i++)
        {
            GameObject newAnt = Instantiate(AntPrefab);
            newAnt.transform.position = GetHexWorldPos(hexes[spawnPos]);;
            AntScript script = newAnt.GetComponent<AntScript>();
            script.AttachDictionary(hexes);
            script.currentHex = spawnPos;
            ants.Append(script);

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
            yield return new WaitForSeconds(TICK_RATE);
        }
    }

    void Tick()
    {
        PheromoneDecay();

        foreach (AntScript ant in ants)
        {
            ant.DoTick();
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
