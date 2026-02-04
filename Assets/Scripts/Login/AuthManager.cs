using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions; // <- para validar e-mail

public class AuthManager : MonoBehaviour
{
    [Header("Configurações")]
    public float tempoMinimoCarregamento = 5.0f; // Duração mínima em segundos

    [Header("Firebase")]
    private FirebaseAuth auth;
    private FirebaseFirestore db;
    private FirebaseUser user;

    [Header("Painéis")]
    public GameObject painelLogin;
    public GameObject painelRegistro;
    public GameObject painelVisitante;
    public GameObject painelCarregamento;

    [Header("UI Login")]
    public TMP_InputField emailLoginInput;
    public TMP_InputField senhaLoginInput;
    public TextMeshProUGUI textoFeedbackLogin;

    [Header("UI Registro")]
    public TMP_InputField emailRegistroInput;
    public TMP_InputField apelidoRegistroInput;
    public TMP_InputField idadeRegistroInput;
    public TMP_InputField senhaRegistroInput;
    public TextMeshProUGUI textoFeedbackRegistro;

    [Header("UI Visitante")]
    public TMP_InputField apelidoVisitanteInput;
    public TMP_InputField idadeVisitanteInput;
    public TextMeshProUGUI textoFeedbackVisitante;

    void Start()
    {
        // Configura restrições dos InputFields (idade, e-mail, etc.)
        ConfigurarInputs();

        // Garante que os painéis comecem em um estado conhecido
        MostrarPainelLogin();
        if (painelCarregamento != null) painelCarregamento.SetActive(false);

        // Apenas inicializa as variáveis do Firebase
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                auth = FirebaseAuth.DefaultInstance;
                db = FirebaseFirestore.DefaultInstance;
            }
            else
            {
                Debug.LogError($"Erro nas dependências do Firebase: {dependencyStatus}");
                if (textoFeedbackLogin != null)
                {
                    textoFeedbackLogin.text = "Erro ao iniciar serviços online. Verifique a configuração do Firebase.";
                }
            }
        });
    }

    /// <summary>
    /// Configura os InputFields (idade só números, máx 3, e e-mail com tipo adequado)
    /// </summary>
    private void ConfigurarInputs()
    {
        // Idade registro – apenas números, até 3 dígitos
        if (idadeRegistroInput != null)
        {
            idadeRegistroInput.contentType = TMP_InputField.ContentType.IntegerNumber;
            idadeRegistroInput.characterLimit = 3;
            idadeRegistroInput.ForceLabelUpdate();
        }

        // Idade visitante – apenas números, até 3 dígitos
        if (idadeVisitanteInput != null)
        {
            idadeVisitanteInput.contentType = TMP_InputField.ContentType.IntegerNumber;
            idadeVisitanteInput.characterLimit = 3;
            idadeVisitanteInput.ForceLabelUpdate();
        }

        // E-mails – tipo e-mail ajuda no teclado virtual
        if (emailRegistroInput != null)
        {
            emailRegistroInput.contentType = TMP_InputField.ContentType.EmailAddress;
            emailRegistroInput.ForceLabelUpdate();
        }

        if (emailLoginInput != null)
        {
            emailLoginInput.contentType = TMP_InputField.ContentType.EmailAddress;
            emailLoginInput.ForceLabelUpdate();
        }
    }

    #region Funções de UI
    public void MostrarPainelLogin()
    {
        painelLogin.SetActive(true);
        painelRegistro.SetActive(false);
        painelVisitante.SetActive(false);
        if (painelCarregamento != null) painelCarregamento.SetActive(false);
    }

    public void MostrarPainelRegistro()
    {
        painelLogin.SetActive(false);
        painelRegistro.SetActive(true);
        painelVisitante.SetActive(false);
        if (painelCarregamento != null) painelCarregamento.SetActive(false);
    }

    public void MostrarPainelVisitante()
    {
        painelLogin.SetActive(false);
        painelRegistro.SetActive(false);
        painelVisitante.SetActive(true);
        if (painelCarregamento != null) painelCarregamento.SetActive(false);
    }

    private void MostrarPainelCarregamento()
    {
        painelLogin.SetActive(false);
        painelRegistro.SetActive(false);
        painelVisitante.SetActive(false);
        if (painelCarregamento != null) painelCarregamento.SetActive(true);
    }
    #endregion

    #region Funções auxiliares

    // Verifica conexão com a internet e mostra mensagem no TextMeshPro
    private bool VerificarInternetEExibirMensagem(TMP_Text textoFeedback)
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            if (textoFeedback != null)
            {
                textoFeedback.text = "Sem conexão com a internet. Verifique sua rede e tente novamente.";
            }

            Debug.LogWarning("Tentativa de login/registro sem conexão com a internet.");
            return false;
        }

        return true;
    }

    // Validação simples de e-mail (formato básico)
    private bool EmailEhValido(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        // Regex simples para validar estrutura de e-mail
        const string padrao = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, padrao);
    }

    #endregion

    #region Funções de Autenticação

    public async void Registrar()
    {
        if (string.IsNullOrEmpty(emailRegistroInput.text) ||
            string.IsNullOrEmpty(senhaRegistroInput.text) ||
            string.IsNullOrEmpty(apelidoRegistroInput.text) ||
            string.IsNullOrEmpty(idadeRegistroInput.text))
        {
            textoFeedbackRegistro.text = "Por favor, preencha todos os campos.";
            return;
        }

        // 🔹 Valida formato do e-mail
        if (!EmailEhValido(emailRegistroInput.text))
        {
            textoFeedbackRegistro.text = "Digite um e-mail válido.";
            return;
        }

        // 🔹 Valida idade (já está limitado a 3 dígitos, mas garantimos aqui)
        if (!int.TryParse(idadeRegistroInput.text, out int idade) || idade < 0)
        {
            textoFeedbackRegistro.text = "Digite uma idade válida.";
            return;
        }

        // 🔹 Verifica conexão antes de chamar o Firebase
        if (!VerificarInternetEExibirMensagem(textoFeedbackRegistro))
            return;

        textoFeedbackRegistro.text = "Registrando...";
        MostrarPainelCarregamento();

        try
        {
            var registerTask = await auth.CreateUserWithEmailAndPasswordAsync(emailRegistroInput.text, senhaRegistroInput.text);
            user = registerTask.User;

            UserProfile profile = new UserProfile { DisplayName = apelidoRegistroInput.text };
            await user.UpdateUserProfileAsync(profile);

            Task creationTask = CriarDadosIniciaisDoJogador(user.UserId, apelidoRegistroInput.text, idade);
            Task delayTask = Task.Delay((int)(tempoMinimoCarregamento * 1000));
            await Task.WhenAll(creationTask, delayTask);

            await PlayerDataManager.Instance.CarregarDadosDoFirebase();
            SceneManager.LoadScene("CenaMenu");
        }
        catch (FirebaseException ex)
        {
            MostrarPainelRegistro();
            textoFeedbackRegistro.text = ObterMensagemDeErro(ex);
        }
    }

    public async void FazerLogin()
    {
        if (string.IsNullOrEmpty(emailLoginInput.text) || string.IsNullOrEmpty(senhaLoginInput.text))
        {
            textoFeedbackLogin.text = "Preencha e-mail e senha.";
            return;
        }

        // 🔹 Valida formato do e-mail
        if (!EmailEhValido(emailLoginInput.text))
        {
            textoFeedbackLogin.text = "Digite um e-mail válido.";
            return;
        }

        // 🔹 Verifica conexão antes de chamar o Firebase
        if (!VerificarInternetEExibirMensagem(textoFeedbackLogin))
            return;

        textoFeedbackLogin.text = "Entrando...";
        MostrarPainelCarregamento();

        try
        {
            var loginTask = await auth.SignInWithEmailAndPasswordAsync(emailLoginInput.text, senhaLoginInput.text);
            user = loginTask.User;

            Task loadingTask = PlayerDataManager.Instance.CarregarDadosDoFirebase();
            Task delayTask = Task.Delay((int)(tempoMinimoCarregamento * 1000));
            await Task.WhenAll(loadingTask, delayTask);

            SceneManager.LoadScene("CenaMenu");
        }
        catch (FirebaseException ex)
        {
            MostrarPainelLogin();
            textoFeedbackLogin.text = ObterMensagemDeErro(ex);
        }
    }

    public async void LoginComoVisitante()
    {
        if (string.IsNullOrEmpty(apelidoVisitanteInput.text))
        {
            textoFeedbackVisitante.text = "Por favor, insira um apelido.";
            return;
        }

        // Idade do visitante é opcional? Se quiser obrigatória, descomenta o bloco abaixo:
        if (!string.IsNullOrEmpty(idadeVisitanteInput.text))
        {
            if (!int.TryParse(idadeVisitanteInput.text, out int idadeTemp) || idadeTemp < 0)
            {
                textoFeedbackVisitante.text = "Digite uma idade válida.";
                return;
            }
        }

        // 🔹 Verifica conexão antes do login anônimo
        if (!VerificarInternetEExibirMensagem(textoFeedbackVisitante))
            return;

        textoFeedbackVisitante.text = "Entrando...";
        MostrarPainelCarregamento();

        try
        {
            var loginTask = await auth.SignInAnonymouslyAsync();
            user = loginTask.User;

            UserProfile profile = new UserProfile { DisplayName = apelidoVisitanteInput.text };
            await user.UpdateUserProfileAsync(profile);

            int.TryParse(idadeVisitanteInput.text, out int idade); // Converte idade de forma segura

            Task creationTask = CriarDadosIniciaisDoJogador(user.UserId, apelidoVisitanteInput.text, idade);
            Task delayTask = Task.Delay((int)(tempoMinimoCarregamento * 1000));
            await Task.WhenAll(creationTask, delayTask);

            await PlayerDataManager.Instance.CarregarDadosDoFirebase();
            SceneManager.LoadScene("CenaMenu");
        }
        catch (FirebaseException ex)
        {
            MostrarPainelVisitante();
            textoFeedbackVisitante.text = ObterMensagemDeErro(ex);
        }
    }
    #endregion

    private async Task CriarDadosIniciaisDoJogador(string userId, string apelido, int idade)
    {
        DocumentReference docRef = db.Collection("jogadores").Document(userId);
        Dictionary<string, object> dadosIniciais = new Dictionary<string, object>
        {
            { "Apelido", apelido },
            { "Idade", idade },
            { "Vidas", 5 },
            { "Moedas", 0 },
            { "PontuacaoMaximaTotal", 0 },
            { "QuantidadeDica", 3 },
            { "Quantidade5050", 3 },
            { "QuantidadeDuasChances", 3 },
            { "AvataresPossuidos", new List<int> { 0 } },
            { "AvatarEquipadoID", 0 }
        };
        await docRef.SetAsync(dadosIniciais);
    }

    // Função auxiliar para traduzir os erros mais comuns do Firebase
    private string ObterMensagemDeErro(FirebaseException ex)
    {
        AuthError errorCode = (AuthError)ex.ErrorCode;
        switch (errorCode)
        {
            case AuthError.WrongPassword:
                return "Senha incorreta.";
            case AuthError.UserNotFound:
                return "Usuário não encontrado.";
            case AuthError.InvalidEmail:
                return "O e-mail fornecido é inválido.";
            case AuthError.EmailAlreadyInUse:
                return "Este e-mail já está em uso.";
            case AuthError.WeakPassword:
                return "A senha precisa ter pelo menos 6 caracteres.";
            default:
                return "Ocorreu um erro. Tente novamente.";
        }
    }
}
