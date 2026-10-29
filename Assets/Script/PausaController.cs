
using UnityEngine;
using UnityEngine.SceneManagement;

public class PausaController : MonoBehaviour
{
    public void PausarJuego()
    {
        // Guardar el nombre de la escena actual
        PlayerPrefs.SetString(
            "EscenaAnterior",
            SceneManager.GetActiveScene().name
        );

        // Ir a la escena de pausa
        SceneManager.LoadScene("EscenaPausa");
    }

    public void ContinuarJuego()
    {
        
        // Recuperar la escena anterior
        string escena = PlayerPrefs.GetString(
            "EscenaAnterior",
            "1Jugador"
        );

        SceneManager.LoadScene(escena);
    }

    public void ReiniciarJuego()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void AlInstruccion()
    {
        SceneManager.LoadScene("EscenaInstrucciones");
    }

    public void AlPausa()
    {
        SceneManager.LoadScene("EscenaPausa");
    }

    public void AlMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}
