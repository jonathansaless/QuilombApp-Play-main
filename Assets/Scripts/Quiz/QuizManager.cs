using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System.Linq;

public class QuizManager : MonoBehaviour
{
    #region Variáveis e Referências

    [Header("Identificação do Nível")]
    public string idDoNivel;

    private List<Pergunta> listaDePerguntas;
    private Pergunta perguntaAtual;
    private int indicePerguntaAtual = 0;

    private int respostasCorretasCount = 0;
    private float tempoRestante;
    private bool jogoEmAndamento = false;

    [Header("Referências de UI - Jogo")]
    public TextMeshProUGUI textoPergunta;
    public Image imagemPergunta;
    public Button[] botoesResposta;

    [Header("Referências de UI - HUD")]
    public TextMeshProUGUI textoNumeroPergunta;
    public TextMeshProUGUI textoCronometro;
    public TextMeshProUGUI textoMoedas;
    public TextMeshProUGUI textoVidas;

    [Header("Referências de UI - Feedback na Pergunta")]
    public GameObject painelCorreto;
    public GameObject painelErrado;

    [Header("Configurações de Feedback Visual")]
    public Color corCorreta = Color.green;
    public Color corErrada = Color.red;
    public float tempoDeEspera = 1f;

    [Header("Áudio de Feedback")]
    public AudioClip somDeAcerto;
    public AudioClip somDeErro;

    [Header("Painéis de Confirmação")]
    public GameObject painelConfirmacaoSair;
    public GameObject painelConfirmacaoReiniciar;
    public GameObject painelConfirmacaoDica;
    public GameObject painelConfirmacaoDuasChances;
    public GameObject painelConfirmacao5050;

    [Header("UI dos Poderes")]
    public GameObject painelDica;
    public TextMeshProUGUI textoDica;
    public Button botaoPowerUpDica;
    public Button botaoPowerUpDuasChances;
    public Button botaoPowerUp5050;
    public TextMeshProUGUI textoQuantidadeDica;
    public TextMeshProUGUI textoQuantidadeDuasChances;
    public TextMeshProUGUI textoQuantidade5050;

    [Header("Overlay Modal (cobre a tela e fecha ao clicar fora)")]
    public Button botaoOverlayModal;

    // (opcional) compatibilidade se você já tinha overlay só da dica
    [Header("Opcional/Legado")]
    public Button botaoOverlayFecharDica;

    private bool jogoPausado = false;
    private float tempoPausadoAcumulado = 0f;
    private float tempoInicioPausa;

    private bool poderUsadoNestaPergunta = false;
    private bool duasChancesAtivoNestaPergunta = false;
    private int cliquesNestaPergunta = 0;

    // --- CONTROLE DE MODAL ---
    private GameObject painelModalAtual = null;
    private bool modalAtualPausouJogo = false;

    private Button OverlayAtivo => botaoOverlayModal != null ? botaoOverlayModal : botaoOverlayFecharDica;
    private bool TemModalAberto => painelModalAtual != null;

    #endregion

    #region Ciclo de Vida da Unity (Start, Update)

    void Start()
    {
        if (GameDataHolder.NivelParaCarregar != null)
        {
            var quizData = (QuizData)GameDataHolder.NivelParaCarregar;
            listaDePerguntas = new List<Pergunta>(quizData.listaDePerguntas);
            idDoNivel = GameDataHolder.NivelParaCarregar.idDoNivel;
            tempoRestante = quizData.tempoLimiteEmSegundos;
        }
        else
        {
            Debug.LogError("Nenhum nível para carregar! Inicie pela Cena de Níveis.");
            SceneManager.LoadScene("CenaMenu");
            return;
        }

        if (listaDePerguntas == null || listaDePerguntas.Count == 0)
        {
            Debug.LogError($"O nível '{idDoNivel}' não tem perguntas configuradas!");
            SceneManager.LoadScene("CenaMenu");
            return;
        }

        if (PlayerDataManager.Instance == null || PlayerDataManager.Instance.Dados == null)
        {
            SceneManager.LoadScene("CenaLogin");
            return;
        }

        // Garante painéis fechados
        FecharTodosOsPaineisSemDespausar();
        ConfigurarOverlayModal();

        AtualizarHUD();
        AtualizarUIPoderes();
        InicializarQuiz();
        EmbaralharPerguntas();
        CarregarProximaPergunta();
    }

