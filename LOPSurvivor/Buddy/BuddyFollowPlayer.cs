using UnityEngine;

public class BuddyFollowPlayer : MonoBehaviour
{
    [SerializeField] private Transform PlayerPenguin;
    [SerializeField] private Vector3 BuddyDistance = new Vector3(1.5f, 1.8f, 0f); //��ϰ� Buddy ������ �Ÿ�
    [SerializeField] private float followSpeed = 3f;
    private void Start()
    {
        if (PlayerPenguin == null)
        {
            try
            {
                PlayerPenguin = GameManager.Instance.characterController.transform;
            }
            catch (System.Exception ex)
            {
                return;
            }

            if (PlayerPenguin == null)
            {
                Debug.LogError("characterPosition disappear");
            }
        }
    }
    private void LateUpdate()
    {
        if (PlayerPenguin == null) return;

        Vector3 targetPos = PlayerPenguin.position + BuddyDistance;
        transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);

        transform.rotation = PlayerPenguin.rotation;
    }
}
