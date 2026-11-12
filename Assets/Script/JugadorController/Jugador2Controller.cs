
using UnityEngine;
using UnityEngine.InputSystem;

public class Jugador2Controller : MonoBehaviour
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

        // Movimiento Horizontal (X)
        float movimentoHorizontal =
            Keyboard.current.leftArrowKey.isPressed ? -1f :
            Keyboard.current.rightArrowKey.isPressed ? 1f :
            0f;

        // Movimiento Vertical (Z)
        float movimentoVertical =
            Keyboard.current.upArrowKey.isPressed ? 1f :
            Keyboard.current.downArrowKey.isPressed ? -1f :
            0f;

        Vector3 direccion = new Vector3(
            movimentoHorizontal,
            0f,
            movimentoVertical
        ).normalized;

        movimiento.EstablecerDireccion(direccion);
    }
}
