using System.Collections.Generic;
using UnityEngine;

public class AntScript : MonoBehaviour
{
    public int multiplier;
    private Dictionary<Vector3Int, Hex> hexes;
    public Hex currHex;

    public enum Directions
    {
        Top_Right,
        Right,
        Bottom_Right,
        Bottom_Left,
        Left,
        Top_Left
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

    private Directions ChooseDirection()
    {
        int adjustment = Random.Range(-1, 2);
        int length = System.Enum.GetValues(typeof(Directions)).Length;
        Directions newDirection = (Directions)(((int)currDirection + adjustment + length) % length);
        Debug.Log("Old Direction: " + currDirection + ", Adjustment: " + adjustment + ", New Direction: " + newDirection);
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
            currDirection = dir;
        }
        else
        {
            currDirection = dir;
            Move(ChooseDirection());
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
    }
}
