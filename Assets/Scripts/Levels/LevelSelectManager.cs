using TMPro;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using Firebase.Auth; // Adicionado para o atalho de depuração
using System.Threading.Tasks; // Adicionado para o atalho de depuração

public class LevelSelectManager : MonoBehaviour
{
    [Header("Configuração")]
    public ModoDeJogoData modoDeJogo;
    
    [Header("Referências da UI")]
    public GameObject levelNodePrefab;
    public Transform contentPanel;
    public ScrollRect scrollView;
    public CanvasGroup scrollViewCanvasGroup; 
    public GameObject painelSemVidas;

    [Header("Configurações de Layout")]
    public float posicaoX = 250f;

    [Header("Referências da NavBar")]
    public TextMeshProUGUI textoMoedas;
    public TextMeshProUGUI textoVidas;
    public TextMeshProUGUI textoApelido;
    public Image iconeAvatarJogador;
    public AvatarDatabase avatarDatabase;

    void Awake() 
    {
        // Atualiza a NavBar com os dados do jogador
        AtualizarNavBar();

        // Lógica de carregamento dinâmico
        if (GameDataHolder.ModoDeJogoParaCarregar != null)
        {
            modoDeJogo = GameDataHolder.ModoDeJogoParaCarregar;
            Debug.Log($"<color=cyan>Modo de Jogo '{modoDeJogo.name}' carregado dinamicamente do Menu.</color>");
            GameDataHolder.ModoDeJogoParaCarregar = null;
        }
        else if (modoDeJogo != null)
        {
            Debug.LogWarning($"AVISO: GameDataHolder estava vazio. Usando o Modo de Jogo '{modoDeJogo.name}' que foi definido no Inspector (Modo de Teste).");
        }
        else
        {
            Debug.LogError("ERRO FATAL: Nenhum Modo de Jogo foi carregado. Voltando ao Menu Principal.");
            SceneManager.LoadScene("CenaMenu");
            return;
        }
        
        GerarNodesDosNiveis();
        StartCoroutine(ForceScrollToBottom());
    }

    public void AtualizarNavBar()
    {
        if (PlayerDataManager.Instance == null || PlayerDataManager.Instance.Dados == null)
        {
            Debug.LogWarning("Dados do jogador ainda não disponíveis para atualizar a NavBar.");
            return;
        }

        var dados = PlayerDataManager.Instance.Dados;

        if (textoMoedas != null) textoMoedas.text = dados.Moedas.ToString();
        if (textoVidas != null) textoVidas.text = dados.Vidas.ToString();
        if (textoApelido != null) textoApelido.text = dados.Apelido;

        if (iconeAvatarJogador != null && avatarDatabase != null)
        {
            int idEquipado = dados.AvatarEquipadoID;
            Sprite spriteEquipado = avatarDatabase.EncontrarSpriteDoAvatarPeloID(idEquipado);
            if (spriteEquipado != null)
            {
                iconeAvatarJogador.sprite = spriteEquipado;
            }
        }
    }
    
    void GerarNodesDosNiveis()
    {
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < modoDeJogo.niveis.Count; i++)
        {
            NivelDataBase nivelAtual = modoDeJogo.niveis[i];

            GameObject containerObj = new GameObject($"Container Nível {i + 1}", typeof(RectTransform));
            containerObj.transform.SetParent(contentPanel, false);

            LayoutElement containerLayout = containerObj.AddComponent<LayoutElement>();
            containerLayout.preferredHeight = 200;

            GameObject nodeObj = Instantiate(levelNodePrefab, containerObj.transform);
            LevelNode levelNode = nodeObj.GetComponent<LevelNode>();
            RectTransform nodeRect = nodeObj.GetComponent<RectTransform>();

            if (i % 2 == 0)
            {
                nodeRect.anchoredPosition = new Vector2(-posicaoX, 0);
            }
            else
            {
                nodeRect.anchoredPosition = new Vector2(posicaoX, 0);
            }
            
            NivelStatus status = NivelStatus.Bloqueado;
            bool concluido = PlayerDataManager.Instance.Dados.PontuacoesPorNivel.ContainsKey(nivelAtual.idDoNivel);
            
            if (concluido)
            {
                status = NivelStatus.Concluido;
            }
            else
            {
                if (i == 0 || PlayerDataManager.Instance.Dados.PontuacoesPorNivel.ContainsKey(modoDeJogo.niveis[i-1].idDoNivel))
                {
                    status = NivelStatus.Disponivel;
                }
            }

            levelNode.Setup(i, status, () => SelecionarNivel(nivelAtual));
        }
    } 

    public void SelecionarNivel(NivelDataBase nivelData)
    {
        // --- NOVA VERIFICAÇÃO DE VIDAS ---
        // Primeiro, verifica se o jogador tem vidas suficientes para jogar.
        if (PlayerDataManager.Instance.Dados.Vidas < 1)
        {
            // Se não tiver vidas, mostra o painel e impede a transição de cena.
            Debug.Log("Jogador sem vidas. Exibindo painel de aviso.");
            if (painelSemVidas != null) painelSemVidas.SetActive(true);
            return; // Encerra a função aqui.
        }
        // --- FIM DA VERIFICAÇÃO ---

        // Se o jogador tiver vidas, o código continua normalmente.
        Debug.Log("Carregando nível: " + nivelData.idDoNivel);
        GameDataHolder.NivelParaCarregar = nivelData;
        SceneManager.LoadScene(modoDeJogo.nomeDaCena);
    }
    
    private IEnumerator ForceScrollToBottom()
    {
        // Garante que o Canvas Group esteja invisível no início
        if (scrollViewCanvasGroup != null)
        {
            scrollViewCanvasGroup.alpha = 0;
        }

        // Espera um frame para o Vertical Layout Group calcular o tamanho do Content.
        yield return new WaitForEndOfFrame();

        // Força a posição da barra de rolagem vertical para o fundo (valor 0).
        if (scrollView != null)
        {
            scrollView.verticalNormalizedPosition = 0f;
        }

        // Agora que está na posição certa, torna o ScrollView visível instantaneamente.
        if (scrollViewCanvasGroup != null)
        {
            scrollViewCanvasGroup.alpha = 1;
            // Reativa a interatividade
            scrollViewCanvasGroup.interactable = true;
            scrollViewCanvasGroup.blocksRaycasts = true;
        }
    }

    #region Funções de Navegação da NavBar Inferior

    public void AbrirMenu()
    {
        SceneManager.LoadScene("CenaMenu");
    }

    public void AbrirRanking()
    {
        SceneManager.LoadScene("CenaRanking");
    }

    public void AbrirLoja()
    {
        SceneManager.LoadScene("CenaLoja");
    }

    public void AbrirConfiguracoes()
    {
        SceneManager.LoadScene("CenaConfiguracoes");
    }

    #endregion
    public void FecharPainelSemVidas()
    {
        if (painelSemVidas != null)
        {
            painelSemVidas.SetActive(false);
        }
    }
}