
using UnityEngine;
using System.Collections.Generic;

public class JugadorColisionController : MonoBehaviour
{
    [Header("Configuración del jugador")]
    [SerializeField] private int idJugador = 1;

    [Header("Detección de otros jugadores")]
    [SerializeField] private float distanciaSeguridad = 0.05f;

    [Header("Componentes")]
    [SerializeField] private Collider jugadorCollider;
    [SerializeField] private Rigidbody rb;

    private CajaController cajaActual;

    public int IdJugador => idJugador;

    public bool EstaEmpujandoCaja => cajaActual != null;

    public Vector3 Centro => jugadorCollider.bounds.center;

    void Awake()
    {
        if (jugadorCollider == null)
            jugadorCollider = GetComponent<Collider>();

        if (rb == null)
            rb = GetComponent<Rigidbody>();
    }

    public void PrepararFrame()
    {
        CancelarEmpuje();
    }

    public bool PuedeMoverEnDireccion(
        Vector3 direccion,
        float distancia)
    {
        if (direccion == Vector3.zero)
            return false;

        Collider obstaculo = DetectarObstaculo(
            direccion,
            distancia
        );

        if (obstaculo == null)
            return !DetectarOtroJugador(direccion, distancia);

        CajaController caja =
            obstaculo.GetComponentInParent<CajaController>();

        // Si no es una caja, bloquear el movimiento
        if (caja == null)
            return false;

        if (cajaActual != null && cajaActual != caja)
            return false;

        cajaActual = caja;

        caja.RegistrarEmpuje(this, direccion);

        // El jugador no se mueve por su cuenta.
        // CajaController moverá al jugador junto con la caja.
        return false;
    }

    private Collider DetectarObstaculo(
        Vector3 direccion,
        float distancia)
    {
        SphereCollider esfera = jugadorCollider as SphereCollider;

        if (esfera == null)
            return null;

        Vector3 centro =
            esfera.transform.TransformPoint(esfera.center);

        Vector3 escala = esfera.transform.lossyScale;

        float radio = esfera.radius * Mathf.Max(
            Mathf.Abs(escala.x),
            Mathf.Abs(escala.y),
            Mathf.Abs(escala.z)
        );

        RaycastHit[] impactos = Physics.SphereCastAll(
            centro,
            radio * 0.95f,
            direccion.normalized,
            distancia + 0.15f,
            ~0,
            QueryTriggerInteraction.Ignore
        );

        System.Array.Sort(
            impactos,
            (a, b) => a.distance.CompareTo(b.distance)
        );

        foreach (RaycastHit impacto in impactos)
        {
            Collider colision = impacto.collider;

            if (colision.transform == transform ||
                colision.transform.IsChildOf(transform))
                continue;

            return colision;
        }

        return null;
    }

    private bool DetectarOtroJugador(
        Vector3 direccion,
        float distancia)
    {
        SphereCollider esfera = jugadorCollider as SphereCollider;

        if (esfera == null)
            return false;

        Vector3 centro =
            esfera.transform.TransformPoint(esfera.center);

        Vector3 escala = esfera.transform.lossyScale;

        float radio = esfera.radius * Mathf.Max(
            Mathf.Abs(escala.x),
            Mathf.Abs(escala.y),
            Mathf.Abs(escala.z)
        );

        Vector3 siguientePosicion =
            centro + direccion.normalized *
            (distancia * 2f + distanciaSeguridad);

        Collider[] impactos = Physics.OverlapSphere(
            siguientePosicion,
            radio,
            ~0,
            QueryTriggerInteraction.Ignore
        );

        foreach (Collider impacto in impactos)
        {
            JugadorColisionController otro =
                impacto.GetComponentInParent<JugadorColisionController>();

            if (otro != null && otro != this)
                return true;
        }

        return false;
    }

    // Comprobar si el jugador puede acompañar el movimiento de la caja
    public bool PuedeAcompanharCaja(
        Vector3 desplazamiento,
        CajaController caja,
        List<JugadorColisionController> empujadores)
    {
        SphereCollider esfera = jugadorCollider as SphereCollider;

        if (esfera == null)
            return false;

        float distancia = desplazamiento.magnitude;

        if (distancia <= 0.0001f)
            return true;

        Vector3 centro =
            esfera.transform.TransformPoint(esfera.center);

        Vector3 escala = esfera.transform.lossyScale;

        float radio = esfera.radius * Mathf.Max(
            Mathf.Abs(escala.x),
            Mathf.Abs(escala.y),
            Mathf.Abs(escala.z)
        );

        RaycastHit[] impactos = Physics.SphereCastAll(
            centro,
            radio * 0.95f,
            desplazamiento.normalized,
            distancia + 0.02f,
            ~0,
            QueryTriggerInteraction.Ignore
        );

        foreach (RaycastHit impacto in impactos)
        {
            Collider colision = impacto.collider;

            if (colision.transform == transform ||
                colision.transform.IsChildOf(transform))
                continue;

            if (colision.transform == caja.transform ||
                colision.transform.IsChildOf(caja.transform))
                continue;

            bool esEmpujador = false;

            foreach (JugadorColisionController jugador in empujadores)
            {
                if (colision.transform == jugador.transform ||
                    colision.transform.IsChildOf(jugador.transform))
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

    // Mover al jugador junto con la caja
    public void MoverConCaja(Vector3 desplazamiento)
    {
        if (rb == null)
            return;

        rb.MovePosition(rb.position + desplazamiento);
    }

    private void CancelarEmpuje()
    {
        if (cajaActual != null)
        {
            cajaActual.CancelarEmpuje(this);
            cajaActual = null;
        }
    }

    void OnDisable()
    {
        CancelarEmpuje();
    }
}
