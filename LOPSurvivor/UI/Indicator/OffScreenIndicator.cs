using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OffScreenIndicator : MonoBehaviour
{

    private void Update()
    {
        if (TrackingArrow.Instance.targetPosition.HasValue)
        {
            bool isVisible = IsTargetInScreen(TrackingArrow.Instance.targetPosition.Value);

            TrackingArrow.Instance.gameObject.SetActive(isVisible);
        }
        else
        {
            TrackingArrow.Instance.gameObject.SetActive(false);
        }
    }

    public bool IsTargetInScreen(Vector3 targetPos)
    {
        Vector3 viewportPos = Camera.main.WorldToViewportPoint(targetPos);

        bool InScreenTarget = viewportPos.z > 0f && viewportPos.x >= 0f && viewportPos.x <= 1f && viewportPos.y >= 0f && viewportPos.y <= 1f;

        return InScreenTarget;
    }
}