    void Update()
    {
        if (jogoEmAndamento && !jogoPausado)
        {
            tempoRestante -= Time.deltaTime;

            if (tempoRestante <= 0)
            {
                tempoRestante = 0;
                textoCronometro.text = "00:00";
                FimDeJogo();
            }
            else
            {
                int minutos = (int)tempoRestante / 60;
                int segundos = (int)tempoRestante % 60;
                textoCronometro.text = string.Format("{0:00}:{1:00}", minutos, segundos);
            }
        }
    }

    #endregion

    #region Modal (Overlay + abrir/fechar painéis)

    private void ConfigurarOverlayModal()
    {
        var overlay = OverlayAtivo;
        if (overlay != null)
        {
            overlay.gameObject.SetActive(false);
            overlay.onClick.RemoveAllListeners();
            overlay.onClick.AddListener(FecharModalAtual); // ✅ clique fora fecha o painel aberto
        }
    }

    private void AbrirModal(GameObject painel, bool pausarJogo)
    {
        if (painel == null) return;

        // Se já tem outro painel aberto, fecha antes
        if (TemModalAberto) FecharModalAtual();

        painelModalAtual = painel;
        modalAtualPausouJogo = pausarJogo;

        if (pausarJogo && !jogoPausado)
        {
            jogoPausado = true;
            tempoInicioPausa = Time.time;
        }

        // Fecha tudo por segurança e abre só o painel desejado
        FecharTodosOsPaineisSemDespausar();
        painel.SetActive(true);

        var overlay = OverlayAtivo;
        if (overlay != null) overlay.gameObject.SetActive(true);
    }

    public void FecharModalAtual()
    {
        if (!TemModalAberto)
        {
            var overlay = OverlayAtivo;
            if (overlay != null) overlay.gameObject.SetActive(false);
            return;
        }

        // Fecha todos os painéis (garante que nada fica “vazando”)
        FecharTodosOsPaineisSemDespausar();

        // Despausa se este modal pausou
        if (modalAtualPausouJogo && jogoPausado)
        {
            tempoPausadoAcumulado += Time.time - tempoInicioPausa;
            jogoPausado = false;
        }

        painelModalAtual = null;
        modalAtualPausouJogo = false;

        var ov = OverlayAtivo;
        if (ov != null) ov.gameObject.SetActive(false);
    }

    private void FecharTodosOsPaineisSemDespausar()
    {
        if (painelDica != null) painelDica.SetActive(false);

        if (painelConfirmacaoSair != null) painelConfirmacaoSair.SetActive(false);
        if (painelConfirmacaoReiniciar != null) painelConfirmacaoReiniciar.SetActive(false);
        if (painelConfirmacaoDica != null) painelConfirmacaoDica.SetActive(false);
        if (painelConfirmacaoDuasChances != null) painelConfirmacaoDuasChances.SetActive(false);
        if (painelConfirmacao5050 != null) painelConfirmacao5050.SetActive(false);
    }

    #endregion

    #region Lógica Principal do Quiz

    void InicializarQuiz()
    {
        respostasCorretasCount = 0;
        indicePerguntaAtual = 0;
        jogoEmAndamento = true;

        if (GameDataHolder.NivelParaCarregar != null)
        {
            tempoRestante = ((QuizData)GameDataHolder.NivelParaCarregar).tempoLimiteEmSegundos;
        }

        jogoPausado = false;

        poderUsadoNestaPergunta = false;
        duasChancesAtivoNestaPergunta = false;
        cliquesNestaPergunta = 0;
        AtualizarUIPoderes();

        FecharModalAtual();
    }

    void CarregarProximaPergunta()
    {
        // ✅ garante que nenhum painel fica aberto passando de pergunta
        if (TemModalAberto) FecharModalAtual();

        poderUsadoNestaPergunta = false;
        duasChancesAtivoNestaPergunta = false;
        cliquesNestaPergunta = 0;
        AtualizarUIPoderes();

        for (int i = 0; i < botoesResposta.Length; i++)
        {
            botoesResposta[i].GetComponent<Image>().color = Color.white;
            botoesResposta[i].interactable = true;
        }

        if (indicePerguntaAtual < listaDePerguntas.Count)
        {
            perguntaAtual = listaDePerguntas[indicePerguntaAtual];
            textoPergunta.text = perguntaAtual.textoDaPergunta;
            textoNumeroPergunta.text = (indicePerguntaAtual + 1).ToString();

            if (perguntaAtual.imagemDaPergunta != null)
            {
                imagemPergunta.sprite = perguntaAtual.imagemDaPergunta;
                imagemPergunta.gameObject.SetActive(true);
            }
            else
            {
                imagemPergunta.gameObject.SetActive(false);
            }

            for (int i = 0; i < botoesResposta.Length; i++)
            {
                botoesResposta[i].GetComponentInChildren<TextMeshProUGUI>().text = perguntaAtual.respostas[i];
            }
        }
        else
        {
            FimDeJogo();
        }
    }

