using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Firebase.Auth; 
using System.Collections.Generic;

public class MenuManager : MonoBehaviour
{
    [Header("UI da NavBar")]
    public CanvasGroup menuCanvasGroup;
    public TextMeshProUGUI textoMoedas;
    public TextMeshProUGUI textoVidas;
    public TextMeshProUGUI textoApelido; 
    public Image iconeAvatarJogador;

    [Header("Dados dos Avatares")]
    public AvatarDatabase avatarDatabase;

    [Header("Dados dos Modos de Jogo")]
    public ModoDeJogoData modoQuizData;
    public ModoDeJogoData modoPuzzleData;
    public ModoDeJogoData modoWordGameData;

    void Start()
    {
        // Garante que a UI esteja invisível desde o início
        if (menuCanvasGroup != null)
        {
            menuCanvasGroup.alpha = 0;
            menuCanvasGroup.interactable = false;
        }

        // Verifica se os dados do jogador existem, senão volta para o Login
        if (PlayerDataManager.Instance == null || PlayerDataManager.Instance.Dados == null)
        {
            SceneManager.LoadScene("CenaLogin");
            return;
        }

        // Aqui só usamos os dados que já foram carregados e ajustados no Splash
        AtualizarNavBar();

        // Torna a UI visível já com vidas e moedas corretas
        if (menuCanvasGroup != null)
        {
            menuCanvasGroup.alpha = 1;
            menuCanvasGroup.interactable = true;
        }
    }


    public void AtualizarNavBar()
    {
        var dados = PlayerDataManager.Instance.Dados;
        textoMoedas.text = dados.Moedas.ToString();
        textoVidas.text = dados.Vidas.ToString();
        textoApelido.text = dados.Apelido;

        // --- LÓGICA PARA EXIBIR O AVATAR EQUIPADO ---
        int idEquipado = PlayerDataManager.Instance.Dados.AvatarEquipadoID;
        Sprite spriteEquipado = avatarDatabase.EncontrarSpriteDoAvatarPeloID(idEquipado);
        if (spriteEquipado != null)
        {
            iconeAvatarJogador.sprite = spriteEquipado;
        }
    }

    public void IniciarQuiz()
    {
        if (modoQuizData == null) 
        {
            Debug.LogError("ModoDeJogoData para o Quiz não foi definido no MenuManager!");
            return;
        }
        GameDataHolder.ModoDeJogoParaCarregar = modoQuizData;
        SceneManager.LoadScene("CenaNiveis"); // O nome da sua cena de seleção de níveis
    }

    public void IniciarQuebraCabeca()
    {
        if (modoPuzzleData == null) 
        {
            Debug.LogError("ModoDeJogoData para o Puzzle não foi definido no MenuManager!");
            return;
        }
        GameDataHolder.ModoDeJogoParaCarregar = modoPuzzleData;
        SceneManager.LoadScene("CenaNiveis");
    }

    public void IniciarJogoDePalavras()
    {
        if (modoWordGameData == null)
        {
            Debug.LogError("ModoDeJogoData para o Jogo de Palavras não foi definido no MenuManager!");
            return;
        }
        GameDataHolder.ModoDeJogoParaCarregar = modoWordGameData;
        SceneManager.LoadScene("CenaNiveis");
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
    
    public void AbrirPerfil()
    {
        SceneManager.LoadScene("CenaPerfil");
    }

    public void AbrirConfiguracoes()
    {
        SceneManager.LoadScene("CenaConfiguracoes");
    }
    
}