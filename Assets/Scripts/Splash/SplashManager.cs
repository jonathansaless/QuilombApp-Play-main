using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using System.Collections;

public class SplashManager : MonoBehaviour
{
    [Header("Referências da UI")]
    public GameObject painelBotoesPrincipais;
    public GameObject painelSobreNos;
    public GameObject painelCarregamento;
    public Slider barraDeProgresso;

    [Header("Configurações")]
    public float tempoEsperaInicial = 2.0f;
    public float tempoAnimacaoBarra = 1.2f;
    public float tempoDeFade = 0.2f;

    private FirebaseAuth auth;
    private CanvasGroup sobreNosCanvasGroup;

    void Start()
    {
        sobreNosCanvasGroup = painelSobreNos.GetComponent<CanvasGroup>();

        painelBotoesPrincipais.SetActive(false);
        painelSobreNos.SetActive(true);
        sobreNosCanvasGroup.alpha = 0;
        sobreNosCanvasGroup.interactable = false;
        sobreNosCanvasGroup.blocksRaycasts = false;
        
        painelCarregamento.SetActive(false);

        Inicializar();
    }

    async void Inicializar()
    {
        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (dependencyStatus == DependencyStatus.Available) 
        { 
            auth = FirebaseAuth.DefaultInstance; 
        }
        else 
        { 
            Debug.LogError($"Erro nas dependências do Firebase: {dependencyStatus}"); 
        }

        await Task.Delay((int)(tempoEsperaInicial * 1000));
        painelBotoesPrincipais.SetActive(true);
    }

    public async void OnJogarButtonClick()
    {
        painelBotoesPrincipais.SetActive(false);
        painelCarregamento.SetActive(true);

        Task animacaoTask = AnimarBarraDeProgresso(tempoAnimacaoBarra);
        Task<bool> carregamentoTask = ChecarLoginECarregarDados();
        await Task.WhenAll(animacaoTask, carregamentoTask);

        bool dadosCarregadosComSucesso = await carregamentoTask;

        // --- ALTERAÇÃO AQUI: Voltamos a usar SceneManager.LoadScene ---
        if (dadosCarregadosComSucesso)
        {
            SceneManager.LoadScene("CenaMenu");
        }
        else
        {
            SceneManager.LoadScene("CenaLogin");
        }
    }

    // A lógica de fade para os painéis continua a mesma
    public void OnSobreNosButtonClick()
    {
        painelBotoesPrincipais.SetActive(false);
        StartCoroutine(Fade(sobreNosCanvasGroup, 1f, tempoDeFade));
    }

    public void VoltarParaSplash()
    {
        StartCoroutine(Fade(sobreNosCanvasGroup, 0f, tempoDeFade, () => {
            painelBotoesPrincipais.SetActive(true);
        }));
    }

    private IEnumerator Fade(CanvasGroup canvasGroup, float targetAlpha, float duration, System.Action onComplete = null)
    {
        float startAlpha = canvasGroup.alpha;
        float time = 0;

        if (targetAlpha > 0)
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        while (time < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;

        if (targetAlpha == 0)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        
        onComplete?.Invoke();
    }

    private async Task<bool> ChecarLoginECarregarDados()
    {
        if (auth != null && auth.CurrentUser != null)
        {
            // 1) Carrega todos os dados do jogador (moedas, vidas, etc.)
            bool carregou = await PlayerDataManager.Instance.CarregarDadosDoFirebase();
            if (!carregou)
                return false;

            // 2) Já faz aqui a verificação / reset diário das vidas
            await PlayerDataManager.Instance.CheckAndResetDailyLives();

            // 3) Se chegou aqui, está tudo pronto para o Menu
            return true;
        }

        return false;
    }


    private async Task AnimarBarraDeProgresso(float duracao)
    {
        float tempo = 0;
        barraDeProgresso.value = 0;
        while (tempo < duracao)
        {
            barraDeProgresso.value = Mathf.Lerp(0, 1, tempo / duracao);
            tempo += Time.deltaTime;
            await Task.Yield();
        }
        barraDeProgresso.value = 1;
    }
}