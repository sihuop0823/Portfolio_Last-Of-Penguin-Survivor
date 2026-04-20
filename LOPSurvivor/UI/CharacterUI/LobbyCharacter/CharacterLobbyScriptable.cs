using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "CharacterLoddyData", menuName = "Data/Character/LoddyData")]

public class CharacterLobbyScriptable : ScriptableObject
{
    public CharacterLobbyData[] characterLobbyData;
}
