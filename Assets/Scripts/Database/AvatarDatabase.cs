using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "AvatarDatabase", menuName = "Sistema de Jogo/Avatar Database")]
public class AvatarDatabase : ScriptableObject
{
    // A lista usa a sua classe de dados: ItemLoja_AvatarData
    public List<ItemLoja_AvatarData> todosOsAvatares;

    // A função para encontrar o sprite pelo ID
    public Sprite EncontrarSpriteDoAvatarPeloID(int id)
    {
        // Verifica se a lista existe para evitar erros
        if (todosOsAvatares == null)
        {
            return null;
        }

        // Loop para encontrar o avatar correspondente
        foreach (ItemLoja_AvatarData avatar in todosOsAvatares)
        {
            // Pula itens nulos na lista para evitar erros
            if (avatar == null)
            {
                continue;
            }
            
            // Compara o ID do avatar na lista com o ID procurado
            if (avatar.avatarID == id)
            {
                // Retorna o sprite se encontrar a correspondência
                return avatar.iconeAvatar;
            }
        }

        // Se o loop terminar sem encontrar, o ID não existe na lista. Retorna null.
        return null;
    }
}