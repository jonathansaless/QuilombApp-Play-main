// Crie este novo script
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Novo Modo de Jogo", menuName = "Sistema de Jogo/Modo de Jogo Data")]
public class ModoDeJogoData : ScriptableObject
{
    public string nomeDoModo; // Ex: "Quiz de Cultura"
    public string nomeDaCena; // Ex: "CenaQuiz"
    public List<NivelDataBase> niveis; // Uma lista com todos os níveis deste modo!
}