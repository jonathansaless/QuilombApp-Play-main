using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Firebase.Auth;

public class PerfilManager : MonoBehaviour
{
    [Header("Referências da UI Principal")]
    public Image avatarPrincipal;
    public TextMeshProUGUI textoApelido;
    public TextMeshProUGUI textoIdade;
    public TextMeshProUGUI textoMoedas;
    public TextMeshProUGUI textoPontos;

    [Header("Referências da UI - Alterar Avatar")]
    public GameObject painelAlterarAvatar;
    public Transform gridContainer;
    public GameObject itemSeletorAvatarPrefab;

    [Header("Banco de Dados de Avatares")]
    public AvatarDatabase avatarDatabase;

    void Start()
    {
        if (PlayerDataManager.Instance == null || PlayerDataManager.Instance.Dados == null)
        {
            SceneManager.LoadScene("CenaLogin");
            return;
        }
        painelAlterarAvatar.SetActive(false);
        CarregarInformacoesDoPerfil();
    }

    void CarregarInformacoesDoPerfil()
    {
        var dados = PlayerDataManager.Instance.Dados;
        textoApelido.text = dados.Apelido;
        textoIdade.text = dados.Idade.ToString() + " anos";
        textoMoedas.text = dados.Moedas.ToString();
        textoPontos.text = dados.PontuacaoMaximaTotal.ToString();
        AtualizarAvatarPrincipal();
    }

    void AtualizarAvatarPrincipal()
    {
        int idEquipado = PlayerDataManager.Instance.Dados.AvatarEquipadoID;
        Sprite spriteEquipado = avatarDatabase.EncontrarSpriteDoAvatarPeloID(idEquipado);
        if (spriteEquipado != null)
        {
            avatarPrincipal.sprite = spriteEquipado;
        }
    }

    public void AbrirPainelDeSelecao()
    {
        foreach (Transform child in gridContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (int avatarId in PlayerDataManager.Instance.Dados.AvataresPossuidos)
        {
            GameObject itemObj = Instantiate(itemSeletorAvatarPrefab, gridContainer);
            

            // Procura a imagem no filho "IconeAvatar"
            GameObject mask = itemObj.transform.Find("Mask").gameObject;
            Image icone = mask.transform.Find("IconeAvatar").GetComponent<Image>();
            // Procura o botão no objeto principal
            Button botao = itemObj.GetComponent<Button>();
            
            Sprite spriteDoAvatar = avatarDatabase.EncontrarSpriteDoAvatarPeloID(avatarId);
            if (spriteDoAvatar != null)
            {
                icone.sprite = spriteDoAvatar;
            }
            
            // Adiciona a função de equipar diretamente ao clique do botão
            int idDoAvatarAtual = avatarId;
            botao.onClick.AddListener(() => EquiparAvatar(idDoAvatarAtual));
        }
        
        painelAlterarAvatar.SetActive(true);
    }

    // Chamado diretamente pelo clique em um ícone de avatar na grade
    public void EquiparAvatar(int avatarId)
    {
        Debug.Log($"Equipando o avatar {avatarId}...");

        // 1. Atualiza o ID do avatar equipado no cache local
        PlayerDataManager.Instance.Dados.AvatarEquipadoID = avatarId;

        // 2. Salva a alteração na nuvem
        PlayerDataManager.Instance.SalvarDadosNoFirebase();

        // 3. Atualiza a imagem principal na tela de perfil
        AtualizarAvatarPrincipal();

        // 4. Fecha o painel de seleção
        painelAlterarAvatar.SetActive(false);
    }

    public void VoltarParaMenu()
    {
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
}