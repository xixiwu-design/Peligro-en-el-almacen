
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

    private const float TIEMPO_SOLICITUD = 0.15f;

    private class SolicitudEmpuje
    {
        public JugadorColisionController jugador;
        public Vector3 direccion;
        public float tiempo;
        public float tiempoFisico;
    }

    private Dictionary<int, SolicitudEmpuje> solicitudes =
        new Dictionary<int, SolicitudEmpuje>();

    void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        if (cajaCollider == null)
            cajaCollider = GetComponent<Collider>();

        requiereDosJugadores = CompareTag("CajaMoviDoble");
    }

    // Registrar la fuerza de un jugador
    public void RegistrarEmpuje(
     JugadorColisionController jugador,
     Vector3 direccion)
    {
        if (jugador == null || direccion == Vector3.zero)
            return;

        solicitudes[jugador.IdJugador] = new SolicitudEmpuje
        {
            jugador = jugador,
            direccion = direccion.normalized,
            tiempo = Time.time,
            tiempoFisico = Time.fixedTime
        };
    }
    // Cancelar la solicitud
    public void CancelarEmpuje(
        JugadorColisionController jugador)
    {
        if (jugador == null)
            return;

        solicitudes.Remove(jugador.IdJugador);
    }

    // Comprobar si el jugador puede empujar
    public bool PuedeEmpujar(
        JugadorColisionController jugador,
        Vector3 direccion)
    {
        if (jugador == null)
            return false;

        LimpiarSolicitudes();

        if (!solicitudes.ContainsKey(jugador.IdJugador))
            return false;

        // CAJA PEQUEÑA:
        // Se permite combinar las fuerzas de diferentes jugadores.
        if (!requiereDosJugadores)
        {
            List<SolicitudEmpuje> empujadores =
                ObtenerTodosLosEmpujadores();

            Vector3 fuerzaTotal = CalcularDireccionTotal(empujadores);

            if (fuerzaTotal == Vector3.zero)
                return false;

            return DestinoLibre(
                fuerzaTotal,
                velocidad * Time.fixedDeltaTime + 0.03f,
                empujadores
            );
        }

        // CAJA GRANDE:
        // Se necesitan dos jugadores en la misma dirección y lado.
        List<SolicitudEmpuje> grupo =
            ObtenerEmpujadores(direccion);

        bool jugadorRegistrado = false;

        foreach (SolicitudEmpuje solicitud in grupo)
        {
            if (solicitud.jugador == jugador)
            {
                jugadorRegistrado = true;
                break;
            }
        }

        if (!jugadorRegistrado || grupo.Count < 2)
            return false;

        return DestinoLibre(
            direccion,
            velocidad * Time.fixedDeltaTime + 0.03f,
            grupo
        );
    }


    void FixedUpdate()
    {
        List<SolicitudEmpuje> empujadores;
        Vector3 direccion;

        if (requiereDosJugadores)
        {
            // CAJA GRANDE:
            // Dos jugadores deben empujar en la misma dirección.
            empujadores = ObtenerTodosLosEmpujadores();

            if (empujadores.Count == 0)
                return;

            direccion = empujadores[0].direccion;

            empujadores = ObtenerEmpujadores(direccion);

            if (empujadores.Count < 2)
                return;
        }
        else
        {
            // CAJA PEQUEÑA:
            // Combinar las fuerzas de diferentes jugadores.
            empujadores = ObtenerTodosLosEmpujadores();

            if (empujadores.Count == 0)
                return;

            direccion = CalcularDireccionTotal(empujadores);

            if (direccion == Vector3.zero)
                return;
        }

        float distancia = velocidad * Time.fixedDeltaTime;

        Vector3 desplazamiento =
            direccion.normalized * distancia;

        // Comprobar el destino de la caja
        if (!DestinoLibre(
            direccion,
            distancia + 0.03f,
            empujadores))
        {
            return;
        }

        // Obtener los jugadores que participan
        List<JugadorColisionController> jugadores =
            new List<JugadorColisionController>();

        foreach (SolicitudEmpuje solicitud in empujadores)
        {
            if (!jugadores.Contains(solicitud.jugador))
                jugadores.Add(solicitud.jugador);
        }

        // Comprobar que todos pueden acompañar la caja
        foreach (JugadorColisionController jugador in jugadores)
        {
            if (!jugador.PuedeAcompanharCaja(
                desplazamiento,
                this,
                jugadores))
            {
                return;
            }
        }

        // Mover la caja
        rb.MovePosition(rb.position + desplazamiento);

        // Mover también a todos los jugadores
        foreach (JugadorColisionController jugador in jugadores)
        {
            jugador.MoverConCaja(desplazamiento);
        }
    }

    // Obtener todos los jugadores con solicitudes válidas
    private List<SolicitudEmpuje> ObtenerTodosLosEmpujadores()
    {
        List<SolicitudEmpuje> resultado =
            new List<SolicitudEmpuje>();

        LimpiarSolicitudes();

        foreach (SolicitudEmpuje solicitud in solicitudes.Values)
        {
            if (solicitud.jugador == null)
                continue;

            // Cada jugador debe estar detrás de la caja
            // respecto a su propia dirección.
            if (!EstaEnElMismoLado(
                solicitud,
                solicitud.direccion))
            {
                continue;
            }

            resultado.Add(solicitud);
        }

        return resultado;
    }

    // Obtener jugadores que empujan en la misma dirección y desde el mismo lado
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

    // Sumar las fuerzas de todos los jugadores
    private Vector3 CalcularDireccionTotal(
        List<SolicitudEmpuje> empujadores)
    {
        Vector3 fuerzaTotal = Vector3.zero;

        foreach (SolicitudEmpuje solicitud in empujadores)
        {
            fuerzaTotal += solicitud.direccion;
        }

        if (fuerzaTotal.sqrMagnitude < 0.0001f)
            return Vector3.zero;

        return fuerzaTotal.normalized;
    }

    // Comprobar que el jugador está detrás de la caja
    private bool EstaEnElMismoLado(
        SolicitudEmpuje solicitud,
        Vector3 direccion)
    {
        Vector3 diferencia =
            solicitud.jugador.Centro -
            cajaCollider.bounds.center;

        float producto = Vector3.Dot(
            diferencia,
            direccion.normalized
        );

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
            limites.extents,
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

            // Ignorar los jugadores que están empujando
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

            return false;
        }

        return true;
    }
}
