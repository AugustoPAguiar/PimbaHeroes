using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.InputSystem.UI;

/// <summary>A self-contained, keyboard-playable local rhythm game prototype.</summary>
public sealed class PimbaRhythmGame : MonoBehaviour
{
    private static readonly Color Ink = new Color32(17, 18, 36, 255);
    private static readonly Color Panel = new Color32(35, 36, 64, 255);
    private static readonly Color Pink = new Color32(255, 72, 160, 255);
    private static readonly Color Cyan = new Color32(66, 224, 255, 255);
    private static readonly Color[] LaneColors = { new Color32(80, 196, 255, 255), new Color32(89, 235, 148, 255), new Color32(255, 218, 81, 255), new Color32(255, 100, 124, 255) };
    private static readonly Key[,] PlayerKeys = { { Key.LeftArrow, Key.DownArrow, Key.UpArrow, Key.RightArrow }, { Key.A, Key.S, Key.W, Key.D } };
    private static readonly string[] Arrows = { "←", "↓", "↑", "→" };
    private static readonly string[] Songs = { "NEON ALLEY", "PIXEL PARADE", "FINAL BOSS" };
    private static readonly string[] SongNotes = { "Warm up under the city lights.", "The arcade is getting louder.", "One last battle. Leave it all on stage." };

    private Canvas canvas;
    private Font font;
    private readonly List<NoteView> notes = new List<NoteView>();
    private readonly Text[] scoreText = new Text[2];
    private readonly Text[] comboText = new Text[2];
    private readonly float[] scores = new float[2];
    private readonly int[] combos = new int[2];
    private readonly int[] misses = new int[2];
    private readonly int[] hits = new int[2];
    private GameObject screen;
    private Text feedback;
    private Text timerText;
    private Image[] bars = new Image[2];
    private int songIndex;
    private int difficulty = 1;
    private int unlocked;
    private int mode;
    private float startTime;
    private float nextNote;
    private float bpm;
    private float beatInterval;
    private float duration;
    private bool playing;
    private bool finished;
    private AudioSource audioSource;

