using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Novo Nível de Quiz", menuName = "Quiz/Quiz Level Data")]
public class QuizData : NivelDataBase
{
    [Header("Configuração do Nível")]
    [Tooltip("Tempo limite para o quiz em segundos (ex: 300 para 5 minutos)")]
    public float tempoLimiteEmSegundos = 300f; // Ex: 5 minutos por padrão

    [Header("Conteúdo do Quiz")]
    public List<Pergunta> listaDePerguntas;
}