using UnityEngine;
using UnityEngine.UI;

// Garante que este script só possa ser adicionado a objetos que tenham um componente Button
[RequireComponent(typeof(Button))]
public class ButtonSound : MonoBehaviour
{
    [Tooltip("O som de clique específico para este botão. Se deixar vazio, usará o som padrão.")]
    public AudioClip somPersonalizado; // Campo para o seu som customizado

    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
    }

    void OnEnable()
    {
        button.onClick.AddListener(PlaySound);
    }

    void OnDisable()
    {
        button.onClick.RemoveListener(PlaySound);
    }

    private void PlaySound()
    {
        if (AudioManager.Instance != null)
        {
            // Se um som personalizado foi arrastado no Inspector, toca ele.
            if (somPersonalizado != null)
            {
                AudioManager.Instance.PlaySFX(somPersonalizado);
            }
            // Senão, toca o som de clique padrão que está no AudioManager.
            else
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.defaultClickSound);
            }
        }
    }
}