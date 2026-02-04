using UnityEngine;
using Firebase.Auth;
using Firebase.Firestore;
using System.Threading.Tasks;
using System.Collections.Generic;

// [FirestoreData] diz ao Firebase que esta classe pode ser convertida.
[FirestoreData]
public class PlayerData
{
    // [FirestoreProperty] marca cada campo que deve ser salvo/lido do banco de dados.
    [FirestoreProperty]
    public string Apelido { get; set; }
    [FirestoreProperty]
    public int Idade { get; set; }
    [FirestoreProperty]
    public int Vidas { get; set; }
    [FirestoreProperty]
    public int Moedas { get; set; }
    [FirestoreProperty]
    public Dictionary<string, long> PontuacoesPorNivel { get; set; }
    
    // Antiga pontuação total. Pode ser mantida por compatibilidade ou removida
    // se você fizer uma migração de dados.
    [FirestoreProperty]
    public long PontuacaoMaximaTotal { get; set; }

    // --- NOVOS CAMPOS PARA RANKING ---
    [FirestoreProperty]
    public long PontuacaoTotalQuiz { get; set; }
    [FirestoreProperty]
    public long PontuacaoTotalPuzzle { get; set; }
    [FirestoreProperty]
    public long PontuacaoTotalWordGame { get; set; }
    // ---------------------------------

    [FirestoreProperty]
    public int QuantidadeDica { get; set; }
    [FirestoreProperty]
    public int Quantidade5050 { get; set; }
    [FirestoreProperty]
    public int QuantidadeDuasChances { get; set; }
    [FirestoreProperty]
    public List<int> AvataresPossuidos { get; set; } // Guarda os avatares que o usuário possui
    [FirestoreProperty]
    public int AvatarEquipadoID { get; set; } // Guarda o ID do avatar em uso
    [FirestoreProperty]
    public string UltimoResetVidas { get; set; } // Guarda a data no formato "AAAA-MM-DD"
    
    public PlayerData() 
    {
        // Inicializa os novos campos para evitar problemas com valores nulos
        // ao criar novos jogadores.
        PontuacoesPorNivel = new Dictionary<string, long>();
        AvataresPossuidos = new List<int>();
        PontuacaoTotalQuiz = 0;
        PontuacaoTotalPuzzle = 0;
        PontuacaoTotalWordGame = 0;
    }
}

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance { get; private set; }

    public PlayerData Dados { get; set; } // Permitimos a escrita para a função de logout
    private DocumentReference docRef;
    private bool isDataLoaded = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public async Task<bool> CarregarDadosDoFirebase()
    {
        if (FirebaseAuth.DefaultInstance.CurrentUser == null)
        {
            Debug.LogError("Tentativa de carregar dados sem usuário logado.");
            return false;
        }

        string userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        docRef = FirebaseFirestore.DefaultInstance.Collection("jogadores").Document(userId);

        var snapshot = await docRef.GetSnapshotAsync();
        if (snapshot.Exists)
        {
            Dados = snapshot.ConvertTo<PlayerData>();
            // Inicializa listas e dicionários se estiverem nulos
            if (Dados.PontuacoesPorNivel == null)
            {
                Dados.PontuacoesPorNivel = new Dictionary<string, long>();
            }
            isDataLoaded = true;
            Debug.Log($"Dados do jogador {Dados.Apelido} carregados com sucesso.");
            return true;
        }
        else
        {
            Debug.LogError($"Documento do jogador com ID {userId} não foi encontrado no Firestore!");
            isDataLoaded = false;
            Dados = null;
            return false;
        }
    }

    public async Task SalvarDadosNoFirebase()
    {
        if (docRef == null || !isDataLoaded || Dados == null) return;
        await docRef.SetAsync(Dados);
        Debug.Log("Dados do jogador salvos na nuvem.");
    }

    private async void OnApplicationQuit()
    {
        Debug.Log("Detectado fechamento do aplicativo. Salvando dados...");
        await SalvarDadosNoFirebase();
    }

    public async Task CheckAndResetDailyLives()
    {
        // 1. Pega a data atual no formato universal AAAA-MM-DD
        string dataAtual = System.DateTime.UtcNow.ToString("yyyy-MM-dd");

        // 2. Pega a data do último reset que está salva nos dados do jogador
        string dataUltimoReset = Dados.UltimoResetVidas;

        Debug.Log($"Verificação de vidas diárias: Data Atual='{dataAtual}', Último Reset Salvo='{dataUltimoReset}'");

        // 3. Compara as datas
        if (dataAtual != dataUltimoReset)
        {
            // Se as datas forem diferentes, é um novo dia (ou o jogador nunca teve um reset)
            Debug.Log("<color=green>Novo dia detectado! Resetando as vidas do jogador.</color>");

            // Reseta as vidas para 5
            Dados.Vidas = 5;
            
            // Atualiza a data do último reset para a data de hoje
            Dados.UltimoResetVidas = dataAtual;

            // Salva as alterações no Firebase
            await SalvarDadosNoFirebase();
        }
        else
        {
            // Se as datas forem iguais, as vidas já foram resetadas hoje.
            Debug.Log("As vidas já foram resetadas hoje. Nenhuma ação necessária.");
        }
    }
}