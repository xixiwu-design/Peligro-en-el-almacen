
using UnityEngine;
using UnityEngine.InputSystem;

public class Jugador1Controller : MonoBehaviour
{
    private JugadorColisionController movimiento;

    void Start()
    {
        movimiento = GetComponent<JugadorColisionController>();
    }

    void Update()
    {
        if (Keyboard.current == null)
        {
            movimiento.EstablecerDireccion(Vector3.zero);
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

        Vector3 direccion = new Vector3(
            movimentoHorizontal,
            0f,
            movimentoVertical
        ).normalized;

        movimiento.EstablecerDireccion(direccion);
    }
}
