
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // Si es modo paleja
    public bool DosJugadores = false;
    private static string escenaNivelCompletado;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            DosJugadores = Instance.DosJugadores;
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // inicia modo solo
    public void IniciarModoSolo()
    {
        DosJugadores = false;
        SceneManager.LoadScene("Solo1");
    }

    // Inicia modo dole
    public void IniciarModoDoble()
    {
        DosJugadores = true;
        SceneManager.LoadScene("Doble1");
    }

    // Cuando termina nivel ir al Sigiente Pagina
    public void CompletarNivel()
    {
        escenaNivelCompletado = SceneManager.GetActiveScene().name;
        Debug.Log("Nivel completado: " + escenaNivelCompletado);
        SceneManager.LoadScene("SigienteNivel");
    }

    // 
    public void SiguienteNivel()
    {
        string escena = escenaNivelCompletado;

        Debug.Log("SiguienteNivel: avanzando desde " + escena);

        if (escena == "1 Jugador")
            SceneManager.LoadScene("Ganado");

        else if (escena == "Solo2")
            SceneManager.LoadScene("Solo3");

        else if (escena == "Solo3")
            SceneManager.LoadScene("Ganado");

        else if (escena == "2 Nivel1")
            SceneManager.LoadScene("Ganado");

        else if (escena == "Doble2")
            SceneManager.LoadScene("Doble3");

        else if (escena == "Doble3")
            SceneManager.LoadScene("Ganado");
        else
        {
            Debug.LogWarning("No hay una siguiente escena configurada para: " + escena);
        }
    }
}
