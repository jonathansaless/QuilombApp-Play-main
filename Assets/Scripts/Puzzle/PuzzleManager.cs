using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class PuzzleManager : MonoBehaviour
{
    [Header("Identificação do Nível")]
    public string idDoNivel = "puzzle_nivel_1";

    [Header("Configuração do Puzzle")]
    public PuzzleData puzzleAtual;

    [Header("Referências da UI do Puzzle")]
    public Transform gridPanel;
    public Transform pecasEmbaralhadasPanel;
    public GameObject pecaPrefab;
    public GameObject slotVazioPrefab;
    public Image imagemReferencia;
    public TextMeshProUGUI textoMovimentos;

    [Header("Referências de Drag & Drop")]
    public Canvas canvasPrincipal;

    [Header("Referências da UI - HUD")]
    public TextMeshProUGUI textoCronometro;
    public TextMeshProUGUI textoMoedas;
    public TextMeshProUGUI textoVidas;

    [Header("Painéis de Confirmação")]
    public GameObject painelConfirmacaoSair;
    public GameObject painelConfirmacaoReiniciar;

    [Header("Modal - Overlay e Bloqueio")]
    public Button botaoOverlayModal;        // overlay fullscreen (Button + Image transparente)
    public CanvasGroup gameplayCanvasGroup; // CanvasGroup do conteúdo do jogo (SEM os painéis/overlay)

    private int movimentos = 0;
    private float tempoInicioPuzzle;
    private bool jogoAtivo = false;

    private bool jogoPausado = false;
    private float tempoPausadoAcumulado = 0f;
    private float tempoInicioPausa;

    private GameObject painelModalAtual = null;

    void Start()
    {
        if (GameDataHolder.NivelParaCarregar != null)
        {
            puzzleAtual = (PuzzleData)GameDataHolder.NivelParaCarregar;
            idDoNivel = GameDataHolder.NivelParaCarregar.idDoNivel;
        }
        else
        {
            Debug.LogWarning("Nenhum nível para carregar via GameDataHolder. Usando o nível definido no Inspector (Modo de Teste).");
        }

        if (PlayerDataManager.Instance == null || PlayerDataManager.Instance.Dados == null)
        {
            SceneManager.LoadScene("CenaLogin");
            return;
        }

        ConfigurarOverlay();
        FecharPainelConfirmacao(); // garante fechado e jogo interativo

        AtualizarHUD();
        InicializarPuzzle();
        ConfigurarPuzzle();
    }

    void Update()
    {
        if (jogoAtivo && !jogoPausado)
        {
            float tempoDecorrido = (Time.time - tempoInicioPuzzle) - tempoPausadoAcumulado;
            if (tempoDecorrido < 0) tempoDecorrido = 0;
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
            botaoOverlayModal.onClick.AddListener(FecharPainelConfirmacao); // ✅ clique fora fecha modal
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

        // fecha qualquer outro
        if (painelConfirmacaoSair != null) painelConfirmacaoSair.SetActive(false);
        if (painelConfirmacaoReiniciar != null) painelConfirmacaoReiniciar.SetActive(false);

        painelModalAtual = painel;
        painel.SetActive(true);

        if (botaoOverlayModal != null) botaoOverlayModal.gameObject.SetActive(true);

        // pausa e bloqueia gameplay
        jogoPausado = true;
        tempoInicioPausa = Time.time;
        SetGameplayInterativo(false);
    }

    void InicializarPuzzle()
    {
        movimentos = 0;
        textoMovimentos.text = $"Nº de Movimentos: {movimentos} / {puzzleAtual.NumeroMaxMovimentos}";
        jogoAtivo = true;
        tempoInicioPuzzle = Time.time;
        tempoPausadoAcumulado = 0f;
    }

    void ConfigurarPuzzle()
    {
        imagemReferencia.sprite = puzzleAtual.imagemReferencia;

        foreach (Transform child in gridPanel) { Destroy(child.gameObject); }
        foreach (Transform child in pecasEmbaralhadasPanel) { Destroy(child.gameObject); }

        List<GameObject> slotsParaEmbaralhar = new List<GameObject>();
        for (int i = 0; i < puzzleAtual.pecasDoPuzzle.Count; i++)
        {
            GameObject slotObj = Instantiate(slotVazioPrefab, gridPanel);
            slotObj.GetComponent<SlotDePeca>().manager = this;
            slotObj.GetComponent<SlotDePeca>().idDoSlot = i;
        }
        for (int i = 0; i < puzzleAtual.pecasDoPuzzle.Count; i++)
        {
            GameObject slotPecaObj = Instantiate(slotVazioPrefab, pecasEmbaralhadasPanel);
            slotPecaObj.GetComponent<SlotDePeca>().manager = this;
            slotPecaObj.GetComponent<SlotDePeca>().idDoSlot = -1;
            slotsParaEmbaralhar.Add(slotPecaObj);

            GameObject pecaObj = Instantiate(pecaPrefab, slotPecaObj.transform);
            pecaObj.GetComponent<Image>().sprite = puzzleAtual.pecasDoPuzzle[i];
            pecaObj.GetComponent<PecaArrastavel>().idDaPeca = i;
        }
        for (int i = 0; i < slotsParaEmbaralhar.Count; i++)
        {
            int randomIndex = Random.Range(i, slotsParaEmbaralhar.Count);
            slotsParaEmbaralhar[i].transform.SetSiblingIndex(randomIndex);
        }
        jogoAtivo = true;
    }

    public void PecaMovida(GameObject peca, SlotDePeca novoSlot)
    {
        if (!jogoAtivo) return;
        if (painelModalAtual != null) return; // ✅ extra: se modal aberto, ignora

        PecaArrastavel pecaArrastavel = peca.GetComponent<PecaArrastavel>();

        if (novoSlot.transform != pecaArrastavel.paiOriginal && novoSlot.idDoSlot != -1)
        {
            movimentos++;
            textoMovimentos.text = $"Nº de Movimentos: {movimentos} / {puzzleAtual.NumeroMaxMovimentos}";
        }

        VerificarCondicaoDeFimDeJogo();
    }

    void VerificarCondicaoDeFimDeJogo()
    {
        bool vitoria = true;
        if (gridPanel.childCount < puzzleAtual.pecasDoPuzzle.Count)
        {
            vitoria = false;
        }
        else
        {
            for (int i = 0; i < gridPanel.childCount; i++)
            {
                Transform slot = gridPanel.GetChild(i);
                if (slot.childCount == 0 || slot.GetChild(0).GetComponent<PecaArrastavel>().idDaPeca != i)
                {
                    vitoria = false;
                    break;
                }
            }
        }

        if (vitoria)
        {
            FimDeJogo(true);
            return;
        }

        if (movimentos >= puzzleAtual.NumeroMaxMovimentos)
        {
            FimDeJogo(false);
        }
    }

    void FimDeJogo(bool vitoria)
    {
        if (!jogoAtivo) return;
        jogoAtivo = false;

        float tempoTotal = (Time.time - tempoInicioPuzzle) - tempoPausadoAcumulado;

        PlayerPrefs.SetString("idDoNivelFinal", idDoNivel);
        PlayerPrefs.SetFloat("TempoFinal", tempoTotal);
        PlayerPrefs.SetInt("MovimentosFinal", movimentos);
        PlayerPrefs.SetString("ResultadoFinal", vitoria ? "vitoria" : "derrota");
        PlayerPrefs.SetInt("MovimentosMaximosDoNivel", puzzleAtual.NumeroMaxMovimentos);

        if (vitoria)
        {
            GameDataHolder.NivelParaCarregar = puzzleAtual;
            PlayerPrefs.SetString("CenaFeedbackDestino", "CenaFeedbackQuebraCabeca");
            SceneManager.LoadScene("CenaExplicacao");
        }
        else
        {
            SceneManager.LoadScene("CenaFeedbackQuebraCabeca");
        }

        PlayerPrefs.Save();
    }

    void AtualizarHUD()
    {
        if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.Dados != null)
        {
            textoMoedas.text = PlayerDataManager.Instance.Dados.Moedas.ToString();
            textoVidas.text = PlayerDataManager.Instance.Dados.Vidas.ToString();
        }
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
        // despausa e acumula tempo de pausa
        if (jogoPausado)
        {
            tempoPausadoAcumulado += Time.time - tempoInicioPausa;
        }
        jogoPausado = false;

        painelModalAtual = null;

        if (painelConfirmacaoSair != null) painelConfirmacaoSair.SetActive(false);
        if (painelConfirmacaoReiniciar != null) painelConfirmacaoReiniciar.SetActive(false);

        if (botaoOverlayModal != null) botaoOverlayModal.gameObject.SetActive(false);

        // reativa gameplay
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

    public void VitoriaParaTeste()
    {
        if (!jogoAtivo) return;
        FimDeJogo(true);
    }
}
