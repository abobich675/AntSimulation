using System.Collections.Generic;
using UnityEngine;

public class AntScript : MonoBehaviour
{
    private Dictionary<Vector3Int, Hex> hexes;

    public void AttachDictionary(Dictionary<Vector3Int, Hex> dict)
    {
        hexes = dict;
    }
    
    void FixedUpdate()
    {
        
    }
}
