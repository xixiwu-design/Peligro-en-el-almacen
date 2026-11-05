using UnityEngine;
using UnityEngine.InputSystem;


public class Jugador1Controller : MonoBehaviour
{
    float vel;

    void Start()
    {
        vel = 40f;

    }

    void Update()
    {
        MovimentoJugador1();
        //ControlarLimitesPantalla();
    }

    /* aun no tiene 
    void ControlarLimitesPantalla()
    {
        Vector3 posActual = transform.position;

        //Mathf.Clamp
        posActual.x = Mathf.Clamp(
        posActual.x,
        ValorsGlobales.limiteIzquierdaX,
        ValorsGlobales.limiteDerechaX
        );

        posActual.y = Mathf.Clamp(
            posActual.y,
            ValorsGlobales.limiteAbajoY,
            ValorsGlobales.limiteArribaY
        );

        transform.position = posActual;
    }
    */


    void MovimentoJugador1(){
    // Movimiento Horizontal A D (X)
    float movimentoHorizontal =
        Keyboard.current.aKey.isPressed ? -2f :
        Keyboard.current.dKey.isPressed ? 2f :
        0f;

    // Movimiento Vertical W S (Z)
    float movimentoVertical =
        Keyboard.current.wKey.isPressed ? 2f :
        Keyboard.current.sKey.isPressed ? -2f :
        0f;

    // Vector3: X, Y, Z
    Vector3 VectorDesplaczmento = new Vector3(
        movimentoHorizontal,
        0f,
        movimentoVertical
    );

    VectorDesplaczmento = VectorDesplaczmento.normalized;

    // Girar el modelo según la dirección del movimiento
    if (VectorDesplaczmento != Vector3.zero)
    {
        transform.rotation = Quaternion.LookRotation(VectorDesplaczmento) * Quaternion.Euler(0f, -90f, 0f);    }

    Vector3 nuevaDesplazamiento = new Vector3(
        vel * VectorDesplaczmento.x * Time.deltaTime,
        0f,
        vel * VectorDesplaczmento.z * Time.deltaTime
    );

    transform.position += nuevaDesplazamiento;
}

}
