using System.Collections.Generic;
using UnityEngine;

public class AntScript : MonoBehaviour
{
    private Dictionary<Vector3Int, Hex> hexes;
    public Hex currHex;

    public enum Directions
    {
        Top_Left,
        Left,
        Bottom_Left,
        Top_Right,
        Right,
        Bottom_Right,
    }
    private Directions currDirection;

    void Start()
    {
        currDirection = (Directions)System.Enum.GetValues(typeof(Directions))
            .GetValue(Random.Range(0, System.Enum.GetValues(typeof(Directions)).Length));
    }

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

        Move(ChooseDirection());
    }

    private Directions ChooseDirection()
    {
        int adjustment = Random.Range(-1, 1);
        int length = System.Enum.GetValues(typeof(Directions)).Length;
        Directions newDirection = (Directions)(((int)currDirection + adjustment) % length);
        return newDirection;
    }

    private void Move(Directions dir)
    {
        if (currHex == null)
            return;

        Hex targetHex;

        switch (dir)
        {
            case Directions.Top_Left: targetHex = currHex.tl; break;
            case Directions.Left: targetHex = currHex.l; break;
            case Directions.Bottom_Left: targetHex = currHex.bl; break;
            case Directions.Top_Right: targetHex = currHex.tr; break;
            case Directions.Right: targetHex = currHex.r; break;
            case Directions.Bottom_Right: targetHex = currHex.br; break;
            default: return;
        }

        if (targetHex != null)
        {
            currHex = targetHex;
            transform.position = targetHex.GetWorldPos();
        }
        else
        {
            int adjustment = 3;
            int length = System.Enum.GetValues(typeof(Directions)).Length;
            currDirection = (Directions)(((int)currDirection + adjustment) % length);
            Debug.Log("Failed to Move");
        }
    }
}
