using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

// Este script é um Singleton, o que significa que só existirá uma instância dele
// e ela persistirá entre as cenas, controlando todas as transições.
public class SceneFader : MonoBehaviour
{
    public static SceneFader Instance { get; private set; }
    
    [Tooltip("O Canvas Group do painel preto que cobre a tela.")]
    public CanvasGroup faderCanvasGroup;
    
    [Tooltip("A duração do fade em segundos.")]
    public float fadeDuration = 0.5f;

    void Awake()
    {
        // Lógica do Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Função pública principal. Chame esta função de qualquer script para mudar de cena com fade.
    /// </summary>
    /// <param name="sceneName">O nome da cena para carregar.</param>
    public void FadeAndLoadScene(string sceneName)
    {
        StartCoroutine(FadeOutAndLoad(sceneName));
    }

    // Rotina que escurece a tela (Fade Out) e depois carrega a cena
    private IEnumerator FadeOutAndLoad(string sceneName)
    {
        yield return StartCoroutine(Fade(1f)); // Espera o fade para preto terminar
        SceneManager.LoadScene(sceneName);
    }

    // Rotina que clareia a tela (Fade In)
    private IEnumerator FadeIn()
    {
        yield return StartCoroutine(Fade(0f));
    }

    // Coroutine genérica que controla a animação do alpha
    private IEnumerator Fade(float targetAlpha)
    {
        faderCanvasGroup.blocksRaycasts = true; // Bloqueia cliques durante a transição
        float startAlpha = faderCanvasGroup.alpha;
        float time = 0;

        while (time < fadeDuration)
        {
            faderCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            time += Time.deltaTime;
            yield return null;
        }

        faderCanvasGroup.alpha = targetAlpha;
        faderCanvasGroup.blocksRaycasts = (targetAlpha == 1f); // Só bloqueia cliques se a tela estiver preta
    }
    
    // --- BÔNUS: Lógica para fazer o Fade In automaticamente em toda nova cena ---
    void OnEnable()
    {
        // Se inscreve no evento 'sceneLoaded'
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        // Cancela a inscrição no evento para evitar erros
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Este método é chamado automaticamente pela Unity sempre que uma nova cena é carregada.
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Inicia o fade in para revelar a nova cena.
        StartCoroutine(FadeIn());
    }
}