using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogPanel : MonoBehaviour
{
    [SerializeField] private GameObject main_dialog;
    [SerializeField] private GameObject sub_dialog;

    [SerializeField] private TMP_Text txt_buddy_maindialog;
    [SerializeField] private TMP_Text txt_buddy_subdialog;

    [SerializeField] private Button btn_arrow_skip;
    [SerializeField] private Button btn_sub_arrow_skip;
    [SerializeField] private Button btn_skip;

    [SerializeField] private DialogDataScriptable dialogData; // ScriptableObject ����

    private int currentIndex = 0;
    private int subIndex = 0;
    private bool isMainDialog = true;
    private bool isDialogActive = false;
    private int currentChapter = 0;

    private void Awake()
    {
        btn_arrow_skip.onClick.AddListener(NextLine);
        btn_sub_arrow_skip.onClick.AddListener(NextLine);
        btn_skip.onClick.AddListener(SkipAll);
    }

    private void OnEnable()
    {
        StartMainDialog();
    }

    private void Update()
    {
        if (isDialogActive && Input.GetKeyDown(KeyCode.Space))
        {
            NextLine();
        }
        else if (isDialogActive && Input.GetKeyDown(KeyCode.Backspace))
        {
            BackDialog();
        }
    }

    private void StartMainDialog()
    {
        currentIndex = 0;
        isMainDialog = true;
        isDialogActive = true;
        ShowMainLine();
    }

    private void ShowMainLine()
    {
        if (currentIndex >= dialogData.GetChapterById(currentChapter).mainDialogs.Count)
        {
            StartSubDialog();
            return;
        }

        txt_buddy_maindialog.text = dialogData.GetChapterById(currentChapter).mainDialogs[currentIndex].Mainmessage;
    }

    private void StartSubDialog()
    {
        isMainDialog = false;
        subIndex = 0;

        if (main_dialog != null)
            main_dialog.SetActive(false);

        if (sub_dialog != null)
            sub_dialog.SetActive(true);

        ShowSubLine();
    }

    private void ShowSubLine()
    {
        if (subIndex >= dialogData.GetChapterById(currentChapter).subDialogs.Count)
        {
            EndAllDialog();
            return;
        }

        txt_buddy_subdialog.text = dialogData.GetChapterById(currentChapter).subDialogs[subIndex].Submessage;
    }

    private void NextLine()
    {
        if (!isDialogActive) return;

        if (isMainDialog)
        {
            currentIndex++;
            ShowMainLine();
        }
        else
        {
            subIndex++;
            ShowSubLine();
        }
    }

    private void BackDialog()
    {
        if (!isDialogActive) return;

        if (isMainDialog)
        {
            if (currentIndex > 0)
            {
                currentIndex--;
                ShowMainLine();
            }
        }
        else
        {
            if (subIndex > 0)
            {
                subIndex--;
                ShowSubLine();
            }
            else
            {
                isMainDialog = true;

                if (main_dialog != null) main_dialog.SetActive(true);
                if (sub_dialog != null) sub_dialog.SetActive(false);

                currentIndex = Mathf.Max(0, dialogData.GetChapterById(currentChapter).mainDialogs.Count - 1);
                ShowMainLine();
            }
        }
    }

    private void SkipAll()
    {
        if (!isDialogActive) return;

        if (isMainDialog)
        {
            currentIndex = dialogData.GetChapterById(currentChapter).mainDialogs.Count;
            StartSubDialog();
        }
        else
        {
            EndAllDialog();
        }
    }

    private void EndAllDialog()
    {
        isDialogActive = false;

        if (main_dialog != null)
            main_dialog.SetActive(false);

        if (txt_buddy_subdialog != null)
        {
            txt_buddy_subdialog.text = "";
            Debug.LogWarning("���� ��� ����");
        }
    }
}

