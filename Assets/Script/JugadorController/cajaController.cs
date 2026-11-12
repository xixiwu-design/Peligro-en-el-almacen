
using UnityEngine;
using System.Collections.Generic;

[DefaultExecutionOrder(100)]
public class CajaController : MonoBehaviour
{
    [Header("Configuración de la caja")]
    [SerializeField] private float velocidad = 40f;
    [SerializeField] private bool requiereDosJugadores = false;

    [Header("Componentes")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Collider cajaCollider;

    // Tiempo máximo durante el que se considera válida una solicitud
    private const float TIEMPO_SOLICITUD = 0.15f;

    // Información de cada jugador que intenta empujar
    private class SolicitudEmpuje
    {
        public JugadorColisionController jugador;
        public Vector3 direccion;
        public float tiempo;
    }

    private Dictionary<int, SolicitudEmpuje> solicitudes =
        new Dictionary<int, SolicitudEmpuje>();

    void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        if (cajaCollider == null)
            cajaCollider = GetComponent<Collider>();

        // Detectar automáticamente si es una caja doble
        if (CompareTag("CajaMoviDoble"))
        {
            requiereDosJugadores = true;
        }
        else
        {
            requiereDosJugadores = false;
        }
    }

    // Registrar que un jugador está intentando empujar
    public void RegistrarEmpuje(
        JugadorColisionController jugador,
        Vector3 direccion)
    {
        if (jugador == null || direccion == Vector3.zero)
            return;

        SolicitudEmpuje solicitud = new SolicitudEmpuje();

        solicitud.jugador = jugador;
        solicitud.direccion = direccion.normalized;
        solicitud.tiempo = Time.time;

        solicitudes[jugador.IdJugador] = solicitud;
    }

    // Cancelar la solicitud de un jugador
    public void CancelarEmpuje(
        JugadorColisionController jugador)
    {
        if (jugador == null)
            return;

        solicitudes.Remove(jugador.IdJugador);
    }

    // Comprobar si un jugador puede empujar esta caja
    public bool PuedeEmpujar(
        JugadorColisionController jugador,
        Vector3 direccion)
    {
        List<SolicitudEmpuje> empujadores =
            ObtenerEmpujadores(direccion);

        bool jugadorRegistrado = false;

        foreach (SolicitudEmpuje solicitud in empujadores)
        {
            if (solicitud.jugador == jugador)
            {
                jugadorRegistrado = true;
                break;
            }
        }

        if (!jugadorRegistrado)
            return false;

        int necesarios = requiereDosJugadores ? 2 : 1;

        if (empujadores.Count < necesarios)
            return false;

        // Comprobar que el destino está libre
        return DestinoLibre(
            direccion,
            velocidad * Time.fixedDeltaTime + 0.03f,
            empujadores
        );
    }

    void FixedUpdate()
    {
        // Obtener los jugadores que están empujando en la misma dirección
        List<SolicitudEmpuje> empujadores =
            ObtenerTodosLosEmpujadores();

        if (empujadores.Count == 0)
            return;

        Vector3 direccion = empujadores[0].direccion;

        int necesarios = requiereDosJugadores ? 2 : 1;

        if (empujadores.Count < necesarios)
            return;

        // Para cajas dobles deben ser dos jugadores diferentes
        // empujando desde el mismo lado y en la misma dirección.
        if (requiereDosJugadores && empujadores.Count < 2)
            return;

        float distancia = velocidad * Time.fixedDeltaTime;

        if (!DestinoLibre(direccion, distancia + 0.03f, empujadores))
            return;

        // Mover la caja
        Vector3 desplazamiento = direccion * distancia;

        rb.MovePosition(rb.position + desplazamiento);
    }

    // Obtener solicitudes válidas en una dirección concreta
    private List<SolicitudEmpuje> ObtenerEmpujadores(
        Vector3 direccion)
    {
        List<SolicitudEmpuje> resultado =
            new List<SolicitudEmpuje>();

        LimpiarSolicitudes();

        foreach (SolicitudEmpuje solicitud in solicitudes.Values)
        {
            if (Vector3.Dot(
                solicitud.direccion,
                direccion.normalized) < 0.98f)
            {
                continue;
            }

            if (!EstaEnElMismoLado(solicitud, direccion))
                continue;

            resultado.Add(solicitud);
        }

        return resultado;
    }

    // Obtener todos los jugadores que están empujando
    private List<SolicitudEmpuje> ObtenerTodosLosEmpujadores()
    {
        List<SolicitudEmpuje> resultado =
            new List<SolicitudEmpuje>();

        LimpiarSolicitudes();

        if (solicitudes.Count == 0)
            return resultado;

        // Tomar la dirección de la primera solicitud
        Vector3 direccion = Vector3.zero;

        foreach (SolicitudEmpuje solicitud in solicitudes.Values)
        {
            direccion = solicitud.direccion;
            break;
        }

        foreach (SolicitudEmpuje solicitud in solicitudes.Values)
        {
            if (Vector3.Dot(
                solicitud.direccion,
                direccion) < 0.98f)
            {
                continue;
            }

            if (!EstaEnElMismoLado(solicitud, direccion))
                continue;

            resultado.Add(solicitud);
        }

        return resultado;
    }

    // Comprobar que el jugador está detrás de la caja
    private bool EstaEnElMismoLado(
        SolicitudEmpuje solicitud,
        Vector3 direccion)
    {
        Vector3 diferencia =
            solicitud.jugador.Centro -
            cajaCollider.bounds.center;

        float producto =
            Vector3.Dot(diferencia, direccion.normalized);

        // El jugador debe estar detrás de la caja
        return producto < -0.05f;
    }

    // Eliminar solicitudes antiguas
    private void LimpiarSolicitudes()
    {
        List<int> eliminar = new List<int>();

        foreach (var elemento in solicitudes)
        {
            if (Time.time - elemento.Value.tiempo > TIEMPO_SOLICITUD ||
                elemento.Value.jugador == null)
            {
                eliminar.Add(elemento.Key);
            }
        }

        foreach (int id in eliminar)
        {
            solicitudes.Remove(id);
        }
    }

    // Comprobar si el espacio delante de la caja está libre
    private bool DestinoLibre(
        Vector3 direccion,
        float distancia,
        List<SolicitudEmpuje> empujadores)
    {
        Bounds limites = cajaCollider.bounds;

        RaycastHit[] impactos = Physics.BoxCastAll(
            limites.center,
            limites.extents * 0.95f,
            direccion.normalized,
            Quaternion.identity,
            distancia,
            ~0,
            QueryTriggerInteraction.Ignore
        );

        foreach (RaycastHit impacto in impactos)
        {
            Collider colision = impacto.collider;

            // Ignorar la propia caja
            if (colision.transform == transform ||
                colision.transform.IsChildOf(transform))
            {
                continue;
            }

            // Ignorar a los jugadores que están empujando
            bool esEmpujador = false;

            foreach (SolicitudEmpuje solicitud in empujadores)
            {
                if (colision.transform == solicitud.jugador.transform ||
                    colision.transform.IsChildOf(
                        solicitud.jugador.transform))
                {
                    esEmpujador = true;
                    break;
                }
            }

            if (esEmpujador)
                continue;

            // Hay un obstáculo delante
            return false;
        }

        return true;
    }
}
