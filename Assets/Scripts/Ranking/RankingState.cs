/// Enum para identificar os diferentes modos de jogo no ranking.
public enum RankingTab
{
    Quiz,
    Puzzle,
    WordGame
}

/// Classe estática simples para guardar o estado de qual aba do ranking
/// deve ser aberta ao carregar a cena de Ranking.

public static class RankingState
{
   
    /// A aba que deve ser exibida por padrão ao entrar na cena.
    /// O Menu e Feedbacks devem atualizar isso antes de carregar a CenaRanking.    
    public static RankingTab TabParaAbrir = RankingTab.Quiz;
}