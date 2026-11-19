
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
        SceneManager.LoadScene("1Jugadores 1");
    }

    // Inicia modo dole
    public void IniciarModoDoble()
    {
        DosJugadores = true;
        SceneManager.LoadScene("2Jugadores 1");
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

        if (escena == "1Jugadores 1")
            SceneManager.LoadScene("1Jugadores 2");

        else if (escena == "1Jugadores 2")
            SceneManager.LoadScene("1Jugadores 3");

        else if (escena == "1Jugadores 3")
            SceneManager.LoadScene("Ganado");

        else if (escena == "2Jugadores 1")
            SceneManager.LoadScene("2Jugadores 2");

        else if (escena == "2Jugadores 2")
            SceneManager.LoadScene("2Jugadores 3");

        else if (escena == "2Jugadores 3")
            SceneManager.LoadScene("Ganado");
        else
        {
            Debug.LogWarning("No hay una siguiente escena configurada para: " + escena);
        }
    }

    public void FalladoReintentar()
    {
        string escena = PlayerPrefs.GetString("Escena", string.Empty);
        if (string.IsNullOrEmpty(escena))
        {
            Debug.LogWarning("No hay una escena guardada para reintentar.");
            return;
        }

        SceneManager.LoadScene(escena);
    }
}
