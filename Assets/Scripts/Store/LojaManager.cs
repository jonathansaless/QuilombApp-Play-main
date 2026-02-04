using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;

// Uma classe auxiliar para organizar os dados dos poderes no Inspector da Unity.
// [System.Serializable] permite que ela apareça no Inspector.
[System.Serializable]
public class PoderLojaData
{
    public string nomeDoPoder; // Deve ser "Dica", "5050", ou "DuasChances"
    public Sprite iconePoder;
    public int preco;
}

public class LojaManager : MonoBehaviour
{
    [Header("Referências da UI - NavBar")]
    public TextMeshProUGUI textoMoedas;
    public Image iconeAvatarJogador;
    public TextMeshProUGUI textoQtdDica;
    public TextMeshProUGUI textoQtd5050;
    public TextMeshProUGUI textoQtdDuasChances;

    [Header("Referências da UI - Loja")]
    public Transform conteudoScroll;
    public GameObject painelFeedbackCompra;
    public GameObject painelMoedasInsuficientes;

    [Header("Prefabs dos Itens")]
    public GameObject itemAvatarPrefab;
    public GameObject itemPoderPrefab;
    public GameObject cabecalhoPrefab;

    [Header("Banco de Dados")]
    public AvatarDatabase avatarDatabase;
    public List<PoderLojaData> poderesParaVenda;

    void Start()
    {
        if (PlayerDataManager.Instance == null || PlayerDataManager.Instance.Dados == null)
        {
            SceneManager.LoadScene("CenaLogin");
            return;
        }

        if (painelFeedbackCompra != null)
            painelFeedbackCompra.SetActive(false);
        
        AtualizarUI();
        PopularLoja();
    }

    void AtualizarUI()
    {
        var dados = PlayerDataManager.Instance.Dados;
        textoMoedas.text = dados.Moedas.ToString();
        
        // Atualiza o avatar na NavBar
        int idEquipado = dados.AvatarEquipadoID;
        Sprite spriteEquipado = avatarDatabase.EncontrarSpriteDoAvatarPeloID(idEquipado);
        if (spriteEquipado != null)
        {
            iconeAvatarJogador.sprite = spriteEquipado;
        }

        // Atualiza a quantidade de poderes na NavBar
        textoQtdDica.text = dados.QuantidadeDica.ToString();
        textoQtdDuasChances.text = dados.QuantidadeDuasChances.ToString();
        textoQtd5050.text = dados.Quantidade5050.ToString();

    }
    
    void PopularLoja()
    {
        // Limpa itens antigos para evitar duplicação ao reentrar na cena
        foreach (Transform child in conteudoScroll)
        {
            Destroy(child.gameObject);
        }

        // --- TÍTULO DOS AVATARES ---
        if (cabecalhoPrefab != null)
        {
            GameObject cabecalhoAvatares = Instantiate(cabecalhoPrefab, conteudoScroll);
            cabecalhoAvatares.transform.Find("Texto").GetComponent<TextMeshProUGUI>().text = "AVATARES";
        }

        // --- ITENS DE AVATARES ---
        foreach (ItemLoja_AvatarData avatarData in avatarDatabase.todosOsAvatares)
        {
            GameObject itemObj = Instantiate(itemAvatarPrefab, conteudoScroll);
            
            int avatarId = avatarData.avatarID;
            int custo = avatarData.preco;

            GameObject mask = itemObj.transform.Find("Mask").gameObject;
            Image icone = mask.transform.Find("IconeAvatar").GetComponent<Image>();
            Button botaoComprar = itemObj.transform.Find("BotaoComprar").GetComponent<Button>();
            TextMeshProUGUI textoComprar = botaoComprar.transform.Find("TextoComprar").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI textoPreco = botaoComprar.transform.Find("TextoPreco").GetComponent<TextMeshProUGUI>();
            GameObject iconeMoeda = botaoComprar.transform.Find("MoedaIcon").gameObject;

            icone.sprite = avatarData.iconeAvatar;
            textoComprar.text = "COMPRAR";
            textoPreco.text = custo.ToString();

            if (PlayerDataManager.Instance.Dados.AvataresPossuidos.Contains(avatarId))
            {
                botaoComprar.interactable = false;
                textoComprar.text = "ADQUIRIDO";
                textoPreco.gameObject.SetActive(false);
                iconeMoeda.SetActive(false);
            }
            else
            {
                botaoComprar.onClick.AddListener(() => ComprarAvatar(avatarData, botaoComprar));
            }
        }

        // --- TÍTULO DOS PODERES ---
        if (cabecalhoPrefab != null)
        {
            GameObject cabecalhoPoderes = Instantiate(cabecalhoPrefab, conteudoScroll);
            cabecalhoPoderes.transform.Find("Texto").GetComponent<TextMeshProUGUI>().text = "PODERES";
        }

        // --- ITENS DE PODERES ---
        foreach (PoderLojaData poderData in poderesParaVenda)
        {
            GameObject itemObj = Instantiate(itemPoderPrefab, conteudoScroll);
            
            Image icone = itemObj.transform.Find("IconePoder").GetComponent<Image>();
            Button botaoComprar = itemObj.transform.Find("BotaoComprar").GetComponent<Button>();
            TextMeshProUGUI textoComprar = botaoComprar.transform.Find("TextoComprar").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI textoPreco = botaoComprar.transform.Find("TextoPreco").GetComponent<TextMeshProUGUI>();

            icone.sprite = poderData.iconePoder;
            textoComprar.text = "COMPRAR";
            textoPreco.text = poderData.preco.ToString();
            
            botaoComprar.onClick.AddListener(() => ComprarPoder(poderData));
        }
    }

