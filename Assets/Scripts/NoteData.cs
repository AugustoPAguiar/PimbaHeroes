using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class NoteData
{
    public float time;          // tempo relativo ao início do Take
    public int direction;       // 0=Left, 1=Down, 2=Up, 3=Right
    public bool isHold;
    public float holdDuration = 1f;
}

[System.Serializable]
public class Section
{
    public string sectionName;
    public List<NoteData> notes = new List<NoteData>();
}