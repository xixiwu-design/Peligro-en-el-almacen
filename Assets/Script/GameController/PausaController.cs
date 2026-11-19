using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PausaController : MonoBehaviour
{
    private const string EscenaPausa = "EscenaPausa";
    private const string EscenaInstrucciones = "EscenaInstrucciones";

    // Variables para controlar el estado de pausa y las escenas involucradas
    private static readonly List<EventSystem> sistemasEventoJuego =
        new List<EventSystem>();

    //Estado de pausa
    private static bool pausaActiva;
    //Guardar escena juego 
    private static string escenaJuegoPausada;
    //La pagina pausa o instruccion
    private static string escenaOverlayActual;

    public void PausarJuego()
    {
        //Evitar hace 2 veces de pausa
        if (pausaActiva)
            return;

        escenaJuegoPausada = SceneManager.GetActiveScene().name;
        if (escenaJuegoPausada == EscenaPausa ||
            escenaJuegoPausada == EscenaInstrucciones)
        {
            Debug.LogWarning("No se puede pausar desde una escena de pausa.");
            return;
        }

        //Si esta pausando velocidad del tiempo = 0 (normal 1)
        pausaActiva = true;
        Time.timeScale = 0f;

        sistemasEventoJuego.Clear();
        foreach (EventSystem sistema in FindObjectsByType<EventSystem>())
        {
            if (!sistema.enabled)
                continue;

            sistemasEventoJuego.Add(sistema);
            sistema.enabled = false;
        }

        CargarOverlay(EscenaPausa);
    }

    public void ContinuarJuego()
    {
        if (!pausaActiva)
        {
            Debug.LogWarning("No hay una partida pausada para continuar.");
            return;
        }

        Scene overlay = SceneManager.GetSceneByName(escenaOverlayActual);
        if (!overlay.IsValid() || !overlay.isLoaded)
        {
            FinalizarPausa();
            return;
        }

        Scene juego = SceneManager.GetSceneByName(escenaJuegoPausada);
        if (juego.IsValid() && juego.isLoaded)
            SceneManager.SetActiveScene(juego);

        AsyncOperation descarga = SceneManager.UnloadSceneAsync(overlay);
        if (descarga == null)
        {
            Debug.LogError("No se pudo cerrar la escena de pausa.");
            return;
        }

        descarga.completed += _ => FinalizarPausa();
    }

    public void ReiniciarJuego()
    {
        string escena = pausaActiva
            ? escenaJuegoPausada
            : SceneManager.GetActiveScene().name;

        LimpiarEstadoPausa();
        Time.timeScale = 1f;
        SceneManager.LoadScene(escena);
    }

    public void AlInstruccion()
    {
        if (pausaActiva)
            CambiarOverlay(EscenaInstrucciones);
        else
            SceneManager.LoadScene(EscenaInstrucciones);
    }

    public void AlSigienteNivel()
    {
        LimpiarEstadoPausa();
        Time.timeScale = 1f;
        SceneManager.LoadScene("SigienteNivel");
    }

    public void AlPausa()
    {
        if (pausaActiva)
            CambiarOverlay(EscenaPausa);
        else
            SceneManager.LoadScene(EscenaPausa);
    }

    public void AlMenu()
    {
        LimpiarEstadoPausa();
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }

    
private static void CambiarOverlay(string nombreEscena)
{
    Scene overlayAnterior =
        SceneManager.GetSceneByName(escenaOverlayActual);

    if (!overlayAnterior.IsValid() || !overlayAnterior.isLoaded)
    {
        CargarOverlay(nombreEscena);
        return;
    }

    // Primero cargar la nueva pantalla
    AsyncOperation carga =
        SceneManager.LoadSceneAsync(nombreEscena, LoadSceneMode.Additive);

    if (carga == null)
    {
        Debug.LogError("No se pudo cargar la nueva pantalla.");
        return;
    }

    carga.completed += _ =>
    {
        Scene nuevoOverlay =
            SceneManager.GetSceneByName(nombreEscena);

        if (!nuevoOverlay.IsValid() || !nuevoOverlay.isLoaded)
        {
            Debug.LogError("La nueva pantalla no se cargó correctamente.");
            return;
        }

        // Descargar la pantalla anterior
        AsyncOperation descarga =
            SceneManager.UnloadSceneAsync(overlayAnterior);

        if (descarga == null)
        {
            Debug.LogError("No se pudo cerrar la pantalla anterior.");
            return;
        }

        descarga.completed += _ =>
        {
            escenaOverlayActual = nombreEscena;
            SceneManager.SetActiveScene(nuevoOverlay);
        };
    };
}

    private static void CargarOverlay(string nombreEscena)
    {
        AsyncOperation carga =
            SceneManager.LoadSceneAsync(nombreEscena, LoadSceneMode.Additive);
        if (carga == null)
        {
            Debug.LogError("No se pudo cargar la pantalla: " + nombreEscena);
            FinalizarPausa();
            return;
        }

        carga.completed += _ =>
        {
            Scene overlay = SceneManager.GetSceneByName(nombreEscena);
            if (!overlay.IsValid() || !overlay.isLoaded)
            {
                Debug.LogError("La pantalla no se cargó correctamente: " + nombreEscena);
                FinalizarPausa();
                return;
            }

            escenaOverlayActual = nombreEscena;
            SceneManager.SetActiveScene(overlay);
        };
    }


    private static void FinalizarPausa()
    {
        Scene juego = SceneManager.GetSceneByName(escenaJuegoPausada);
        if (juego.IsValid() && juego.isLoaded)
            SceneManager.SetActiveScene(juego);

        foreach (EventSystem sistema in sistemasEventoJuego)
        {
            if (sistema != null)
                sistema.enabled = true;
        }

        sistemasEventoJuego.Clear();
        LimpiarEstadoPausa();
        Time.timeScale = 1f;
    }

    private static void LimpiarEstadoPausa()
    {
        pausaActiva = false;
        escenaJuegoPausada = null;
        escenaOverlayActual = null;
        sistemasEventoJuego.Clear();
    }
}
