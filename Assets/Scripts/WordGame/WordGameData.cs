using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Novo Jogo de Palavras", menuName = "Jogo de Palavras/Nível Data")]
public class WordGameData : NivelDataBase
{
    [Header("Regras do Nível")]
    public int NumeroMaxMovimentos = 30;

    [Header("Conteúdo da Explicação")]
    public string titulo;
    public Sprite imagemPrincipal;
    [TextArea(10, 20)]
    public string textoDaExplicacao;
    public string fonteExplicacao;

    [Header("Conteúdo do Nível")]
    public string respostaCorreta; // A palavra que deve ser formada. Ex: TURBANTE
    public Sprite imagemDeDica;
    
    [TextArea(3, 5)]
    public string textoDaPergunta; // A pergunta que o avatar faz.

    [Header("Letras")]
    [Tooltip("Todas as letras que aparecerão para o jogador, incluindo as corretas e as extras. Ex: TURBANTEIUJB")]
    public string letrasParaEmbaralhar;
}