using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour, IPointerDownHandler
{
    public static AudioManager Instance { get; private set; }

    [Header("Mixer e Fontes de Áudio")]
    public AudioMixer mainMixer;
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    [Header("Sons Padrão")]
    public AudioClip defaultClickSound;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start é chamado um pouco depois do Awake, garantindo que tudo esteja pronto
    void Start()
    {
        // Carrega os volumes salvos assim que o jogo inicia
        CarregarVolumesSalvos();
    }
    
    private void CarregarVolumesSalvos()
    {
        if (mainMixer == null)
        {
            Debug.LogError("A referência do MainMixer não foi atribuída no Inspector do AudioManager!");
            return;
        }

        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.75f);
        float bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 0.75f);

        mainMixer.SetFloat("SFXVolume", Mathf.Log10(sfxVolume > 0.001f ? sfxVolume : 0.001f) * 20);
        mainMixer.SetFloat("BGMVolume", Mathf.Log10(bgmVolume > 0.001f ? bgmVolume : 0.001f) * 20);
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Button clickedButton = eventData.pointerCurrentRaycast.gameObject.GetComponent<Button>();
        if (clickedButton != null && clickedButton.interactable)
        {
            PlaySFX(defaultClickSound);
        }
    }
}