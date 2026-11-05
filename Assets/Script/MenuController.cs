using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
     public static void Jugador1Historia()
    {
        SceneManager.LoadScene("HistoriajugadorUno");
    }
    public static void Jugador2Historia()
    {
        SceneManager.LoadScene("HistoriajugadorDos");
    }

public static void Menu()
    {
        SceneManager.LoadScene("Menu");
    }
  public static void Jugador1()
    {
        SceneManager.LoadScene("1Jugador");
    }
    public static void Jugador2()
    {
        SceneManager.LoadScene("2Jugadores");
    }

    public static void SalirAplicacion()
    {
        Application.Quit();//funciona despues del build

        //demo
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;//在编辑器中停止运行
        #endif
    }
}
