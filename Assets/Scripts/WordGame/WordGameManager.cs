using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.SceneManagement;
using System.Text;

public class WordGameManager : MonoBehaviour
{
    [Header("Identificação do Nível")]
    public string idDoNivel = "palavras_nivel_1";

    [Header("Configuração do Nível")]
    public WordGameData nivelAtual;

    [Header("Referências da UI - Jogo")]
    public TextMeshProUGUI textoPerguntaAvatar;
    public Image imagemDica;
    public Transform painelSlotsResposta;
    public Transform painelLetrasDisponiveis;

    [Header("Prefabs")]
    public GameObject slotLetraPrefab;
    public GameObject slotRespostaPrefab;
    public GameObject letraPrefab;

    [Header("Referências da UI - HUD")]
    public TextMeshProUGUI textoMovimentos;
    public TextMeshProUGUI textoCronometro;
    public TextMeshProUGUI textoMoedas;
    public TextMeshProUGUI textoVidas;

    [Header("Painéis de Confirmação")]
    public GameObject painelConfirmacaoSair;
    public GameObject painelConfirmacaoReiniciar;

    [Header("Modal - Overlay e Bloqueio")]
    public Button botaoOverlayModal;        // overlay fullscreen
    public CanvasGroup gameplayCanvasGroup; // CanvasGroup do conteúdo do jogo (SEM os painéis/overlay)

    private int movimentos = 0;
    private float tempoInicioJogo;
    private bool jogoAtivo = false;

    private bool jogoPausado = false;
    private float tempoPausadoAcumulado = 0f;
    private float tempoInicioPausa;

    private GameObject painelModalAtual = null;

    void Start()
    {
        if (GameDataHolder.NivelParaCarregar != null)
        {
            nivelAtual = (WordGameData)GameDataHolder.NivelParaCarregar;
            idDoNivel = GameDataHolder.NivelParaCarregar.idDoNivel;
        }
        else
        {
            Debug.LogWarning("Nenhum nível para carregar via GameDataHolder. Usando o nível definido no Inspector (Modo de Teste).");
        }

        if (PlayerDataManager.Instance == null || PlayerDataManager.Instance.Dados == null)
        {
            Debug.LogError("PlayerDataManager não encontrado! Voltando para a cena de Login.");
            SceneManager.LoadScene("CenaLogin");
            return;
        }

        ConfigurarOverlay();
        FecharPainelConfirmacao();

        AtualizarHUD();
        InicializarJogo();
        ConfigurarNivel();
    }

    void Update()
    {
        if (jogoAtivo && !jogoPausado)
        {
            float tempoDecorrido = (Time.time - tempoInicioJogo) - tempoPausadoAcumulado;
            int minutos = (int)tempoDecorrido / 60;
            int segundos = (int)tempoDecorrido % 60;
            textoCronometro.text = string.Format("{0:00}:{1:00}", minutos, segundos);
        }
    }

    void ConfigurarOverlay()
    {
        if (botaoOverlayModal != null)
        {
            botaoOverlayModal.gameObject.SetActive(false);
            botaoOverlayModal.onClick.RemoveAllListeners();
            botaoOverlayModal.onClick.AddListener(FecharPainelConfirmacao);
        }
    }

    void SetGameplayInterativo(bool ativo)
    {
        if (gameplayCanvasGroup == null) return;
        gameplayCanvasGroup.interactable = ativo;
        gameplayCanvasGroup.blocksRaycasts = ativo;
    }

    void AbrirModal(GameObject painel)
    {
        if (painel == null) return;

        if (painelConfirmacaoSair != null) painelConfirmacaoSair.SetActive(false);
        if (painelConfirmacaoReiniciar != null) painelConfirmacaoReiniciar.SetActive(false);

        painelModalAtual = painel;
        painel.SetActive(true);

        if (botaoOverlayModal != null) botaoOverlayModal.gameObject.SetActive(true);

        jogoPausado = true;
        tempoInicioPausa = Time.time;

        SetGameplayInterativo(false);
    }

    void InicializarJogo()
    {
        movimentos = 0;
        textoMovimentos.text = $"Nº de Movimentos: {movimentos} / {nivelAtual.NumeroMaxMovimentos}";
        jogoAtivo = true;
        tempoInicioJogo = Time.time;

        jogoPausado = false;
        tempoPausadoAcumulado = 0f;
        tempoInicioPausa = 0f;
    }

