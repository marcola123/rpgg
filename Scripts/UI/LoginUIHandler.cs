using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;

public class LoginUIHandler : MonoBehaviour
{
    [SerializeField] private TMP_InputField _usernameInput;
    [SerializeField] private TMP_InputField _passwordInput;
    [SerializeField] private Button _loginButton;
    [SerializeField] private TextMeshProUGUI _statusText;

    private AuthenticationService _authService;

    void Start()
    {
        _authService = FindObjectOfType<AuthenticationService>();
        if (_authService == null)
        {
            Debug.LogError("AuthenticationService não encontrado na cena. Certifique-se de que ele está presente.");
            // Adiciona um AuthenticationService se não houver um na cena
            _authService = gameObject.AddComponent<AuthenticationService>();
        }

        _loginButton.onClick.AddListener(OnLoginButtonClicked);
        _statusText.text = "";
    }

    private async void OnLoginButtonClicked()
    {
        string username = _usernameInput.text;
        string password = _passwordInput.text;

        _statusText.text = "Autenticando...";
        _loginButton.interactable = false;

        bool isAuthenticated = await _authService.Authenticate(username, password);

        if (isAuthenticated)
        {
            _statusText.text = "Autenticação bem-sucedida. Conectando à rede...";
            // Tenta iniciar o jogo como host (servidor autoritário)
            // Em um cenário real, você pode ter um botão para 'Host' e outro para 'Join'
            // Para este protótipo, vamos assumir que o primeiro a logar é o host/servidor
            if (NetworkManager.Instance != null)
            {
                await NetworkManager.Instance.StartGame(GameMode.Host, "MyTestSession");
            }
            else
            {
                _statusText.text = "Erro: NetworkManager não encontrado.";
                Debug.LogError("NetworkManager.Instance é nulo. Certifique-se de que o NetworkManager está configurado corretamente.");
                _loginButton.interactable = true;
            }
        }
        else
        {
            _statusText.text = "Falha na autenticação. Verifique usuário e senha.";
            _loginButton.interactable = true;
        }
    }

    // Callback do NetworkManager para quando a conexão é estabelecida
    public void OnConnectedToNetwork()
    {
        _statusText.text = "Conectado à rede. Carregando cena...";
        // A transição de cena será tratada pelo NetworkManager no OnPlayerJoined
    }

    // Callback do NetworkManager para quando a conexão falha
    public void OnConnectionFailed()
    {
        _statusText.text = "Falha ao conectar à rede.";
        _loginButton.interactable = true;
    }
}

