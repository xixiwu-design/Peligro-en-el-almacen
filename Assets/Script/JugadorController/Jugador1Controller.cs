
using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-100)]
public class Jugador1Controller : MonoBehaviour
{
    [Header("Configuración del jugador")]
    [SerializeField] private float velocidad = 40f;

    [Header("Componentes")]
    [SerializeField] private Rigidbody rb;

    private JugadorColisionController colision;
    private Vector3 direccionMovimiento;

    void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        colision = GetComponent<JugadorColisionController>();
    }

    void Update()
    {
        if (Keyboard.current == null)
        {
            direccionMovimiento = Vector3.zero;
            return;
        }

        // Movimiento Horizontal A D (X)
        float movimentoHorizontal =
            Keyboard.current.aKey.isPressed ? -1f :
            Keyboard.current.dKey.isPressed ? 1f :
            0f;

        // Movimiento Vertical W S (Z)
        float movimentoVertical =
            Keyboard.current.wKey.isPressed ? 1f :
            Keyboard.current.sKey.isPressed ? -1f :
            0f;

        direccionMovimiento = new Vector3(
            movimentoHorizontal,
            0f,
            movimentoVertical
        ).normalized;
    }

    void FixedUpdate()
    {
        colision.PrepararFrame();

        if (direccionMovimiento == Vector3.zero)
            return;

        float distancia = velocidad * Time.fixedDeltaTime;

        bool puedeMoverX = true;
        bool puedeMoverZ = true;

        // Comprobar el eje X
        if (Mathf.Abs(direccionMovimiento.x) > 0.01f)
        {
            Vector3 direccionX = new Vector3(
                Mathf.Sign(direccionMovimiento.x),
                0f,
                0f
            );

            puedeMoverX = colision.PuedeMoverEnDireccion(
                direccionX,
                distancia
            );
        }

        // Comprobar el eje Z
        if (Mathf.Abs(direccionMovimiento.z) > 0.01f)
        {
            Vector3 direccionZ = new Vector3(
                0f,
                0f,
                Mathf.Sign(direccionMovimiento.z)
            );

            puedeMoverZ = colision.PuedeMoverEnDireccion(
                direccionZ,
                distancia
            );
        }

        // Calcular la velocidad de cada eje
        float velocidadX = 0f;
        float velocidadZ = 0f;

        bool soloHorizontal = Mathf.Abs(direccionMovimiento.x) > 0.01f &&
                      Mathf.Abs(direccionMovimiento.z) < 0.01f;

        bool soloVertical = Mathf.Abs(direccionMovimiento.z) > 0.01f &&
                            Mathf.Abs(direccionMovimiento.x) < 0.01f;

        if (soloHorizontal)
        {
            if (puedeMoverX)
                velocidadX = direccionMovimiento.x;
        }
        else if (soloVertical)
        {
            if (puedeMoverZ)
                velocidadZ = direccionMovimiento.z;
        }
        else
        {
            if (puedeMoverX)
                velocidadX = direccionMovimiento.x;

            if (puedeMoverZ)
                velocidadZ = direccionMovimiento.z;
        }

        Vector3 desplazamiento = new Vector3(
            velocidadX,
            0f,
            velocidadZ
        ) * distancia;

        // Girar según la dirección original de entrada
        Quaternion rotacion =
            Quaternion.LookRotation(direccionMovimiento) *
            Quaternion.Euler(0f, -90f, 0f);

        rb.MoveRotation(rotacion);

        // Aplicar el movimiento
        if (desplazamiento != Vector3.zero)
        {
            rb.MovePosition(rb.position + desplazamiento);
        }
    }
}