    public void RespostaSelecionada(int indiceDoBotao)
    {
        // ✅ se tem painel aberto, não permite responder
        if (TemModalAberto) return;

        StartCoroutine(ProcessarRespostaCoroutine(indiceDoBotao));
    }

    IEnumerator ProcessarRespostaCoroutine(int indiceSelecionado)
    {
        bool acertou = (indiceSelecionado == perguntaAtual.indiceRespostaCorreta);
        Image imagemBotaoSelecionado = botoesResposta[indiceSelecionado].GetComponent<Image>();

        if (duasChancesAtivoNestaPergunta)
        {
            cliquesNestaPergunta++;
            if (acertou)
            {
                for (int i = 0; i < botoesResposta.Length; i++) { botoesResposta[i].interactable = false; }
                if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(somDeAcerto);
                respostasCorretasCount++;
                imagemBotaoSelecionado.color = corCorreta;
                painelCorreto.SetActive(true);
            }
            else
            {
                imagemBotaoSelecionado.color = corErrada;
                botoesResposta[indiceSelecionado].interactable = false;

                if (cliquesNestaPergunta >= 2)
                {
                    if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(somDeErro);
                    Image imagemBotaoCorreto = botoesResposta[perguntaAtual.indiceRespostaCorreta].GetComponent<Image>();
                    imagemBotaoCorreto.color = corCorreta;
                    painelErrado.SetActive(true);
                }
                else
                {
                    yield break;
                }
            }
        }
        else
        {
            for (int i = 0; i < botoesResposta.Length; i++) { botoesResposta[i].interactable = false; }

            if (acertou)
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(somDeAcerto);
                respostasCorretasCount++;
                imagemBotaoSelecionado.color = corCorreta;
                painelCorreto.SetActive(true);
            }
            else
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(somDeErro);
                imagemBotaoSelecionado.color = corErrada;
                Image imagemBotaoCorreto = botoesResposta[perguntaAtual.indiceRespostaCorreta].GetComponent<Image>();
                imagemBotaoCorreto.color = corCorreta;
                painelErrado.SetActive(true);
            }
        }

        yield return new WaitForSeconds(tempoDeEspera);

        painelCorreto.SetActive(false);
        painelErrado.SetActive(false);

        indicePerguntaAtual++;
        CarregarProximaPergunta();
    }

    void EmbaralharPerguntas()
    {
        for (int i = 0; i < listaDePerguntas.Count; i++)
        {
            Pergunta temp = listaDePerguntas[i];
            int randomIndex = Random.Range(i, listaDePerguntas.Count);
            listaDePerguntas[i] = listaDePerguntas[randomIndex];
            listaDePerguntas[randomIndex] = temp;
        }
    }

    void FimDeJogo()
    {
        if (!jogoEmAndamento) return;
        jogoEmAndamento = false;

        float tempoTotal = tempoRestante;
        float performance = (float)respostasCorretasCount / listaDePerguntas.Count;

        bool vitoria = (performance >= 0.5f) && (tempoTotal > 0);

        PlayerPrefs.SetInt("AcertosFinal", respostasCorretasCount);
        PlayerPrefs.SetInt("TotalPerguntasFinal", listaDePerguntas.Count);
        PlayerPrefs.SetFloat("TempoFinal", tempoTotal);

        if (GameDataHolder.NivelParaCarregar != null)
        {
            PlayerPrefs.SetFloat("TempoLimiteInicial", ((QuizData)GameDataHolder.NivelParaCarregar).tempoLimiteEmSegundos);
        }

        PlayerPrefs.SetString("idDoNivelFinal", idDoNivel);
        PlayerPrefs.SetString("ResultadoFinal", vitoria ? "vitoria" : "derrota");

        PlayerPrefs.SetString("UltimaCenaJogada", SceneManager.GetActiveScene().name);
        PlayerPrefs.Save();

        SceneManager.LoadScene("CenaFeedback");
    }

    #endregion

    #region Lógica dos Poderes

    public void AtivarDica()
    {
        if (PlayerDataManager.Instance.Dados.QuantidadeDica <= 0 || poderUsadoNestaPergunta) return;

        PlayerDataManager.Instance.Dados.QuantidadeDica--;
        poderUsadoNestaPergunta = true;
        AtualizarUIPoderes();

        textoDica.text = perguntaAtual.dica;

        // ✅ dica vira modal: bloqueia fora + fecha clicando fora
        AbrirModal(painelDica, false);
    }

    public void Ativar5050()
    {
        if (PlayerDataManager.Instance.Dados.Quantidade5050 <= 0 || poderUsadoNestaPergunta) return;
        PlayerDataManager.Instance.Dados.Quantidade5050--;
        poderUsadoNestaPergunta = true;
        AtualizarUIPoderes();

        List<int> indicesIncorretos = new List<int>();
        for (int i = 0; i < botoesResposta.Length; i++)
        {
            if (i != perguntaAtual.indiceRespostaCorreta) indicesIncorretos.Add(i);
        }

        for (int i = 0; i < indicesIncorretos.Count; i++)
        {
            int temp = indicesIncorretos[i];
            int randomIndex = Random.Range(i, indicesIncorretos.Count);
            indicesIncorretos[i] = indicesIncorretos[randomIndex];
            indicesIncorretos[randomIndex] = temp;
        }

        botoesResposta[indicesIncorretos[0]].interactable = false;
        botoesResposta[indicesIncorretos[0]].GetComponent<Image>().color = Color.grey;
        botoesResposta[indicesIncorretos[1]].interactable = false;
        botoesResposta[indicesIncorretos[1]].GetComponent<Image>().color = Color.grey;
    }

    public void AtivarDuasChances()
    {
        if (PlayerDataManager.Instance.Dados.QuantidadeDuasChances <= 0 || poderUsadoNestaPergunta) return;
        PlayerDataManager.Instance.Dados.QuantidadeDuasChances--;
        poderUsadoNestaPergunta = true;
        AtualizarUIPoderes();
        duasChancesAtivoNestaPergunta = true;
    }

    #endregion

    #region Funções de UI

    void AtualizarHUD()
    {
        textoMoedas.text = PlayerDataManager.Instance.Dados.Moedas.ToString();
        textoVidas.text = PlayerDataManager.Instance.Dados.Vidas.ToString();
    }

    void AtualizarUIPoderes()
    {
        var dados = PlayerDataManager.Instance.Dados;
        textoQuantidadeDica.text = dados.QuantidadeDica.ToString() + "x";
        textoQuantidadeDuasChances.text = dados.QuantidadeDuasChances.ToString() + "x";
        textoQuantidade5050.text = dados.Quantidade5050.ToString() + "x";

        if (poderUsadoNestaPergunta)
        {
            botaoPowerUpDica.interactable = false;
            botaoPowerUpDuasChances.interactable = false;
            botaoPowerUp5050.interactable = false;
        }
        else
        {
            botaoPowerUpDica.interactable = (dados.QuantidadeDica > 0);
            botaoPowerUpDuasChances.interactable = (dados.QuantidadeDuasChances > 0);
            botaoPowerUp5050.interactable = (dados.Quantidade5050 > 0);
        }
    }

    // ✅ TODOS os painéis de confirmação viram “modais” com overlay
    public void MostrarConfirmacaoSair() { if (!jogoEmAndamento) return; AbrirModal(painelConfirmacaoSair, true); }
    public void MostrarConfirmacaoReiniciar() { if (!jogoEmAndamento) return; AbrirModal(painelConfirmacaoReiniciar, true); }
    public void MostrarConfirmacaoDica() { if (!jogoEmAndamento) return; AbrirModal(painelConfirmacaoDica, true); }
    public void MostrarConfirmacaoDuasChances() { if (!jogoEmAndamento) return; AbrirModal(painelConfirmacaoDuasChances, true); }
    public void MostrarConfirmacao5050() { if (!jogoEmAndamento) return; AbrirModal(painelConfirmacao5050, true); }

    // Botão "Não/Cancelar" (e clique fora também)
    public void FecharPainelConfirmacao() { FecharModalAtual(); }

    // Botão OK da dica (também fecha)
    public void FecharPainelDica() { FecharModalAtual(); }

    // Confirmações (botão "Sim")
    public void ConfirmarSair()
    {
        FecharModalAtual();
        SceneManager.LoadScene("CenaMenu");
    }

    public void ConfirmarReiniciar()
    {
        FecharModalAtual();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ConfirmarDica()
    {
        FecharModalAtual();
        AtivarDica();
    }

    public void ConfirmarDuasChances()
    {
        FecharModalAtual();
        AtivarDuasChances();
    }

    public void Confirmar5050()
    {
        FecharModalAtual();
        Ativar5050();
    }

    #endregion
}
