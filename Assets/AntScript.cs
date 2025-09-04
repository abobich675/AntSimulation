using System.Collections.Generic;
using UnityEngine;

public class AntScript : MonoBehaviour
{
    private Dictionary<Vector3Int, Hex> hexes;
    public Vector3Int currentHex;

    public void AttachDictionary(Dictionary<Vector3Int, Hex> dict)
    {
        hexes = dict;
    }

    public void DoTick()
    {
        if (hexes == null)
        {
            return;
        }


    }
}
