using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class NicknameSystem : MonoBehaviour
{
    [SerializeField] private TextMeshPro text;
    [SerializeField] private SpriteRenderer bg;

    [SerializeField] private float paddingX = 0.1f;
    [SerializeField] private float paddingY = 0.1f;

    [SerializeField] private float fixedHeight = 0.3f;

    public string Nickname => text != null ? text.text : "";
    public void SetNickname(string nickname)
    {
        text.text = nickname;
        //UpdateBackground();
    }

    private void UpdateBackground()
    {
        text.ForceMeshUpdate();
        Bounds bounds = text.bounds;

        float width = bounds.size.x + paddingX;

        bg.size = new Vector2(
            bounds.size.x + paddingX,
            fixedHeight
        );

        bg.transform.localPosition = bounds.center;
    }

}
