using UnityEngine;

// A anotação abaixo permite criar "arquivos de pergunta" dentro da Unity
[CreateAssetMenu(fileName = "Nova Pergunta", menuName = "Quiz/Pergunta")]
public class Pergunta : ScriptableObject
{
    [TextArea(3, 10)] // Faz o campo de texto ser maior no Inspector
    public string textoDaPergunta;
    public string[] respostas; // Array para guardar as 4 respostas
    public int indiceRespostaCorreta; // Qual dos 4 é o correto (0, 1, 2 ou 3)
    [TextArea(2,5)]
    public string dica;

    public Sprite imagemDaPergunta;
}