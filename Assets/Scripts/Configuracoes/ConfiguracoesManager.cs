using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;
using UnityEngine.SceneManagement;
using Firebase.Auth;

public class ConfiguracoesManager : MonoBehaviour
{
    [Header("Referências de Áudio")]
    public AudioMixer mainMixer;

    [Header("Referências da UI")]
    public Slider sfxSlider;
    public Slider bgmSlider;

    [Header("Ícones de Volume")]
    public Image sfxIcon;
    public Image bgmIcon;
    public Sprite sfxOnSprite;
    public Sprite sfxOffSprite;
    public Sprite bgmOnSprite;
    public Sprite bgmOffSprite;

    void Start()
    {
        CarregarConfiguracoes();
    }

    private void CarregarConfiguracoes()
    {
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 0.75f);
        bgmSlider.value = PlayerPrefs.GetFloat("BGMVolume", 0.75f);
    }

    public void SetSFXVolume(float volume)
    {
        float volumeEmDB = (volume > 0.001f) ? Mathf.Log10(volume) * 20 : -80f;
        mainMixer.SetFloat("SFXVolume", volumeEmDB);
        PlayerPrefs.SetFloat("SFXVolume", volume);

        // Atualiza o ícone com base no volume
        if (sfxIcon != null)
        {
            sfxIcon.sprite = (volume > 0.001f) ? sfxOnSprite : sfxOffSprite;
        }
    }

    public void SetBGMVolume(float volume)
    {
        float volumeEmDB = (volume > 0.001f) ? Mathf.Log10(volume) * 20 : -80f;
        mainMixer.SetFloat("BGMVolume", volumeEmDB);
        PlayerPrefs.SetFloat("BGMVolume", volume);

        // Atualiza o ícone com base no volume
        if (bgmIcon != null)
        {
            bgmIcon.sprite = (volume > 0.001f) ? bgmOnSprite : bgmOffSprite;
        }
    }

    public void ToggleSFXMute()
    {
        // Se o volume atual é maior que zero, silencia. Senão, restaura para 75%.
        if (sfxSlider.value > 0.001f)
        {
            sfxSlider.value = 0f;
        }
        else
        {
            sfxSlider.value = 0.50f;
        }
        // Mover o slider já chama SetSFXVolume e atualiza tudo automaticamente.
    }

    public void ToggleBGMMute()
    {
        if (bgmSlider.value > 0.001f)
        {
            bgmSlider.value = 0f;
        }
        else
        {
            bgmSlider.value = 0.50f;
        }
        // Mover o slider já chama SetBGMVolume e atualiza tudo automaticamente.
    }

    public void Salvar()
    {
        PlayerPrefs.Save();
        SceneManager.LoadScene("CenaMenu");
    }

    public void Logout()
    {
        if (FirebaseAuth.DefaultInstance.CurrentUser != null)
        {
            FirebaseAuth.DefaultInstance.SignOut();
        }
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.Dados = null;
        }
        SceneManager.LoadScene("CenaLogin");
    }

    public void AbrirMenu()
    {
        SceneManager.LoadScene("CenaMenu");
    }

    public void AbrirLoja()
    {
        SceneManager.LoadScene("CenaLoja");
    }
    
    public void AbrirRanking()
    {
        // Define a aba padrão (Quiz) para abrir
        RankingState.TabParaAbrir = RankingTab.Quiz;
        SceneManager.LoadScene("CenaRanking");
    }
}