    public void ComprarAvatar(ItemLoja_AvatarData avatarData, Button botao)
    {
        int custo = avatarData.preco;
        if (PlayerDataManager.Instance.Dados.Moedas >= custo)
        {
            // Altera os dados apenas no cache local
            PlayerDataManager.Instance.Dados.Moedas -= custo;
            PlayerDataManager.Instance.Dados.AvataresPossuidos.Add(avatarData.avatarID);
            
            Debug.Log($"Avatar {avatarData.avatarID} comprado localmente!");

            // Atualiza a UI para refletir a compra
            AtualizarUI();
            botao.interactable = false;
            botao.transform.Find("TextoComprar").GetComponent<TextMeshProUGUI>().text = "ADQUIRIDO";
            botao.transform.Find("TextoPreco").gameObject.SetActive(false);
            botao.transform.Find("MoedaIcon").gameObject.SetActive(false);

            StartCoroutine(MostrarFeedbackCompra());
        }
        else
        {
            Debug.Log("Moedas insuficientes!");
            StartCoroutine(MostrarFeedbackErro());
        }
    }

    public void ComprarPoder(PoderLojaData poderData)
    {
        if (PlayerDataManager.Instance.Dados.Moedas >= poderData.preco)
        {
            // Altera os dados apenas no cache local
            PlayerDataManager.Instance.Dados.Moedas -= poderData.preco;
            
            switch (poderData.nomeDoPoder)
            {
                case "Dica":
                    PlayerDataManager.Instance.Dados.QuantidadeDica++;
                    break;
                case "5050":
                    PlayerDataManager.Instance.Dados.Quantidade5050++;
                    break;
                case "DuasChances":
                    PlayerDataManager.Instance.Dados.QuantidadeDuasChances++;
                    break;
            }

            Debug.Log($"Poder '{poderData.nomeDoPoder}' comprado localmente!");
            
            // Atualiza a UI para mostrar a nova quantidade de moedas e poderes
            AtualizarUI(); 
            StartCoroutine(MostrarFeedbackCompra());
        }
        else
        {
            Debug.Log("Moedas insuficientes!");
            StartCoroutine(MostrarFeedbackErro());
        }
    }
    
    private IEnumerator MostrarFeedbackCompra()
    {
        if (painelFeedbackCompra != null)
            painelFeedbackCompra.SetActive(true);

        // Espera por 1 segundo
        yield return new WaitForSeconds(1f);

        if (painelFeedbackCompra != null)
            painelFeedbackCompra.SetActive(false);
    }

    private IEnumerator MostrarFeedbackErro()
    {
        if (painelMoedasInsuficientes != null)
            painelMoedasInsuficientes.SetActive(true);

        // Espera por 1.5 segundos (um pouco mais para o jogador ler)
        yield return new WaitForSeconds(1.5f);

        if (painelMoedasInsuficientes != null)
            painelMoedasInsuficientes.SetActive(false);
    }

    public void AbrirMenu()
    {
        // Salva todos os dados na nuvem ANTES de sair da loja
        Debug.Log("Salvando dados na nuvem...");
        PlayerDataManager.Instance.SalvarDadosNoFirebase();
        
        SceneManager.LoadScene("CenaMenu");
    }

    public void AbrirPerfil()
    {
        PlayerDataManager.Instance.SalvarDadosNoFirebase();
        SceneManager.LoadScene("CenaPerfil");
    }

    public void AbrirRanking()
    {
        PlayerDataManager.Instance.SalvarDadosNoFirebase();
        RankingState.TabParaAbrir = RankingTab.Quiz;
        SceneManager.LoadScene("CenaRanking");
    }

    public void AbrirConfiguracoes()
    {
        PlayerDataManager.Instance.SalvarDadosNoFirebase();
        SceneManager.LoadScene("CenaConfiguracoes");
    }
}