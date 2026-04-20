using UnityEngine;

[System.Serializable]
public class DialogData
{
    public MainDialog mainDialog;
    public SubDialog subDialog;
}

[System.Serializable]
public class MainDialog
{
    public string Mainmessage;
}

[System.Serializable]
public class SubDialog
{
    public string Submessage;
}
