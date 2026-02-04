using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Firebase.Auth;
using Firebase.Firestore;
using System.Linq;

public class RankingManager : MonoBehaviour
{
    [Header("Referências da UI (Geral)")]
    public Image iconeAvatarJogador; // Avatar do jogador logado na NavBar
    public GameObject painelCarregando; // Um painel para mostrar enquanto carrega
    public GameObject itemRankingPrefab; // Prefab para um item da lista

    [Header("Banco de Dados")]
    public AvatarDatabase avatarDatabase;

    [Header("Abas e Botões")]
    public Button botaoQuiz;
    public Button botaoPuzzle;
    public Button botaoWordGame;
    // Removemos as referências aos sprites de botão ativo/inativo
    // public Sprite spriteBotaoAtivo;   // Sprite para o botão selecionado
    // public Sprite spriteBotaoInativo; // Sprite para os botões não selecionados

    [Header("Painéis de Ranking")]
    public GameObject painelRankingQuiz;
    public GameObject painelRankingPuzzle;
    public GameObject painelRankingWordGame;

    [Header("Conteúdo dos ScrollViews")]
    public Transform conteudoScrollQuiz;    // Content do ScrollView do Quiz
    public Transform conteudoScrollPuzzle;  // Content do ScrollView do Puzzle
    public Transform conteudoScrollWordGame; // Content do ScrollView do Jogo de Palavras

    private int rankingsCarregados = 0; // Contador para saber quando todos os rankings foram carregados
    private FirebaseFirestore db;

    void Start()
    {
        if (PlayerDataManager.Instance == null || PlayerDataManager.Instance.Dados == null)
        {
            SceneManager.LoadScene("CenaLogin");
            return;
        }

        db = FirebaseFirestore.DefaultInstance;
        rankingsCarregados = 0;
        if (painelCarregando != null) painelCarregando.SetActive(true);

        AtualizarNavBar();

        // Adiciona os listeners (funções) para cada botão
        botaoQuiz.onClick.AddListener(() => AbrirTab(RankingTab.Quiz));
        botaoPuzzle.onClick.AddListener(() => AbrirTab(RankingTab.Puzzle));
        botaoWordGame.onClick.AddListener(() => AbrirTab(RankingTab.WordGame));

        // Começa a carregar todos os rankings em paralelo
        CarregarRanking(RankingTab.Quiz);
        CarregarRanking(RankingTab.Puzzle);
        CarregarRanking(RankingTab.WordGame);

        // Abre a aba correta com base no que foi definido na cena anterior
        AbrirTab(RankingState.TabParaAbrir);
        
        // Reseta o estado para o padrão (Quiz) da próxima vez que vier do menu
        RankingState.TabParaAbrir = RankingTab.Quiz;
    }

    void AtualizarNavBar()
    {
        int idEquipado = PlayerDataManager.Instance.Dados.AvatarEquipadoID;
        Sprite spriteEquipado = avatarDatabase.EncontrarSpriteDoAvatarPeloID(idEquipado);
        if (spriteEquipado != null)
        {
            iconeAvatarJogador.sprite = spriteEquipado;
        }
    }

    /// <summary>
    /// Controla qual painel de ranking é exibido e atualiza a aparência dos botões.
    /// </summary>
    public void AbrirTab(RankingTab tab)
    {
        // 1. Esconde todos os painéis
        painelRankingQuiz.SetActive(false);
        painelRankingPuzzle.SetActive(false);
        painelRankingWordGame.SetActive(false);

        // 2. Define todos os botões como "inativos" (Alpha 100)
        // O valor 100 em 0-255 é (100f / 255f) em 0-1f
        float inactiveAlpha = 100f / 255f;
        SetButtonAlpha(botaoQuiz, inactiveAlpha);
        SetButtonAlpha(botaoPuzzle, inactiveAlpha);
        SetButtonAlpha(botaoWordGame, inactiveAlpha);

        // 3. Ativa o painel e o botão corretos com base no parâmetro 'tab' (Alpha 255)
        float activeAlpha = 1.0f; // 255f / 255f
        switch (tab)
        {
            case RankingTab.Quiz:
                painelRankingQuiz.SetActive(true);
                SetButtonAlpha(botaoQuiz, activeAlpha);
                break;
            case RankingTab.Puzzle:
                painelRankingPuzzle.SetActive(true);
                SetButtonAlpha(botaoPuzzle, activeAlpha);
                break;
            case RankingTab.WordGame:
                painelRankingWordGame.SetActive(true);
                SetButtonAlpha(botaoWordGame, activeAlpha);
                break;
        }
    }

