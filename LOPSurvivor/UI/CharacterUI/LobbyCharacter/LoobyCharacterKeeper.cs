using UnityEngine;

public class LoobyCharacterKeeper : MonoBehaviour
{
    public static LoobyCharacterKeeper Instance;
    public int Index = -1;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if(WorldInputManager.Instance !=null)
        {
            WorldInputManager.Instance.gameInputType = WorldInputManager.GameInputType.None;
        }
    }
}
