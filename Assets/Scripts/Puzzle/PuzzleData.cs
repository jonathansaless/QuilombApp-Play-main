using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Novo Puzzle", menuName = "Puzzle/Puzzle Data")]
public class PuzzleData : NivelDataBase
{
    [Header("Regras do Nível")]
    public int NumeroMaxMovimentos = 10;

    [Header("Conteúdo da Explicação")]
    public string titulo; 
    public Sprite imagemPrincipal;
    [TextArea(10, 20)]
    public string textoDaExplicacao;
    public string fonteExplicacao;

    [Header("Conteúdo do Puzzle")]
    public Sprite imagemReferencia;
    public List<Sprite> pecasDoPuzzle; 
    
}