    /// <summary>
    /// Carrega o ranking específico para um modo de jogo.
    /// </summary>
    async void CarregarRanking(RankingTab tab)
    {
        string campoOrderBy = "";
        Transform conteudoScroll = null;

        // Define as variáveis corretas com base na aba
        switch (tab)
        {
            case RankingTab.Quiz:
                campoOrderBy = "PontuacaoTotalQuiz";
                conteudoScroll = conteudoScrollQuiz;
                break;
            case RankingTab.Puzzle:
                campoOrderBy = "PontuacaoTotalPuzzle";
                conteudoScroll = conteudoScrollPuzzle;
                break;
            case RankingTab.WordGame:
                campoOrderBy = "PontuacaoTotalWordGame";
                conteudoScroll = conteudoScrollWordGame;
                break;
        }

        // Limpa os itens antigos do ranking
        foreach (Transform child in conteudoScroll)
        {
            Destroy(child.gameObject);
        }

        // Cria a query no Firebase ordenando pelo campo correto
        Query query = db.Collection("jogadores").OrderByDescending(campoOrderBy).Limit(50);
        var querySnapshot = await query.GetSnapshotAsync();

        int posicao = 1;
        foreach (DocumentSnapshot documentSnapshot in querySnapshot.Documents)
        {
            PlayerData jogador = documentSnapshot.ConvertTo<PlayerData>();

            // Ignora jogadores com pontuação 0 neste modo
            long pontuacaoDoModo = 0;
            switch (tab)
            {
                case RankingTab.Quiz: pontuacaoDoModo = jogador.PontuacaoTotalQuiz; break;
                case RankingTab.Puzzle: pontuacaoDoModo = jogador.PontuacaoTotalPuzzle; break;
                case RankingTab.WordGame: pontuacaoDoModo = jogador.PontuacaoTotalWordGame; break;
            }

            if (pontuacaoDoModo <= 0) continue; // Pula para o próximo jogador

            GameObject itemObj = Instantiate(itemRankingPrefab, conteudoScroll);

            // Encontra os componentes no prefab
            TextMeshProUGUI textoPosicao = itemObj.transform.Find("TextoPosicao").GetComponent<TextMeshProUGUI>();
            GameObject mask = itemObj.transform.Find("Mask").gameObject;
            Image iconeAvatar = mask.transform.Find("IconeAvatar").GetComponent<Image>();
            TextMeshProUGUI textoNome = itemObj.transform.Find("TextoNome").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI textoPontos = itemObj.transform.Find("TextoPontos").GetComponent<TextMeshProUGUI>();

            // Preenche os dados
            textoPosicao.text = posicao.ToString();
            textoNome.text = jogador.Apelido;
            textoPontos.text = pontuacaoDoModo.ToString(); // Exibe a pontuação correta do modo

            Sprite spriteAvatar = avatarDatabase.EncontrarSpriteDoAvatarPeloID(jogador.AvatarEquipadoID);
            if (spriteAvatar != null)
            {
                iconeAvatar.sprite = spriteAvatar;
            }

            posicao++;
        }

        // Incrementa o contador de rankings carregados
        rankingsCarregados++;

        // Se todos os 3 rankings (Quiz, Puzzle, WordGame) terminaram de carregar,
        // esconde o painel de carregamento.
        if (rankingsCarregados == 3 && painelCarregando != null)
        {
            painelCarregando.SetActive(false);
        }
    }

    // --- NOVA FUNÇÃO HELPER ---
    /// <summary>
    /// Define o canal Alfa (transparência) do componente Image de um Botão.
    /// </summary>
    /// <param name="button">O botão-alvo.</param>
    /// <param name="alpha">O valor do alfa (0.0f a 1.0f).</param>
    void SetButtonAlpha(Button button, float alpha)
    {
        // Pega o componente Image do botão
        Image image = button.GetComponent<Image>();
        if (image != null)
        {
            // Pega a cor atual, altera o alfa, e aplica de volta
            Color tempColor = image.color;
            tempColor.a = alpha;
            image.color = tempColor;
        }
    }

    // Funções da NavBar (sem alterações)
    public void AbrirMenu()
    {
        SceneManager.LoadScene("CenaMenu");
    }

    public void AbrirPerfil()
    {
        SceneManager.LoadScene("CenaPerfil");
    }

    public void AbrirLoja()
    {
        SceneManager.LoadScene("CenaLoja");
    }

    public void AbrirConfiguracoes()
    {
        SceneManager.LoadScene("CenaConfiguracoes");
    }
}