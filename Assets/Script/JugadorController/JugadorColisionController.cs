
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class JugadorColisionController : MonoBehaviour
{
    [Header("Configuración del jugador")]
    [SerializeField] private int idJugador = 1;
    [SerializeField] private float velocidad = 40f;

    [Header("Componentes")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Collider jugadorCollider;

    private Vector3 direccionMovimiento;
    private CajaController cajaActual;

    public int IdJugador => idJugador;

    public Vector3 Centro
    {
        get
        {
            return jugadorCollider.bounds.center;
        }
    }

    void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        if (jugadorCollider == null)
            jugadorCollider = GetComponent<Collider>();
    }

    public void EstablecerDireccion(Vector3 direccion)
    {
        direccionMovimiento = direccion.normalized;
    }

    void FixedUpdate()
    {
        if (direccionMovimiento == Vector3.zero)
        {
            CancelarEmpuje();
            return;
        }

        float distancia = velocidad * Time.fixedDeltaTime;

        // Detectar obstáculos delante del jugador
        Collider obstaculo = DetectarObstaculo(
            direccionMovimiento,
            distancia
        );

        // Si no hay obstáculos, mover al jugador
        if (obstaculo == null)
        {
            CancelarEmpuje();
            MoverJugador(distancia);
            return;
        }

        // Comprobar si el obstáculo es una caja móvil
        CajaController caja =
            obstaculo.GetComponentInParent<CajaController>();

        // Si no es una caja móvil, bloquear el movimiento
        if (caja == null)
        {
            CancelarEmpuje();
            return;
        }

        // Si hemos cambiado de caja, cancelar la solicitud anterior
        if (cajaActual != null && cajaActual != caja)
        {
            cajaActual.CancelarEmpuje(this);
        }

        cajaActual = caja;

        // Solicitar empujar la caja
        caja.RegistrarEmpuje(this, direccionMovimiento);

        // Comprobar si tenemos permiso para empujarla
        if (caja.PuedeEmpujar(this, direccionMovimiento))
        {
            MoverJugador(distancia);
        }
    }

    private void MoverJugador(float distancia)
    {
        // Girar al jugador hacia la dirección del movimiento
        Quaternion rotacion =
            Quaternion.LookRotation(direccionMovimiento) *
            Quaternion.Euler(0f, -90f, 0f);

        rb.MoveRotation(rotacion);

        // Mover al jugador
        Vector3 desplazamiento =
            direccionMovimiento * distancia;

        rb.MovePosition(rb.position + desplazamiento);
    }

    private Collider DetectarObstaculo(
        Vector3 direccion,
        float distancia)
    {
        Bounds limites = jugadorCollider.bounds;

        RaycastHit[] impactos = Physics.BoxCastAll(
            limites.center,
            limites.extents * 0.95f,
            direccion,
            Quaternion.identity,
            distancia + 0.03f,
            ~0,
            QueryTriggerInteraction.Ignore
        );

        // Ordenar los impactos por distancia
        System.Array.Sort(
            impactos,
            (a, b) => a.distance.CompareTo(b.distance)
        );

        foreach (RaycastHit impacto in impactos)
        {
            Collider colision = impacto.collider;

            // Ignorar los colliders del propio jugador
            if (colision.transform == transform ||
                colision.transform.IsChildOf(transform))
            {
                continue;
            }

            return colision;
        }

        return null;
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
