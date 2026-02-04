using UnityEngine;

// A linha abaixo permite criar estes objetos no menu da Unity
[CreateAssetMenu(fileName = "Novo Avatar", menuName = "QuilombApp/Item Loja Avatar")]
public class ItemLoja_AvatarData : ScriptableObject
{
    public int avatarID;
    public Sprite iconeAvatar;
    public int preco;
    // No futuro, você pode adicionar mais informações aqui, como nome, descrição, etc.
}