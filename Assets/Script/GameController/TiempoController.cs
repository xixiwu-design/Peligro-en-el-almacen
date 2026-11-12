
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class TestTiempo : MonoBehaviour
{
    public TMP_Text textoTiempo;
    private float tiempo = 60f;

    void Update()
    {
        if (tiempo > 0)
        {
            tiempo -= Time.deltaTime;

            if (tiempo < 0)
            {
                tiempo = 0;
            }
        }

        int minutos = Mathf.FloorToInt(tiempo / 60f);
        int segundos = Mathf.FloorToInt(tiempo % 60f);

        textoTiempo.text = minutos.ToString("00") + ":" + segundos.ToString("00");

        if (tiempo <= 0)
        {
            SceneManager.LoadScene("Fallado");
        }
    }
}
//tiene un minuto 




/*
float tiempo = 0f; crea una variable que guarda los segundos.
Start() pone el cronómetro a cero cuando comienza.
Update() se ejecuta continuamente mientras el juego está funcionando.
Time.deltaTime suma el tiempo transcurrido entre fotogramas.
ToString("F2") muestra el tiempo con dos decimales, por ejemplo, 5.27 segundos.
Debug.Log() muestra el resultado en la consola de Unity.
*/