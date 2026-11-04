using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroMusicManager : MonoBehaviour
{
    public static IntroMusicManager Instance;

    public AudioSource musicSource;
    public AudioClip musicaMenu;          // Música das cenas de Menu e Multiplayer

    [Header("Cenas onde a música deve tocar")]
    public string[] cenasComMusica = { "1 - Menu Principal", "Multiplayer" }; // coloque os nomes exatos das cenas

    void Awake()
    {
        // Singleton + DontDestroyOnLoad
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject); // evita duplicar a música
            return;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        Debug.Log("Começo Start");
        if (musicSource != null)
        {
            Debug.Log("Entro no if");
            musicSource.clip = musicaMenu;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool deveTocar = false;

        foreach (string nomeCena in cenasComMusica)
        {
            if (scene.name == nomeCena)
            {
                deveTocar = true;
                break;
            }
        }

        if (deveTocar)
        {
            // Está em uma das cenas permitidas
            if (!musicSource.isPlaying)
            {
                musicSource.clip = musicaMenu;
                musicSource.loop = true;
                musicSource.Play();
            }
        }
        else
        {
            // Saiu das cenas de menu/multiplayer → para a música
            musicSource.Stop();
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}