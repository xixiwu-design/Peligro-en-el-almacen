
using UnityEngine;
using UnityEngine.SceneManagement;

public class PausaController : MonoBehaviour
{
    
    private void Awake()
    {
        string escenaActual = SceneManager.GetActiveScene().name;

        // No guardar escenas que no sean niveles de juego
        if (escenaActual == "1Jugador" ||
            escenaActual == "2Jugadores")
        {
            PlayerPrefs.SetString("Escena", escenaActual);
            PlayerPrefs.Save();
        }
    }

    public void PausarJuego()
    {
        // Guardar el nombre de la escena actual
        PlayerPrefs.SetString(
            "EscenaAnterior",
            SceneManager.GetActiveScene().name
        );

        PlayerPrefs.Save();

        SceneManager.LoadScene("EscenaPausa");
    }

    public void ContinuarJuego()
    {
        string escena = PlayerPrefs.GetString("EscenaAnterior");

        if (!string.IsNullOrEmpty(escena))
        {
            SceneManager.LoadScene(escena);
        }
    }

    public void ReiniciarJuego()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void AlInstruccion()
    {
        SceneManager.LoadScene("EscenaInstrucciones");
    }
    //Al escena Sigiente nivel no es juego
    public void AlSigienteNivel()
    {
        SceneManager.LoadScene("SigienteNivel");
    }


    public void AlPausa()
    {
        SceneManager.LoadScene("EscenaPausa");
    }

    public void AlMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    public void FalladoReintentar()
    {
        string Escena = PlayerPrefs.GetString("Escena");
        SceneManager.LoadScene(Escena);
    }

}
