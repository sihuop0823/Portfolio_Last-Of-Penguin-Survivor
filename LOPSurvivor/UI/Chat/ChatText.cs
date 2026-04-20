using System.Net.Sockets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatText : MonoBehaviour
{
    [SerializeField] private TMP_Text nicknameText;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Image character_img;
    [SerializeField] private GameObject chatText_img;

    [Header("Size Settings")]
    [SerializeField] private float baseHeight = 60f;
    [SerializeField] private float heightPerLine = 60f;

    public void SetContent(string nickname, string message)
    {
        nicknameText.text = nickname;
        messageText.text = message;
        
        ChatTextSizeUpdate();
    }

    public void ChatTextSizeUpdate()
    {
        float textHeight = messageText.preferredHeight;

        float totalHeight = textHeight + 50f;

        messageText.rectTransform.sizeDelta = new Vector2(messageText.rectTransform.sizeDelta.x, textHeight);
        chatText_img.GetComponent<RectTransform>().sizeDelta = new Vector2(chatText_img.GetComponent<RectTransform>().sizeDelta.x, totalHeight);
        GetComponent<RectTransform>().sizeDelta = new Vector2(GetComponent<RectTransform>().sizeDelta.x, totalHeight);
    }
}