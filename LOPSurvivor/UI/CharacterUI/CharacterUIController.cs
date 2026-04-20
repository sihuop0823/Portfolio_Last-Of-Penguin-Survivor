using Island;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterUIController : MonoBehaviour
{
    private CharacterController characterController;

    [Header("UI Setting Data SO")]
    [SerializeField] private CharacterUIDataSO characterUIDataSO;                   // UI ���� ������ SO
    [Header("HPUISetting")]
    [SerializeField] private Animator hpUIOutlineAnimator;                          // HP UI �ƿ���� �ִϸ�����
    private readonly int hashIsHit = Animator.StringToHash("IsHit");                // HP UI �ִϸ��̼� Ʈ���� �ؽ�
    [SerializeField] private Image img_hpGauge;                                     // HP ������ �̹���

    [Header("HungerUISetting")]
    [SerializeField] private GameObject hungerUIOutline;                            // ��� UI �ƿ���� ������Ʈ
    [SerializeField] private Image img_hungerGauge;                                 // ��� ������ �̹���
    [SerializeField] private Image img_damaged;
    private Coroutine hungerShakeRoutine;                                                   // ��� ��鸲 �ڷ�ƾ
    private Vector3 hungerUIOriginalPos;                                                    // ��� UI ���� ��ġ

    [Header("Hunger Shake Setting")]
    [SerializeField] private float shakeInterval = 5f;     // ��鸲 �ֱ�
    [SerializeField] private float shakeDuration = 0.3f;   // �� �� ���� �ð�
    [SerializeField] private float shakeAmount = 10f;      // ��鸲 ����(�ȼ�)

    [Header("TemperatureUISetting")]
    [SerializeField] private Image img_bodyTemperatureGauge;                                // ü�� ������ �̹���
    [SerializeField] private Image img_temperatureGauge;                                    // ��� ������ �̹���
    [SerializeField] private BodyTemperatureStateSetting[] bodyTemperatureStateSettings;    // ü�� ���ð� �迭
    [SerializeField] private TemperatureStateSetting[] temperatureStateSettings;            // ��� ���ð� �迭

    private Dictionary<TemperatureType, Color> bodyTemperatureStateSettingDict = new();     // ü�� �ܰ躰 ���� ��ųʸ�
    private Dictionary<TemperatureType, Color> temperatureStateSettingDict = new();         // ��� �ܰ躰 ���� ��ųʸ�

    private float prevHp = -1f;                                                             // ���� HP ��

    [Header("ClothesUISetting")]
    public GameObject img_ClothesGauge;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        img_ClothesGauge.SetActive(false);
        if (characterController != null)
        {
            characterController.OnHpChanged += OnCharacterHpChanged;
            characterController.OnDied += OnCharacterDied;
        }
        if (characterController?.CharacterStat != null)
            prevHp = characterController.CharacterStat.curHp;

        if (hungerUIOutline != null)
            hungerUIOriginalPos = hungerUIOutline.transform.localPosition;

        //ü�� ���ð� ��ųʸ� �ʱ�ȭ
        foreach (var setting in bodyTemperatureStateSettings)
        {
            bodyTemperatureStateSettingDict[setting.temperatureType] = setting.color;
        }

        ////��� ���ð� ��ųʸ� �ʱ�ȭ
        foreach (var setting in temperatureStateSettings)
        {
            temperatureStateSettingDict[setting.temperatureType] = setting.color;
        }
    }

    private void Start()
    {
        GaugeUpdate();
    }

    private void OnDisable()
    {
        if (characterController != null)
        {
            characterController.OnHpChanged -= OnCharacterHpChanged;
            characterController.OnDied -= OnCharacterDied;
        }
        StopHungerShake();
    }

    public void GaugeUpdate()
    {
        if(characterController == null || characterController.CharacterStat == null)
        {
            return;
        }

        float hp = characterController.CharacterStat.curHp;
        float maxHp = characterController.CharacterStat.maxHp;
        float hunger = characterController.CharacterStat.curHunger;
        float maxHunger = characterController.CharacterStat.maxHunger;

        float hpGaugeValue = (maxHp > 0f) ? hp / maxHp : 0f;
        float hungerGaugeValue = (maxHunger > 0f) ? hunger / maxHunger : 0f;

        // HP UI
        img_hpGauge.fillAmount = hpGaugeValue;
        img_hpGauge.color = (hpGaugeValue > characterUIDataSO.hpGaugeHighValue) ? characterUIDataSO.hpGaugeColorHigh :
                            (hpGaugeValue > characterUIDataSO.hpGaugeMiddleValue) ? characterUIDataSO.hpGaugeColorMiddle : characterUIDataSO.hpGaugeColorLow;

        // HP ���� �� �ִϸ��̼�
        if (prevHp >= 0f && hp < prevHp && hpUIOutlineAnimator != null)
            hpUIOutlineAnimator.SetTrigger(hashIsHit);
        prevHp = hp;

        // Hunger UI
        img_hungerGauge.fillAmount = hungerGaugeValue;
        img_hungerGauge.color = (hungerGaugeValue > characterUIDataSO.hungerGaugeHighValue) ? characterUIDataSO.hungerGaugeColorHigh : characterUIDataSO.hungerGaugeColorLow;

        // ��� �Ӱ�ġ ������ �� UI ����
        if (hungerGaugeValue < characterUIDataSO.hungerGaugeHighValue && hungerShakeRoutine == null)
            hungerShakeRoutine = StartCoroutine(Co_HungerShakeLoop());
        else if (hungerGaugeValue >= characterUIDataSO.hungerGaugeHighValue && hungerShakeRoutine != null)
            StopHungerShake();

        //ü�� UI
        img_bodyTemperatureGauge.color = bodyTemperatureStateSettingDict[characterController.CharacterStat.curBodyTemperature];

        ////��� UI
        img_temperatureGauge.color = temperatureStateSettingDict[TemperatureManager.Instance.AmbientTemperature];
    }

    private System.Collections.IEnumerator Co_HungerShakeLoop()
    {
        var waitInterval = new WaitForSeconds(Mathf.Max(0.01f, shakeInterval));
        while (true)
        {
            yield return ShakeHungerUIOnce();
            yield return waitInterval;
        }
    }

    private System.Collections.IEnumerator ShakeHungerUIOnce()
    {
        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            float xOffset = Mathf.Sin(elapsed * 50f) * shakeAmount; // ������ �¿� ����
            hungerUIOutline.transform.localPosition = hungerUIOriginalPos + new Vector3(xOffset, 0f, 0f);
            yield return null;
        }
        hungerUIOutline.transform.localPosition = hungerUIOriginalPos; // ����ġ
    }
    public IEnumerator Co_ShowDamageEffect()
    {
        if (img_damaged.color == null) yield break;

        float duration = 0.3f; // 투명도가 올라가는 시간
        float fadeDuration = 0.8f; // 투명도가 내려가는 시간
        float elapsed = 0f;

        Color color = img_damaged.color;

        // 나타나기
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(0f, 0.8f, elapsed / duration);
            img_damaged.color = color;
            yield return null;
        }

        color.a = 0.8f;
        img_damaged.color = color;

        // 사라지기
        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(0.8f, 0f, elapsed / fadeDuration);
            img_damaged.color = color;
            yield return null;
        }

        color.a = 0f;
        img_damaged.color = color;
    }

    private void StopHungerShake()
    {
        if (hungerShakeRoutine != null)
        {
            StopCoroutine(hungerShakeRoutine);
            hungerShakeRoutine = null;
        }
        if (hungerUIOutline != null)
            hungerUIOutline.transform.localPosition = hungerUIOriginalPos;
    }


    private void OnCharacterHpChanged(float hp)
    {
        GaugeUpdate();
    }

    private void OnCharacterDied()
    {
    }
}

//ü�� �ܰ躰 ���ð�
[Serializable]
public struct BodyTemperatureStateSetting
{
    public TemperatureType temperatureType;
    public Color color;
    public Sprite sprite;
}

//��� �ܰ躰 ���ð�
[Serializable]
public struct TemperatureStateSetting
{
    public TemperatureType temperatureType;
    public Color color;
    public Sprite sprite;

}
