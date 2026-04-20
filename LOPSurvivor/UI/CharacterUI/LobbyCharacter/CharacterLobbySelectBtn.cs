using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterLobbySelectBtn : MonoBehaviour
{
    [SerializeField] private CharacterLobbyScriptable characterLobbyScriptable;
    [SerializeField] private CharacterLobbySelectUI characterLobbySelectUI;
    [SerializeField] private int characterIndex;
    //[SerializeField] private AudioClip clickClip; 오디오는 당장은 필요 없으니 Off
    //TODO :: 여기도 사운드 매니저 없어서 주석처리

    public void Awake()
    {
        var btn = GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(OnClickSelect);
    }

    private void Start()
    {
        if (LoobyCharacterKeeper.Instance.Index != -1)
        {
            int i = characterIndex;
            characterIndex = LoobyCharacterKeeper.Instance.Index;
            OnClickSelect();
            characterIndex = i;
        }
    }

    private void OnClickSelect()
    {
        if (characterLobbyScriptable == null)
        {
            Debug.LogError("No Scriptable");
            return;
        }

        LoobyCharacterKeeper.Instance.Index = characterIndex;
        CharacterLobbyData data = characterLobbyScriptable.characterLobbyData[characterIndex];
        //SoundManager.Instance.PlaySFX(clickClip);
        //TODO :: 여기도 사운드 매니저 없어서 주석처리

        if (data != null)
        {
            characterLobbySelectUI.SelectCharacter(data);
        }
    }
}