    void ConfigurarNivel()
    {
        foreach (Transform child in painelSlotsResposta) { Destroy(child.gameObject); }
        foreach (Transform child in painelLetrasDisponiveis) { Destroy(child.gameObject); }

        textoPerguntaAvatar.text = nivelAtual.textoDaPergunta;
        imagemDica.sprite = nivelAtual.imagemDeDica;

        for (int i = 0; i < nivelAtual.respostaCorreta.Length; i++)
        {
            GameObject slotObj = Instantiate(slotRespostaPrefab, painelSlotsResposta);
            slotObj.GetComponent<SlotDeLetra>().manager = this;
        }

        var letras = nivelAtual.letrasParaEmbaralhar.ToList();
        System.Random rng = new System.Random();
        letras = letras.OrderBy(a => rng.Next()).ToList();

        foreach (char letraChar in letras)
        {
            GameObject slotOrigem = Instantiate(slotLetraPrefab, painelLetrasDisponiveis);
            slotOrigem.GetComponent<SlotDeLetra>().manager = this;

            GameObject letraObj = Instantiate(letraPrefab, slotOrigem.transform);
            letraObj.GetComponentInChildren<TextMeshProUGUI>().text = letraChar.ToString();
            letraObj.GetComponent<LetraArrastavel>().letra = letraChar;
        }
    }

    public void LetraMovida()
    {
        if (!jogoAtivo) return;
        if (painelModalAtual != null) return; // ✅ extra

        movimentos++;
        textoMovimentos.text = $"Nº de Movimentos: {movimentos} / {nivelAtual.NumeroMaxMovimentos}";

        VerificarCondicaoDeFimDeJogo();
    }

    void VerificarCondicaoDeFimDeJogo()
    {
        if (movimentos >= nivelAtual.NumeroMaxMovimentos)
        {
            FimDeJogo(false);
            return;
        }

        foreach (Transform slot in painelSlotsResposta)
        {
            if (slot.childCount == 0) return;
        }

        StringBuilder palavraFormadaBuilder = new StringBuilder();
        foreach (Transform slot in painelSlotsResposta)
        {
            palavraFormadaBuilder.Append(slot.GetChild(0).GetComponent<LetraArrastavel>().letra);
        }

        string palavraFormada = palavraFormadaBuilder.ToString().Trim();
        string respostaCorreta = nivelAtual.respostaCorreta.Trim();

        if (palavraFormada.Equals(respostaCorreta, System.StringComparison.InvariantCultureIgnoreCase))
        {
            FimDeJogo(true);
        }
    }

    void FimDeJogo(bool vitoria)
    {
        if (!jogoAtivo) return;
        jogoAtivo = false;

        float tempoTotal = (Time.time - tempoInicioJogo) - tempoPausadoAcumulado;

        PlayerPrefs.SetString("idDoNivelFinal", idDoNivel);
        PlayerPrefs.SetInt("MovimentosFinal", movimentos);
        PlayerPrefs.SetFloat("TempoFinal", tempoTotal);
        PlayerPrefs.SetString("ResultadoFinal", vitoria ? "vitoria" : "derrota");
        PlayerPrefs.SetInt("MovimentosMaximosDoNivel", nivelAtual.NumeroMaxMovimentos);

        if (vitoria)
        {
            GameDataHolder.NivelParaCarregar = nivelAtual;
            PlayerPrefs.SetString("CenaFeedbackDestino", "CenaFeedbackJogoDePalavras");
            SceneManager.LoadScene("CenaExplicacao");
        }
        else
        {
            SceneManager.LoadScene("CenaFeedbackJogoDePalavras");
        }

        PlayerPrefs.Save();
    }

    void AtualizarHUD()
    {
        if (textoMoedas != null) textoMoedas.text = PlayerDataManager.Instance.Dados.Moedas.ToString();
        if (textoVidas != null) textoVidas.text = PlayerDataManager.Instance.Dados.Vidas.ToString();
    }

    public void MostrarConfirmacaoSair()
    {
        if (!jogoAtivo) return;
        AbrirModal(painelConfirmacaoSair);
    }

    public void MostrarConfirmacaoReiniciar()
    {
        if (!jogoAtivo) return;
        AbrirModal(painelConfirmacaoReiniciar);
    }

    public void FecharPainelConfirmacao()
    {
        if (jogoPausado)
        {
            tempoPausadoAcumulado += Time.time - tempoInicioPausa;
        }
        jogoPausado = false;

        painelModalAtual = null;

        if (painelConfirmacaoSair != null) painelConfirmacaoSair.SetActive(false);
        if (painelConfirmacaoReiniciar != null) painelConfirmacaoReiniciar.SetActive(false);

        if (botaoOverlayModal != null) botaoOverlayModal.gameObject.SetActive(false);

        SetGameplayInterativo(true);
    }

    public void ConfirmarSair()
    {
        FecharPainelConfirmacao();
        SceneManager.LoadScene("CenaMenu");
    }

    public void ConfirmarReiniciar()
    {
        FecharPainelConfirmacao();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
