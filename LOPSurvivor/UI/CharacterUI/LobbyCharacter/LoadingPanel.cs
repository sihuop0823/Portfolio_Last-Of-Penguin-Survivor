using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using NUnit.Framework.Constraints;

public class LoadingPanel : Panel<LoadingPanel>
{
    public override PanelType PanelType => PanelType.LoadingPanel;

    [SerializeField] private TMP_Text txt_loading;
    
    public string loadScene;

    [Header("Fade")]
    [SerializeField] private RectTransform fade_img;
    [SerializeField] private CanvasGroup fade_img_alpha;
    [SerializeField] private float fade_img_scale;
    [SerializeField] private float fade_time = 2f;

    [Header("Tip")]
    [SerializeField] private TMP_Text txt_tip;
    [SerializeField] private string[] tipList;

    public class Args : PanelArgument
    {
        public string sceneName;

        public Args(string sceneName)
        { 
            this.sceneName = sceneName;
        }
    }

    private void OnEnable()
    {
        fade_img.localScale = Vector3.zero;
        fade_img_alpha.alpha = 0f;

        StartCoroutine(LoadingTextAnim());
        StartCoroutine(RandomTip());
    }

    private IEnumerator LoadingTextAnim()
    {
        string loadingText = "Loading";
        int dotCount = 0;

        while (dotCount < 3)
        {
            for (int i = 0; i <= 3; i++)
            {
                txt_loading.text = loadingText + new string('.', i);
                yield return new WaitForSeconds(0.2f);
            }

            txt_loading.text = loadingText;
            yield return new WaitForSeconds(0.3f);

            dotCount++;

        }

        StartCoroutine(IrisOutFade());
    }

    private IEnumerator RandomTip()
    {
        if (tipList.Length == 0)
        {
            Debug.LogWarning("Tip list is null");
            yield break;
        }

        int tipCount = tipList.Length;

        int randomIndex = Random.Range(0, tipCount);

        string randomTip = tipList[randomIndex];

        txt_tip.text = "Tip : " + randomTip;

        yield return null;
    }

    IEnumerator IrisOutFade()
    {
        float currentTime = 0f;
        Vector3 fadeScale = Vector3.one * fade_img_scale;

        while (currentTime < fade_time)
        {
            currentTime += Time.deltaTime;
            float fadeProgress = currentTime / fade_time;

            fade_img.localScale = Vector3.Lerp(Vector3.zero, fadeScale, fadeProgress);

            fade_img_alpha.alpha = fadeProgress;

            yield return null;
        }

        fade_img.localScale = fadeScale;

        fade_img_alpha.alpha = 1f;

        SceneManager.LoadScene(loadScene);
    }

    public override void OnShow(PanelArgument panelArguments)
    {
        if (panelArguments is Args args)
        {
            loadScene = args.sceneName;
        }
    }

    public override void OnHide()
    {

    }
}
