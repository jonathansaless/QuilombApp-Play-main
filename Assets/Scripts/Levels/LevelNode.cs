using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class LevelNode : MonoBehaviour
{
    public Button button;
    public Image backgroundImage;
    public TextMeshProUGUI levelNumberText;
    public GameObject starIcon; // A estrela que aparece no nível concluído
    public GameObject dottedLineConnector; // A linha pontilhada que liga ao próximo

    public void Setup(int levelIndex, NivelStatus status, UnityAction onClickAction)
    {
        levelNumberText.text = (levelIndex + 1).ToString();
        button.onClick.AddListener(onClickAction);

        // Aqui você irá definir as cores/sprites com base no status
        // Este é um exemplo, você pode criar variáveis públicas de cor para configurar no Inspector
        switch (status)
        {
            case NivelStatus.Concluido:
                backgroundImage.color = new Color(0.5f, 0.7f, 1f); // Azul
                button.interactable = true;
                if(starIcon != null) starIcon.SetActive(true);
                if(dottedLineConnector != null) dottedLineConnector.SetActive(true);
                break;
            case NivelStatus.Disponivel:
                backgroundImage.color = Color.yellow; // Amarelo
                button.interactable = true;
                if(starIcon != null) starIcon.SetActive(false);
                if(dottedLineConnector != null) dottedLineConnector.SetActive(false);
                break;
            case NivelStatus.Bloqueado:
                backgroundImage.color = new Color(0.8f, 0.8f, 0.8f, 0.5f); // Branco transparente
                button.interactable = false;
                if(starIcon != null) starIcon.SetActive(false);
                if(dottedLineConnector != null) dottedLineConnector.SetActive(false);
                break;
        }
    }
}

public enum NivelStatus
{
    Bloqueado,
    Disponivel,
    Concluido
}