using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBlueprints
{
    public string itemName;

    public string Req1;
    public string Req2;
    public string Req3;

    public int Req1Amount;
    public int Req2Amount;
    public int Req3Amount;

    public int numOfRequirements;
    public int numOfItemsToProduce;

    public ItemBlueprints(string name, int producedItems, int reqNUM, string R1, int R1num, string R2, int R2num, string R3, int R3num)
    {
        itemName = name;

        numOfRequirements = reqNUM;

        numOfItemsToProduce = producedItems;

        Req1 = R1;
        Req2 = R2;
        Req3 = R3;

        Req1Amount = R1num;
        Req2Amount = R2num;
        Req3Amount = R3num;
    }
}
