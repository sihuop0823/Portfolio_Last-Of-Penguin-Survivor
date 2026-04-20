using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueData", menuName = "Buddy/Dialogue Data")]
public class DialogDataScriptable : ScriptableObject
{
    public List<DialogueChapter> chapters = new List<DialogueChapter>();

    [System.Serializable]
    public class DialogueChapter
    {
        public int chapterId;
        public List<MainDialog> mainDialogs = new List<MainDialog>();
        public List<SubDialog> subDialogs = new List<SubDialog>();
    }

    [System.Serializable]
    public class MainDialog
    {
        [TextArea(2, 5)]
        public string Mainmessage;
    }

    [System.Serializable]
    public class SubDialog
    {
        [TextArea(2, 5)]
        public string Submessage;
    }

    /// <summary>
    /// 챕터 ID로 대화 데이터를 찾아 반환
    /// </summary>
    public DialogueChapter GetChapterById(int id)
    {
        return chapters.Find(c => c.chapterId == id);
    }
}
