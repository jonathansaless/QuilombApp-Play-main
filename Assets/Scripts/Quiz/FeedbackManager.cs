using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class FeedbackManager : MonoBehaviour
{
    [Header("Painéis")]
    public GameObject painelPositivo;
    public GameObject painelNegativo;
    public GameObject painelSemVidas;

    [Header("Áudio de Feedback")]
    public AudioClip somFeedbackPositivo;
    public AudioClip somFeedbackNegativo;

    [Header("UI Painel Positivo")]
    public TextMeshProUGUI textoAcertosPositivo;
    public TextMeshProUGUI textoTempoPositivo;
    public TextMeshProUGUI textoMoedasGanhaas;
    public TextMeshProUGUI textoPontosGanhos;

    [Header("UI Painel Negativo")]
    public TextMeshProUGUI textoAcertosNegativo;
    public TextMeshProUGUI textoTempoNegativo;

    [Header("UI da Barra de Navegação Superior (NavBar)")]
    public TextMeshProUGUI textoMoedasTopbar;
    public TextMeshProUGUI textoVidasTopbar;

    [Header("Navegação - Retornar (QUIZ)")]
    [SerializeField] private string cenaNiveis = "CenaNiveis";
    [SerializeField] private ModoDeJogoData modoQuizData; // <- arraste aqui o ScriptableObject do Quiz

    private string ultimaCenaJogada;

    void Start()
    {
        if (painelSemVidas != null) painelSemVidas.SetActive(false);

        if (PlayerDataManager.Instance == null || PlayerDataManager.Instance.Dados == null)
        {
            SceneManager.LoadScene("CenaLogin");
            return;
        }

        ultimaCenaJogada = PlayerPrefs.GetString("UltimaCenaJogada");
        int acertos = PlayerPrefs.GetInt("AcertosFinal", 0);
        int totalPerguntas = PlayerPrefs.GetInt("TotalPerguntasFinal", 1);
        float tempoRestante = PlayerPrefs.GetFloat("TempoFinal", 0f);
        string idDoNivel = PlayerPrefs.GetString("idDoNivelFinal", "nivel_desconhecido");
        string resultado = PlayerPrefs.GetString("ResultadoFinal", "derrota");

        float tempoLimite = PlayerPrefs.GetFloat("TempoLimiteInicial", 300f);

        PlayerPrefs.DeleteKey("AcertosFinal");
        PlayerPrefs.DeleteKey("TotalPerguntasFinal");
        PlayerPrefs.DeleteKey("TempoFinal");
        PlayerPrefs.DeleteKey("idDoNivelFinal");
        PlayerPrefs.DeleteKey("ResultadoFinal");
        PlayerPrefs.DeleteKey("TempoLimiteInicial");

        float tempoGasto = tempoLimite - tempoRestante;
        if (tempoGasto < 0f) tempoGasto = 0f;
        string tempoFormatado = string.Format("{0:00}:{1:00}", (int)tempoGasto / 60, (int)tempoGasto % 60);

        if (resultado == "vitoria")
        {
            painelPositivo.SetActive(true);
            painelNegativo.SetActive(false);
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(somFeedbackPositivo);

            textoAcertosPositivo.text = $"{acertos}/{totalPerguntas} ACERTOS";
            textoTempoPositivo.text = tempoFormatado;

            int moedasGanhaas = 10 + (acertos * 5);
            int bonusDeTempo = (int)(tempoRestante * 2.0f);
            int pontuacaoFinal = 50 + (acertos * 100) + bonusDeTempo;

            textoMoedasGanhaas.text = moedasGanhaas + " moedas";
            textoPontosGanhos.text = pontuacaoFinal + " pontos";

            PlayerDataManager.Instance.Dados.Moedas += moedasGanhaas;

            PlayerDataManager.Instance.Dados.PontuacoesPorNivel.TryGetValue(idDoNivel, out long pontuacaoAntiga);
            if (pontuacaoFinal > pontuacaoAntiga)
            {
                long diferencaDePontos = pontuacaoFinal - pontuacaoAntiga;
                PlayerDataManager.Instance.Dados.PontuacaoTotalQuiz += diferencaDePontos;
                PlayerDataManager.Instance.Dados.PontuacoesPorNivel[idDoNivel] = pontuacaoFinal;
            }
        }
        else
        {
            painelPositivo.SetActive(false);
            painelNegativo.SetActive(true);
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(somFeedbackNegativo);

            textoAcertosNegativo.text = $"{acertos}/{totalPerguntas} ACERTOS";
            textoTempoNegativo.text = tempoFormatado;

            PlayerDataManager.Instance.Dados.Vidas--;
            if (PlayerDataManager.Instance.Dados.Vidas < 0) PlayerDataManager.Instance.Dados.Vidas = 0;
        }

        textoMoedasTopbar.text = PlayerDataManager.Instance.Dados.Moedas.ToString();
        textoVidasTopbar.text = PlayerDataManager.Instance.Dados.Vidas.ToString();
        PlayerDataManager.Instance.SalvarDadosNoFirebase();
    }

    public void TentarNovamente()
    {
        if (PlayerDataManager.Instance.Dados.Vidas < 1)
        {
            if (painelSemVidas != null) painelSemVidas.SetActive(true);
            return;
        }

        if (!string.IsNullOrEmpty(ultimaCenaJogada))
        {
            SceneManager.LoadScene(ultimaCenaJogada);
        }
        else
        {
            Debug.LogError("Não foi possível encontrar a última cena jogada! Voltando aos níveis do Quiz.");
            RetornarAoMenu(); // cai no método abaixo (que agora volta aos níveis)
        }
    }

    public void RetornarAoMenu()
    {
        GameDataHolder.NivelParaCarregar = null;

        if (modoQuizData == null)
        {
            Debug.LogError("modoQuizData não foi definido no FeedbackManager (Quiz). Voltando ao menu principal por segurança.");
            SceneManager.LoadScene("CenaMenu");
            return;
        }

        GameDataHolder.ModoDeJogoParaCarregar = modoQuizData;
        SceneManager.LoadScene(cenaNiveis);
    }

    public void IrParaPlacar()
    {
        GameDataHolder.NivelParaCarregar = null;
        RankingState.TabParaAbrir = RankingTab.Quiz;
        SceneManager.LoadScene("CenaRanking");
    }

    public void FecharPainelSemVidas()
    {
        if (painelSemVidas != null) painelSemVidas.SetActive(false);
    }
}
