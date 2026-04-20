using UnityEngine;

[CreateAssetMenu(fileName = "CharacterUIDataSO", menuName = "Scriptable Objects/CharacterUIDataSO")]
public class CharacterUIDataSO : ScriptableObject
{
    [Header("HPUISetting")]
    [field: SerializeField] public Color hpGaugeColorHigh;                                // HP 게이지 색상 (높음)
    [field: SerializeField][Range(0f, 1f)] public float hpGaugeHighValue = 0.7f;          // HP 게이지 높음 임계값
    [field: SerializeField] public Color hpGaugeColorMiddle;                              // HP 게이지 색상 (중간)
    [field: SerializeField][Range(0f, 1f)] public float hpGaugeMiddleValue = 0.4f;        // HP 게이지 중간 임계값
    [field: SerializeField] public Color hpGaugeColorLow;                                 // HP 게이지 색상 (낮음)

    [Header("HungerUISetting")]
    [field: SerializeField] public Color hungerGaugeColorHigh;                            // 허기 게이지 색상 (높음)
    [field: SerializeField][Range(0f, 1f)] public float hungerGaugeHighValue = 0.5f;      // 허기 게이지 높음 임계값
    [field: SerializeField] public Color hungerGaugeColorLow;                             // 허기 게이지 색상 (낮음)
}
