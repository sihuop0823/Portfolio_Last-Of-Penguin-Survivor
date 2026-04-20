using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using GlobalAudio;

public class ChatAlarm : MonoBehaviour
{
    [SerializeField] private Button chat_btn;
    [SerializeField] private GameObject alarm_img;
    [SerializeField] private AudioClip alarm_clip;

    [SerializeField] private float alarmPulseTime = 0.5f;
    private Image btnImg;
    private Color originColor;

    private void Awake()
    {
        chat_btn.onClick.AddListener(ChatPopupOpen);

        btnImg = chat_btn.GetComponent<Image>();
        originColor = btnImg.color;
    }

    private void Start()
    {
        ChatPopup.Instance.isNewMessage += ChatAlarmUpdate;
    }

    private void ChatAlarmUpdate(bool isNewChat)
    {
       
        alarm_img.SetActive(isNewChat);

        if (isNewChat) 
        {
            AudioManager.Instance.PlaySFX(alarm_clip);
            StartCoroutine(ColorPulse());
        }
            
    }

    private IEnumerator ColorPulse()
    {
        float t = 0f;
        btnImg.color = Color.yellow;

        while (t < 1f)
        {
            yield return new WaitForSeconds(0.1f);
            t += Time.deltaTime / alarmPulseTime;
            btnImg.color = Color.Lerp(Color.yellow, originColor, t); 
            yield return null;
        }
    }

    private void ChatPopupOpen()
    {
        ChatPopup.Instance.Show(new PopupArgument());
        ChatAlarmUpdate(false);
    }

    private void OnDestroy()
    {
        ChatPopup.Instance.isNewMessage -= ChatAlarmUpdate;
    }
}
