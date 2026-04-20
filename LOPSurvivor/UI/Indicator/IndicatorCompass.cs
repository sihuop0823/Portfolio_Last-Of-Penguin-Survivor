using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class IndicatorCompass : MonoBehaviour
{
    [Header("Graduations")]
    [SerializeField] private RectTransform graduationN;
    [SerializeField] private RectTransform graduationE;
    [SerializeField] private RectTransform graduationS;
    [SerializeField] private RectTransform graduationW;

    [Header("Quset Tracking")]
    [SerializeField] private GameObject TrackingContainer;
    [SerializeField] private RectTransform QuestTracking;

    [SerializeField] private GameObject CompassContainer;

    private Transform targetPosition;
    private float targetYRotation;

    private float fov = 60f;
    private float CompassWidth = 800f;
    private float TrackingWidth = 800f;

    private void Awake()
    {
        StartCoroutine(InitTarget());
    }

    private void Update()
    {
        if (targetPosition != null)
        {
            targetYRotation = targetPosition.eulerAngles.y;
        }
    }

    private void LateUpdate()
    {
        UpdateMarker(graduationN, 0f);
        UpdateMarker(graduationE, 90f);
        UpdateMarker(graduationS, 180f);
        UpdateMarker(graduationW, 270f);

        if (TrackingArrow.Instance != null && TrackingArrow.Instance.targetPosition.HasValue && targetPosition != null)
        {
            // 이것도 수학 계산하다가 못해서 AI 썼읍니다
            Vector3 dir = TrackingArrow.Instance.targetPosition.Value - targetPosition.position;
            float targetAngle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;

            UpdateTracking(QuestTracking, targetAngle);
        }
        else
        {
            if (QuestTracking.gameObject.activeSelf) QuestTracking.gameObject.SetActive(false);
        }
    }

    private void UpdateMarker(RectTransform rect, float markerAngle)
    {
        float angleDiff = Mathf.DeltaAngle(targetYRotation, markerAngle);

        if (Mathf.Abs(angleDiff) < fov / 2f)
        {
            if (!rect.gameObject.activeSelf) rect.gameObject.SetActive(true);

            // 수학 계산만 AI 썻읍니다,...
            float xPos = (angleDiff / (fov / 2f)) * (CompassWidth / 2f);
            rect.anchoredPosition = new Vector2(xPos, rect.anchoredPosition.y);
        }
        else
        {
            if (rect.gameObject.activeSelf)
            {
                rect.gameObject.SetActive(false);
            }
        }
    }

    private void UpdateTracking(RectTransform rect, float trakerAngle)
    {
        float TrackingAngleDiff = Mathf.DeltaAngle(targetYRotation, trakerAngle);

        if (!rect.gameObject.activeSelf) rect.gameObject.SetActive(true);

        float ratio = Mathf.Clamp(TrackingAngleDiff / (fov / 2f), -1f, 1f);

        float xPos = ratio * (TrackingWidth / 2f);
        rect.anchoredPosition = new Vector2(xPos, rect.anchoredPosition.y);
    }

    private IEnumerator InitTarget()
    {
        while (targetPosition == null)
        {
            if (GameManager.Instance != null &&
                GameManager.Instance.characterController != null)
            {
                targetPosition = GameManager.Instance.characterController.transform;
                targetYRotation = GameManager.Instance.characterController.transform.eulerAngles.y;
                yield break;
            }

            yield return null;
        }
    }
}
