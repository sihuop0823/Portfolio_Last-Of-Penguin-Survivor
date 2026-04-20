using UnityEngine;

[System.Serializable]
public class CharacterLobbyData
{
    public string id;
    public string displayName;// 예: "일반 펭귄"
    public string quote;// 설명
    public string traitDescription;// 상세 설명에 나올 특성

    [Range(0f, 1f)] public float hp;
    [Range(0f, 1f)] public float hunger;
    [Range(0f, 1f)] public float vitality;
    [Range(0f, 1f)] public float moveSpeed;

    public Sprite image;
    public GameObject penguin3dPrefab;
    public int penguinId;
}