    //[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        if (FindAnyObjectByType<PimbaRhythmGame>() != null) return;
        var root = new GameObject("Pimba Rhythm Game");
        DontDestroyOnLoad(root);
        root.AddComponent<PimbaRhythmGame>();
    }

    private void Awake()
    {
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        unlocked = Mathf.Clamp(PlayerPrefs.GetInt("pimba_unlocked", 0), 0, 2);
        audioSource = gameObject.AddComponent<AudioSource>();
        BuildCanvas();
        ShowHome();
    }

    private void Update()
    {
        if (!playing) return;
        float elapsed = Time.time - startTime;
        timerText.text = FormatTime(Mathf.Max(0, duration - elapsed));
        while (nextNote < duration && nextNote <= elapsed + 1.6f)
        {
            SpawnBeat(nextNote);
            nextNote += beatInterval;
        }
        for (int i = notes.Count - 1; i >= 0; i--)
        {
            var note = notes[i];
            float until = note.time - elapsed;
            note.rect.anchoredPosition = new Vector2(note.rect.anchoredPosition.x, 80 + until * 300f);
            if (until < -0.18f)
            {
                if (note.image != null) Destroy(note.image.gameObject);
                notes.RemoveAt(i);
                AddMiss(note.player);
            }
        }
        PollKeys();
        if (elapsed >= duration && notes.Count == 0) FinishRound();
    }

    private void BuildCanvas()
    {
        var go = new GameObject("Game Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        go.transform.SetParent(transform, false);
        canvas = go.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = go.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1280, 720); scaler.matchWidthOrHeight = .5f;
        if (FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var es = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(InputSystemUIInputModule));
            es.transform.SetParent(transform, false);
        }
    }

    private void ShowHome()
    {
        playing = false; finished = false; ClearScreen();
        var root = MakeScreen("PIMBA HEROES");
        Label(root, "A LOCAL RHYTHM BATTLE", 24, Cyan, new Vector2(0, 205), new Vector2(700, 40));
        Label(root, "Hit the arrows. Feel the beat.", 21, Color.white, new Vector2(0, 155), new Vector2(700, 40));
        Button(root, "CAMPAIGN", new Vector2(0, 65), new Vector2(360, 68), Pink, ShowCampaign);
        Button(root, "CHAPTER SELECT", new Vector2(0, -20), new Vector2(360, 68), new Color32(110, 90, 230, 255), ShowChapters);
        Button(root, "2 PLAYER LOCAL", new Vector2(0, -105), new Vector2(360, 68), Cyan, ShowMultiplayer);
        Label(root, "PLAYER 1:  ←  ↓  ↑  →     •     PLAYER 2:  A  S  W  D", 17, new Color32(196, 201, 225, 255), new Vector2(0, -220), new Vector2(800, 38));
    }

    private void ShowCampaign()
    {
        mode = 0; songIndex = 0; ShowSongPicker(false);
    }

    private void ShowChapters()
    {
        mode = 0; ShowSongPicker(true);
    }

    private void ShowSongPicker(bool chapters)
    {
        ClearScreen(); var root = MakeScreen(chapters ? "CHAPTER SELECT" : "CAMPAIGN");
        Label(root, chapters ? "Replay any song you have unlocked." : "Three songs. The beat gets faster each time.", 20, Color.white, new Vector2(0, 210), new Vector2(850, 40));
        for (int i = 0; i < 3; i++)
        {
            int index = i; bool locked = chapters && i > unlocked;
            string title = locked ? "🔒  ???" : (i + 1).ToString("00") + "  " + Songs[i];
            Button(root, title, new Vector2(0, 115 - i * 85), new Vector2(550, 65), locked ? new Color32(70, 72, 91, 255) : (i == unlocked ? Pink : Panel), () => { if (!locked) { songIndex = index; ShowDifficulty(); } });
            if (!locked) Label(root, SongNotes[i], 15, new Color32(205, 210, 235, 255), new Vector2(0, 77 - i * 85), new Vector2(700, 26));
        }
        Button(root, "BACK", new Vector2(0, -205), new Vector2(200, 54), Panel, ShowHome);
    }

    private void ShowDifficulty()
    {
        ClearScreen(); var root = MakeScreen(Songs[songIndex]);
        Label(root, SongNotes[songIndex], 20, Color.white, new Vector2(0, 155), new Vector2(800, 40));
        Button(root, "EASY", new Vector2(-210, 20), new Vector2(220, 75), new Color32(80, 190, 125, 255), () => { difficulty = 0; StartRound(); });
        Button(root, "NORMAL", new Vector2(0, 20), new Vector2(220, 75), Pink, () => { difficulty = 1; StartRound(); });
        Button(root, "HARD", new Vector2(210, 20), new Vector2(220, 75), new Color32(235, 100, 90, 255), () => { difficulty = 2; StartRound(); });
        Button(root, "BACK", new Vector2(0, -115), new Vector2(200, 54), Panel, () => ShowSongPicker(songIndex > unlocked));
    }

    private void ShowMultiplayer()
    {
        ClearScreen(); var root = MakeScreen("2 PLAYER LOCAL");
        Label(root, "Choose how you want to battle.", 21, Color.white, new Vector2(0, 155), new Vector2(700, 40));
        Button(root, "TEAM UP", new Vector2(0, 55), new Vector2(380, 75), Cyan, () => { mode = 1; songIndex = 0; ShowMultiSong(); });
        Label(root, "Both players work together to beat a rival.", 16, new Color32(205, 210, 235, 255), new Vector2(0, 15), new Vector2(700, 28));
        Button(root, "VERSUS", new Vector2(0, -65), new Vector2(380, 75), Pink, () => { mode = 2; songIndex = 0; ShowMultiSong(); });
        Label(root, "Battle for the highest score.", 16, new Color32(205, 210, 235, 255), new Vector2(0, -105), new Vector2(700, 28));
        Button(root, "BACK", new Vector2(0, -205), new Vector2(200, 54), Panel, ShowHome);
    }

    private void ShowMultiSong()
    {
        ClearScreen(); var root = MakeScreen(mode == 1 ? "TEAM UP" : "VERSUS");
        Label(root, "Pick a song and difficulty.", 20, Color.white, new Vector2(0, 180), new Vector2(700, 40));
        for (int i = 0; i < 3; i++) { int ix = i; Button(root, Songs[i], new Vector2(0, 100 - i * 66), new Vector2(420, 54), i == songIndex ? Pink : Panel, () => { songIndex = ix; }); }
        Button(root, "EASY", new Vector2(-205, -145), new Vector2(180, 58), new Color32(80, 190, 125, 255), () => { difficulty = 0; StartRound(); });
        Button(root, "NORMAL", new Vector2(0, -145), new Vector2(180, 58), Cyan, () => { difficulty = 1; StartRound(); });
        Button(root, "HARD", new Vector2(205, -145), new Vector2(180, 58), new Color32(235, 100, 90, 255), () => { difficulty = 2; StartRound(); });
        Button(root, "BACK", new Vector2(0, -220), new Vector2(180, 48), Panel, ShowMultiplayer);
    }

    private void StartRound()
    {
        ClearScreen(); notes.Clear(); scores[0] = scores[1] = 0; combos[0] = combos[1] = 0; misses[0] = misses[1] = 0; hits[0] = hits[1] = 0;
        bpm = new[] { 92f, 112f, 132f }[songIndex] + new[] { -16f, 0f, 18f }[difficulty];
        beatInterval = 60f / bpm; duration = 27f + songIndex * 3f;
        var root = MakeScreen(Songs[songIndex]);
        Label(root, (mode == 0 ? "PLAYER 1" : mode == 1 ? "TEAM UP" : "VERSUS"), 19, Cyan, new Vector2(0, 295), new Vector2(400, 32));
        timerText = Label(root, "", 20, Color.white, new Vector2(0, 255), new Vector2(200, 32));
        int playerCount = mode == 0 ? 1 : 2;
        for (int p = 0; p < playerCount; p++)
        {
            float x = playerCount == 1 ? 0 : (p == 0 ? -300 : 300);
            Label(root, p == 0 ? "P1  ← ↓ ↑ →" : "P2  A S W D", 17, p == 0 ? Cyan : Pink, new Vector2(x, 205), new Vector2(320, 32));
            for (int lane = 0; lane < 4; lane++)
            {
                var target = Label(root, Arrows[lane], 40, LaneColors[lane], new Vector2(x - 135 + lane * 90, 45), new Vector2(72, 72));
                target.fontStyle = FontStyle.Bold;
                var box = PanelImage(root, new Vector2(x - 135 + lane * 90, 45), new Vector2(76, 76)); box.transform.SetAsFirstSibling();
            }
            scoreText[p] = Label(root, "SCORE  0", 18, Color.white, new Vector2(x, -255), new Vector2(300, 30));
            comboText[p] = Label(root, "COMBO  0", 17, new Color32(255, 220, 120, 255), new Vector2(x, -285), new Vector2(300, 28));
        }
        feedback = Label(root, "GET READY!", 25, Color.white, new Vector2(0, -175), new Vector2(650, 40));
        if (mode == 1) { bars[0] = MakeBar(root, "TEAM", new Vector2(0, -220), 500); }
        if (mode == 2) { bars[0] = MakeBar(root, "P1", new Vector2(-300, -220), 330); bars[1] = MakeBar(root, "P2", new Vector2(300, -220), 330); }
        playing = true; finished = false; startTime = Time.time + 2.0f; nextNote = 0;
    }

    private void SpawnBeat(float beat)
    {
        int players = mode == 0 ? 1 : 2;
        for (int p = 0; p < players; p++)
        {
            // Half the beats are shared in co-op, keeping both charts readable.
            if (mode == 1 && Random.value < .28f) continue;
            int lane = Random.Range(0, 4);
            float x = players == 1 ? 0 : (p == 0 ? -300 : 300);
            var note = Label(screen.transform, Arrows[lane], 38, LaneColors[lane], new Vector2(x - 135 + lane * 90, 900), new Vector2(70, 70));
            note.fontStyle = FontStyle.Bold;
            notes.Add(new NoteView { image = note, rect = note.rectTransform, time = beat + 2.0f, lane = lane, player = p });
        }
    }

    private void PollKeys()
    {
        if (Keyboard.current == null) return;
        int players = mode == 0 ? 1 : 2;
        for (int p = 0; p < players; p++) for (int lane = 0; lane < 4; lane++) if (Keyboard.current[PlayerKeys[p, lane]].wasPressedThisFrame) Hit(p, lane);
    }

    private void Hit(int player, int lane)
    {
        int found = -1; float best = .19f; float elapsed = Time.time - startTime;
        for (int i = 0; i < notes.Count; i++)
        {
            var n = notes[i]; float delta = Mathf.Abs(n.time - elapsed);
            if (n.player == player && n.lane == lane && delta < best) { best = delta; found = i; }
        }
        if (found < 0) { combos[player] = 0; ShowFeedback("MISS", new Color32(255, 110, 110, 255)); UpdateHud(); return; }
        var note = notes[found]; float deltaBeat = Mathf.Abs(note.time - elapsed);
        scores[player] += (deltaBeat < .075f ? 350 : deltaBeat < .13f ? 220 : 100) + combos[player] * 3;
        combos[player]++; hits[player]++; Destroy(note.image.gameObject); notes.RemoveAt(found);
        ShowFeedback(deltaBeat < .075f ? "PERFECT!" : deltaBeat < .13f ? "GREAT!" : "GOOD", LaneColors[lane]);
        UpdateHud(); PlayTone(330f + lane * 90f);
    }

    private void AddMiss(int player)
    {
        misses[player]++; combos[player] = 0; scores[player] = Mathf.Max(0, scores[player] - 80); ShowFeedback("MISS", new Color32(255, 110, 110, 255)); UpdateHud();
    }

    private void UpdateHud()
    {
        for (int p = 0; p < 2; p++) if (scoreText[p] != null) { scoreText[p].text = "SCORE  " + Mathf.RoundToInt(scores[p]).ToString("N0"); comboText[p].text = "COMBO  " + combos[p]; }
        if (mode == 1 && bars[0] != null) bars[0].fillAmount = Mathf.Clamp01((hits[0] + hits[1] - misses[0] - misses[1] * 1f + 5f) / 32f);
        if (mode == 2) for (int p = 0; p < 2; p++) if (bars[p] != null) bars[p].fillAmount = Mathf.Clamp01(scores[p] / Mathf.Max(1, scores[0] + scores[1]));
    }

    private void FinishRound()
    {
        if (finished) return; finished = true; playing = false;
        if (mode == 0 && songIndex == unlocked) { unlocked = Mathf.Min(2, unlocked + 1); PlayerPrefs.SetInt("pimba_unlocked", unlocked); PlayerPrefs.Save(); }
        string result;
        if (mode == 0) result = misses[0] == 0 ? "FLAWLESS!" : "NICE PERFORMANCE!";
        else if (mode == 1) result = scores[0] + scores[1] > 3000 ? "RIVAL DEFEATED!" : "KEEP PRACTICING!";
        else result = scores[0] == scores[1] ? "IT'S A DRAW!" : scores[0] > scores[1] ? "PLAYER 1 WINS!" : "PLAYER 2 WINS!";
        ClearScreen(); var root = MakeScreen("SONG COMPLETE"); Label(root, result, 32, Pink, new Vector2(0, 155), new Vector2(700, 55));
        int players = mode == 0 ? 1 : 2;
        for (int p = 0; p < players; p++) Label(root, (p == 0 ? "PLAYER 1" : "PLAYER 2") + "   " + Mathf.RoundToInt(scores[p]).ToString("N0") + " pts   •   " + hits[p] + " hits   •   " + misses[p] + " misses", 20, p == 0 ? Cyan : Pink, new Vector2(0, 75 - p * 45), new Vector2(900, 36));
        Button(root, "PLAY AGAIN", new Vector2(0, -70), new Vector2(280, 62), Pink, StartRound);
        Button(root, "MENU", new Vector2(0, -150), new Vector2(280, 62), Panel, ShowHome);
    }

    private void PlayTone(float frequency)
    {
        if (audioSource == null) return;
        var clip = AudioClip.Create("Pimba blip", 2205, 1, 22050, false);
        float[] samples = new float[2205]; for (int i = 0; i < samples.Length; i++) samples[i] = Mathf.Sin(2 * Mathf.PI * frequency * i / 22050f) * (1f - i / (float)samples.Length) * .18f;
        clip.SetData(samples, 0); audioSource.PlayOneShot(clip);
    }

    private Image MakeBar(Transform parent, string title, Vector2 pos, float width)
    {
        Label(parent, title, 13, Color.white, pos + Vector2.up * 14, new Vector2(width, 22));
        var back = PanelImage(parent, pos + Vector2.down * 6, new Vector2(width, 14));
        var fillObj = new GameObject("Progress", typeof(RectTransform), typeof(Image)); fillObj.transform.SetParent(back.transform, false);
        var rt = fillObj.GetComponent<RectTransform>(); rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        var img = fillObj.GetComponent<Image>(); img.color = Pink; img.type = Image.Type.Filled; img.fillMethod = Image.FillMethod.Horizontal; img.fillAmount = .5f; return img;
    }

    private Image MakeBar(GameObject parent, string title, Vector2 pos, float width)
    {
        return MakeBar(parent.transform, title, pos, width);
    }

    private GameObject MakeScreen(string title)
    {
        screen = new GameObject("Screen - " + title, typeof(RectTransform), typeof(Image)); screen.transform.SetParent(canvas.transform, false);
        var rt = screen.GetComponent<RectTransform>(); rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        screen.GetComponent<Image>().color = Ink;
        var glow = PanelImage(screen.transform, new Vector2(0, 305), new Vector2(1280, 6)); glow.color = Pink;
        var header = Label(screen.transform, title, 32, Color.white, new Vector2(0, 310), new Vector2(800, 55)); header.fontStyle = FontStyle.Bold;
        return screen;
    }

    private Text Label(Transform parent, string value, int size, Color color, Vector2 pos, Vector2 dimensions)
    {
        var go = new GameObject("Text - " + value, typeof(RectTransform), typeof(Text)); go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>(); rt.anchorMin = rt.anchorMax = new Vector2(.5f, .5f); rt.pivot = new Vector2(.5f, .5f); rt.anchoredPosition = pos; rt.sizeDelta = dimensions;
        var t = go.GetComponent<Text>(); t.text = value; t.font = font; t.fontSize = size; t.color = color; t.alignment = TextAnchor.MiddleCenter; t.horizontalOverflow = HorizontalWrapMode.Wrap; t.verticalOverflow = VerticalWrapMode.Overflow; return t;
    }

    private Text Label(GameObject parent, string value, int size, Color color, Vector2 pos, Vector2 dimensions)
    {
        return Label(parent.transform, value, size, color, pos, dimensions);
    }

    private Image PanelImage(Transform parent, Vector2 pos, Vector2 size)
    {
        var go = new GameObject("Panel", typeof(RectTransform), typeof(Image)); go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>(); rt.anchorMin = rt.anchorMax = new Vector2(.5f, .5f); rt.pivot = new Vector2(.5f, .5f); rt.anchoredPosition = pos; rt.sizeDelta = size;
        var image = go.GetComponent<Image>(); image.color = Panel; return image;
    }

    private Image PanelImage(GameObject parent, Vector2 pos, Vector2 size)
    {
        return PanelImage(parent.transform, pos, size);
    }

    private void Button(Transform parent, string label, Vector2 pos, Vector2 size, Color color, UnityEngine.Events.UnityAction action)
    {
        var go = new GameObject("Button - " + label, typeof(RectTransform), typeof(Image), typeof(Button)); go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>(); rt.anchorMin = rt.anchorMax = new Vector2(.5f, .5f); rt.pivot = new Vector2(.5f, .5f); rt.anchoredPosition = pos; rt.sizeDelta = size;
        var image = go.GetComponent<Image>(); image.color = color;
        var button = go.GetComponent<Button>(); button.targetGraphic = image; button.onClick.AddListener(action);
        Label(go.transform, label, 20, Color.white, Vector2.zero, size - new Vector2(16, 8)).fontStyle = FontStyle.Bold;
    }

    private void Button(GameObject parent, string label, Vector2 pos, Vector2 size, Color color, UnityEngine.Events.UnityAction action)
    {
        Button(parent.transform, label, pos, size, color, action);
    }

    private void ShowFeedback(string value, Color color) { if (feedback != null) { feedback.text = value; feedback.color = color; } }
    private static string FormatTime(float seconds) { int s = Mathf.CeilToInt(seconds); return (s / 60).ToString("00") + ":" + (s % 60).ToString("00"); }
    private void ClearScreen() { if (screen != null) Destroy(screen); scoreText[0] = scoreText[1] = null; comboText[0] = comboText[1] = null; feedback = null; timerText = null; bars[0] = bars[1] = null; }

    private sealed class NoteView { public Text image; public RectTransform rect; public int lane; public int player; public float time; }
}
