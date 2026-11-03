using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Song Chart", menuName = "Pimba Heroes/Song Chart")]
public class SongChart : ScriptableObject
{
    public string songName;
    public AudioClip music;

    [Header("Takes da Música (em ordem)")]
    public List<Section> sections = new List<Section>();
}