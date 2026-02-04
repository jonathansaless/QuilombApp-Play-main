using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
public class PecaArrastavel : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public static GameObject itemSendoArrastado;
    public int idDaPeca;

    public Transform paiOriginal { get; private set; }    
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Canvas canvasPrincipal; // Referência para o Canvas principal

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        // Encontra o Canvas principal na cena
        canvasPrincipal = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        itemSendoArrastado = gameObject;
        paiOriginal = transform.parent;
        transform.SetParent(canvasPrincipal.transform); // Define o Canvas principal como pai
        
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Converte a posição do rato para a posição local do Canvas
        rectTransform.anchoredPosition += eventData.delta / canvasPrincipal.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        itemSendoArrastado = null;
        
        // Se a peça não foi solta num novo slot válido, ela volta para casa
        if (transform.parent == canvasPrincipal.transform)
        {
            transform.SetParent(paiOriginal);
            transform.localPosition = Vector3.zero;
        }
        
        canvasGroup.blocksRaycasts = true;
    }
}