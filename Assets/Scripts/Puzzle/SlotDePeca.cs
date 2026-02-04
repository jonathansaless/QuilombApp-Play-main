using UnityEngine;
using UnityEngine.EventSystems;

public class SlotDePeca : MonoBehaviour, IDropHandler
{
    public PuzzleManager manager;
    public int idDoSlot; // O ID nos diz se é um slot do grid (0+) ou do painel de retorno (-1)

    public void OnDrop(PointerEventData eventData)
    {
        GameObject pecaArrastada = PecaArrastavel.itemSendoArrastado;
        if (pecaArrastada == null) return;

        // REGRA PRINCIPAL: Se o slot onde estamos tentando soltar a peça JÁ TEM um filho
        // (ou seja, já tem outra peça), a ação é inválida.
        if (transform.childCount > 0)
        {
            // Não fazemos nada. O OnEndDrag da peça cuidará de mandá-la de volta.
            return;
        }

        // Se o slot está vazio, o drop é válido.
        // A peça se torna filha deste slot.
        pecaArrastada.transform.SetParent(transform);
        // Reseta a posição local para garantir que a peça fique centralizada no slot.
        pecaArrastada.transform.localPosition = Vector3.zero;

        // Notifica o PuzzleManager que a peça foi movida para um novo slot.
        manager.PecaMovida(pecaArrastada, this);
    }
}