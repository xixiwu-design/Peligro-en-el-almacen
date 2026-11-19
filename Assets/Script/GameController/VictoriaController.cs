
using UnityEngine;
using System.Collections.Generic;

public class VictoriaController : MonoBehaviour
{
    [Header("Configuración de la meta")]
    [SerializeField] private bool DosJugadores = false;

    private HashSet<int> enMeta = new HashSet<int>();

    private bool Ganar = false;

    private void OnTriggerEnter(Collider otro)
    {
        if (Ganar)
            return;

        // Comprobar si la meta tiene el Tag Finish
        if (!CompareTag("Finish"))
            return;

        // Obtener el jugador
        JugadorColisionController jugador =
            otro.GetComponentInParent<JugadorColisionController>();

        if (jugador == null)
            return;

        // Registrar al jugador
        enMeta.Add(jugador.IdJugador);

        Debug.Log("Jugador " + jugador.IdJugador + " ha llegado a la meta.");

        // Hacer desaparecer al jugador
        jugador.gameObject.SetActive(false);

        // Comprobar si todos han llegado
        if (!DosJugadores ||
            (enMeta.Contains(1) && enMeta.Contains(2)))
        {
            FinalizarPartida();
        }
    }

    private void FinalizarPartida()
    {
        Ganar = true;

        Debug.Log("Nivel completado");

        GameManager.Instance.CompletarNivel();
    }
}
