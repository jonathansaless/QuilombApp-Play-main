using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class ExplanationManager : MonoBehaviour
{
    [Header("Configurações de Paginação")]
    [Tooltip("Número máximo de caracteres por página, incluindo espaços.")]
    private int maxCaracteresPorPagina = 350;

    [Header("Referências da UI")]
    public Image imagemPrincipal;
    public TextMeshProUGUI textoExplicacao;
    public TextMeshProUGUI textoContadorPaginas;
    public TextMeshProUGUI textoTitulo;
    public TextMeshProUGUI textoFonte;

    public Button botaoProximaPagina;

    // ✅ NOVO: botão de voltar página
    public Button botaoPaginaAnterior;

    // Esse é o botão que você já tinha (na última página vai para o feedback)
    public Button botaoRetornar;

    private List<string> paginas;
    private int paginaAtual = 0;
    private string cenaFeedbackDestino;

    void Start()
    {
        NivelDataBase nivelData = GameDataHolder.NivelParaCarregar;

        if (nivelData == null)
        {
            Debug.LogError("Nenhum dado de nível encontrado! Voltando ao menu.");
            SceneManager.LoadScene("CenaMenu");
            return;
        }

        cenaFeedbackDestino = PlayerPrefs.GetString("CenaFeedbackDestino");

        Sprite spritePrincipal = null;
        string textoCompleto = "";
        string titulo = "";
        string fonte = "";

        if (nivelData is PuzzleData puzzleData)
        {
            spritePrincipal = puzzleData.imagemPrincipal;
            textoCompleto = puzzleData.textoDaExplicacao;
            titulo = puzzleData.titulo;
            fonte = puzzleData.fonteExplicacao;
        }
        else if (nivelData is WordGameData wordGameData)
        {
            spritePrincipal = wordGameData.imagemPrincipal;
            textoCompleto = wordGameData.textoDaExplicacao;
            titulo = wordGameData.titulo;
            fonte = wordGameData.fonteExplicacao;
        }
        else
        {
            Debug.LogWarning($"O nível '{nivelData.idDoNivel}' não tem conteúdo de explicação. Pulando para o feedback.");
            IrParaFeedback();
            return;
        }

        paginas = PaginarTexto(textoCompleto, maxCaracteresPorPagina);

        imagemPrincipal.sprite = spritePrincipal;
        textoTitulo.text = string.IsNullOrEmpty(titulo) ? "" : titulo.ToUpper();

        if (textoFonte != null)
        {
            if (!string.IsNullOrEmpty(fonte))
            {
                textoFonte.gameObject.SetActive(true);
                textoFonte.text = $"Fonte: {fonte}";
            }
            else
            {
                textoFonte.gameObject.SetActive(false);
            }
        }

        MostrarPagina(0);
    }

    void MostrarPagina(int index)
    {
        paginaAtual = Mathf.Clamp(index, 0, paginas.Count - 1);

        textoExplicacao.text = paginas[paginaAtual];
        textoContadorPaginas.text = $"{paginaAtual + 1}/{paginas.Count}";

        bool ultimaPagina = (paginaAtual >= paginas.Count - 1);
        bool temAnterior = (paginaAtual > 0);

        // Próxima só aparece se NÃO for a última
        if (botaoProximaPagina != null)
            botaoProximaPagina.gameObject.SetActive(!ultimaPagina);

        // ✅ Voltar só aparece se houver página anterior
        if (botaoPaginaAnterior != null)
            botaoPaginaAnterior.gameObject.SetActive(temAnterior);

        // Retornar (ir para o feedback) só aparece na última página (mantém seu comportamento atual)
        if (botaoRetornar != null)
            botaoRetornar.gameObject.SetActive(ultimaPagina);
    }

    private List<string> PaginarTexto(string textoCompleto, int maxCaracteres)
    {
        var paginasResultantes = new List<string>();

        if (string.IsNullOrEmpty(textoCompleto))
        {
            paginasResultantes.Add("Nenhuma explicação encontrada para este nível.");
            return paginasResultantes;
        }

        string textoRestante = textoCompleto;

        while (textoRestante.Length > 0)
        {
            if (textoRestante.Length <= maxCaracteres)
            {
                paginasResultantes.Add(textoRestante);
                break;
            }

            string pedaco = textoRestante.Substring(0, maxCaracteres);
            int ultimoEspaco = pedaco.LastIndexOf(' ');
            int pontoDeCorte = (ultimoEspaco > 0) ? ultimoEspaco : maxCaracteres;

            paginasResultantes.Add(textoRestante.Substring(0, pontoDeCorte));
            textoRestante = textoRestante.Substring(pontoDeCorte).TrimStart();
        }

        return paginasResultantes;
    }

    public void ProximaPagina()
    {
        if (paginaAtual < paginas.Count - 1)
        {
            MostrarPagina(paginaAtual + 1);
        }
    }

    // página anterior
    public void PaginaAnterior()
    {
        if (paginaAtual > 0)
        {
            MostrarPagina(paginaAtual - 1);
        }
    }

    public void IrParaFeedback()
    {
        GameDataHolder.NivelParaCarregar = null;
        PlayerPrefs.DeleteKey("CenaFeedbackDestino");

        if (!string.IsNullOrEmpty(cenaFeedbackDestino))
        {
            SceneManager.LoadScene(cenaFeedbackDestino);
        }
        else
        {
            Debug.LogError("Nome da cena de feedback não encontrado! Voltando ao menu.");
            SceneManager.LoadScene("CenaMenu");
        }
    }
}
