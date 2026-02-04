using UnityEngine;
using UnityEngine.EventSystems;

public class SlotDeLetra : MonoBehaviour, IDropHandler
{
    // Este slot pode ser um slot de resposta (no topo) ou um slot de origem (embaixo)
    public WordGameManager manager;

    public void OnDrop(PointerEventData eventData)
    {
        GameObject letraArrastada = LetraArrastavel.itemSendoArrastado;
        if (letraArrastada == null) return;

        // Se o slot já tiver uma letra, a ação é inválida.
        if (transform.childCount > 0)
        {
            return; // OnEndDrag da letra vai mandá-la de volta.
        }

        // Se o slot está vazio, o drop é válido.
        letraArrastada.transform.SetParent(transform);
        letraArrastada.transform.localPosition = Vector3.zero;

        // Notifica o manager que um movimento ocorreu
        manager.LetraMovida();
    }
}