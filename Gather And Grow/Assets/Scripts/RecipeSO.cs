using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Recipe", menuName = "ScriptableObjects/RecipeSO", order = 1)]
public class RecipeSO : ScriptableObject {

    public List<StationType> stationTypeList;
    public bool showInCookbook = true;
    public ItemSO alternativeItemInput;
    public List<ItemAmount> input;
    public List<ItemAmount> output;

    public float timeToCook;
    public int progressMax;

    [Header("Station Specific")]
    public Color[] cuttingStationAnimColor;

}
