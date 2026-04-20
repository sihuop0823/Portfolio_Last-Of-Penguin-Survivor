using UnityEngine;

public class BuddyController : MonoBehaviour
{
    [SerializeField] private Animator buddyAnimator;
    private CharacterState characterState = CharacterState.Alive; // -> ĳ���� dead ���� Ȯ���Ϸ��� ��������

    private bool isInventoryOpen = false;
    private bool isClosing = false;

    private void Awake()
    {
        buddyAnimator = GetComponentInChildren<Animator>();

        if (buddyAnimator == null)
            Debug.LogError("버디 컨트롤러 애니메이터 왜 자꾸 없어지지..");
    }

    private void Start()
    {
        InventoryOnOff.OnInventoryOpen += BuddyInventoryOpen;
        InventoryOnOff.OnInventoryClose += BuddyInventoryClose;
    }

    private void OnDestroy()
    {
        InventoryOnOff.OnInventoryOpen -= BuddyInventoryOpen;
        InventoryOnOff.OnInventoryClose -= BuddyInventoryClose;
    }

    private void BuddyInventoryOpen()
    {
        isInventoryOpen = true;
        isClosing = false;

        buddyAnimator.Play("Buddy_Open Inventory", 0, 0f);
    }

    private void BuddyInventoryClose()
    {

        isClosing = true;

        buddyAnimator.Play("Buddy_Close Inventory", 0, 0f);
        Debug.Log("buddy close");
    }

    private void BuddyDead()
    {
        if (characterState == CharacterState.Dead)
        {
            buddyAnimator.Play("Buddy_Death", 0, 0f);
        }
    }

    private void Update()
    {

        if (characterState == CharacterState.Dead)
        {
            BuddyDead();
            return; // Dead�� ������ �ִϸ��̼� ���� ����
        }

        AnimatorStateInfo state = buddyAnimator.GetCurrentAnimatorStateInfo(0);

        if (isInventoryOpen && !isClosing && state.IsName("Buddy_Open Inventory") && state.normalizedTime >= 1f)
        {
            buddyAnimator.Play("Buddy_Open Loop", 0, 0f);
        }

        // Close ������ idle�� ��ȯ
        if (isClosing && state.IsName("Buddy_Close Inventory") && state.normalizedTime >= 1f)
        {
            buddyAnimator.Play("Buddy_Idle", 0, 0f);
            isInventoryOpen = false;
            isClosing = false;
        }
        // state.normalizedTime �� �Ѿ□�� �� ������ �������� 2�� Ȯ��!
    }
}