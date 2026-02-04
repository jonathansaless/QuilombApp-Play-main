using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
public class LetraArrastavel : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public static GameObject itemSendoArrastado;
    public char letra; // A letra que este objeto representa

    public Transform paiOriginal { get; private set; }    
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Canvas canvasPrincipal;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvasPrincipal = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        itemSendoArrastado = gameObject;
        paiOriginal = transform.parent;
        transform.SetParent(canvasPrincipal.transform); 
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvasPrincipal.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        itemSendoArrastado = null;
        
        if (transform.parent == canvasPrincipal.transform)
        {
            transform.SetParent(paiOriginal);
            transform.localPosition = Vector3.zero;
        }
        
        canvasGroup.blocksRaycasts = true;
    }
}