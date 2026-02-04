using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class FeedbackWordGameManager : MonoBehaviour
{
    [Header("Painéis")]
    public GameObject painelPositivo;
    public GameObject painelNegativo;
    public GameObject painelSemVidas;

    [Header("Áudio de Feedback")]
    public AudioClip somFeedbackPositivo;
    public AudioClip somFeedbackNegativo;

    [Header("UI Painel Positivo")]
    public TextMeshProUGUI textoMovimentosPositivo;
    public TextMeshProUGUI textoTempoPositivo;
    public TextMeshProUGUI textoMoedasGanhaas;
    public TextMeshProUGUI textoPontosGanhos;

    [Header("UI Painel Negativo")]
    public TextMeshProUGUI textoMovimentosNegativo;
    public TextMeshProUGUI textoTempoNegativo;

    [Header("UI da Barra de Navegação Superior (NavBar)")]
    public TextMeshProUGUI textoMoedasTopbar;
    public TextMeshProUGUI textoVidasTopbar;

    [Header("Navegação - Retornar (WORDGAME)")]
    [SerializeField] private string cenaNiveis = "CenaNiveis";
    [SerializeField] private ModoDeJogoData modoWordGameData; // <- arraste aqui o ScriptableObject do WordGame

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
        int movimentos = PlayerPrefs.GetInt("MovimentosFinal", 0);
        float tempoFinal = PlayerPrefs.GetFloat("TempoFinal", 0f);
        string idDoNivel = PlayerPrefs.GetString("idDoNivelFinal", "nivel_desconhecido");
        string resultado = PlayerPrefs.GetString("ResultadoFinal", "derrota");
        int movimentosMaximosDoNivel = PlayerPrefs.GetInt("MovimentosMaximosDoNivel", 30);

        PlayerPrefs.DeleteKey("MovimentosFinal");
        PlayerPrefs.DeleteKey("TempoFinal");
        PlayerPrefs.DeleteKey("idDoNivelFinal");
        PlayerPrefs.DeleteKey("ResultadoFinal");
        PlayerPrefs.DeleteKey("MovimentosMaximosDoNivel");

        string tempoFormatado = string.Format("{0:00}:{1:00}", (int)tempoFinal / 60, (int)tempoFinal % 60);

        if (resultado == "vitoria")
        {
            painelPositivo.SetActive(true);
            painelNegativo.SetActive(false);

            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(somFeedbackPositivo);

            textoMovimentosPositivo.text = "Nº DE MOVIMENTOS: " + movimentos.ToString();
            textoTempoPositivo.text = tempoFormatado;

            int moedasGanhaas = 25 + (int)Mathf.Max(0, (movimentosMaximosDoNivel - movimentos) * 0.5f);
            if (moedasGanhaas > 60) moedasGanhaas = 60;

            int bonusPorMovimentos = (movimentosMaximosDoNivel - movimentos) * 7;
            int bonusPorTempo = (int)Mathf.Max(0, 120 - tempoFinal) * 3;
            int pontuacaoFinal = 100 + bonusPorMovimentos + bonusPorTempo;

            textoMoedasGanhaas.text = moedasGanhaas.ToString() + " moedas";
            textoPontosGanhos.text = pontuacaoFinal.ToString() + " pontos";

            PlayerDataManager.Instance.Dados.Moedas += moedasGanhaas;
            PlayerDataManager.Instance.Dados.PontuacoesPorNivel.TryGetValue(idDoNivel, out long pontuacaoAntiga);

            if (pontuacaoFinal > pontuacaoAntiga)
            {
                long diferencaDePontos = pontuacaoFinal - pontuacaoAntiga;
                PlayerDataManager.Instance.Dados.PontuacaoTotalWordGame += diferencaDePontos;
                PlayerDataManager.Instance.Dados.PontuacoesPorNivel[idDoNivel] = pontuacaoFinal;
            }
        }
        else
        {
            painelNegativo.SetActive(true);
            painelPositivo.SetActive(false);

            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(somFeedbackNegativo);

            textoMovimentosNegativo.text = "Nº MÁXIMO DE MOVIMENTOS ATINGIDO";
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
            Debug.LogError("Não foi possível encontrar a última cena jogada! Voltando aos níveis do WordGame.");
            RetornarAoMenu();
        }
    }

    // ✅ MUDANÇA: volta para os níveis do WORDGAME
    public void RetornarAoMenu()
    {
        GameDataHolder.NivelParaCarregar = null;

        if (modoWordGameData == null)
        {
            Debug.LogError("modoWordGameData não foi definido no FeedbackWordGameManager. Voltando ao menu principal por segurança.");
            SceneManager.LoadScene("CenaMenu");
            return;
        }

        GameDataHolder.ModoDeJogoParaCarregar = modoWordGameData;
        SceneManager.LoadScene(cenaNiveis);
    }

    public void IrParaPlacar()
    {
        GameDataHolder.NivelParaCarregar = null;
        RankingState.TabParaAbrir = RankingTab.WordGame;
        SceneManager.LoadScene("CenaRanking");
    }

    public void FecharPainelSemVidas()
    {
        if (painelSemVidas != null) painelSemVidas.SetActive(false);
    }
}
