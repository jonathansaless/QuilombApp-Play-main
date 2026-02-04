using UnityEngine;
using UnityEngine.UI;

public class BackgroundAnimator : MonoBehaviour
{
    [Tooltip("A imagem de fundo que será animada.")]
    public RawImage backgroundImage;

    [Header("Configurações do Movimento")]

    [Tooltip("A velocidade do balanço. Valores maiores = mais rápido.")]
    public float movementSpeed = 0.5f;

    [Tooltip("A distância que o fundo se move. Valores maiores = movimento mais intenso.")]
    public float movementIntensity = 0.02f;

    private Rect initialUvRect;

    void Start()
    {
        if (backgroundImage == null)
        {
            backgroundImage = GetComponent<RawImage>();
        }

        if (backgroundImage != null)
        {
            // Salva a posição inicial da textura para que o movimento seja sempre em relação a ela.
            initialUvRect = backgroundImage.uvRect;
        }
        else
        {
            Debug.LogError("RawImage não encontrado para animação de background!");
            enabled = false;
        }
    }

    void Update()
    {
        // Mathf.Sin() cria uma onda suave que vai de -1 a 1 ao longo do tempo.
        // Multiplicamos por Time.time * movementSpeed para controlar a velocidade da onda.
        float offsetX = Mathf.Sin(Time.time * movementSpeed) * movementIntensity;
        float offsetY = Mathf.Cos(Time.time * movementSpeed) * movementIntensity; // Usamos Cos para um movimento circular/diagonal suave

        // Aplica o deslocamento calculado à posição inicial da textura.
        backgroundImage.uvRect = new Rect(
            initialUvRect.x + offsetX, 
            initialUvRect.y + offsetY, 
            initialUvRect.width, 
            initialUvRect.height
        );
    }
}