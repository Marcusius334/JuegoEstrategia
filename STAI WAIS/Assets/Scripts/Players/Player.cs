using UnityEngine;

public class Player : MonoBehaviour
{
    //Para saber si toca turno
    private bool myTurn = false;

    [Header("PlayerTeam")]
    public int team = 0;
    public Player otherPlayer;

    [Header("Resources")]
    public int chatarra = 400;
    public int creditos = 200;
    public int tritio = 20;
    public int cristal = 0;

    [Header("Managers")]
    public GameManager gameManager;

    public void PlayerTurnStart(){
        myTurn = true;
        RecolectResources();
    }

    public void RecolectResources(){
        //Esta funcion es igual para los dos por lo tanto se pone aqui
        //Recorre todos los puestos de recursos del jugador y le genera los recursos necesarios
    }

    public void PlayerTurnEnd(){
        myTurn = false;
        otherPlayer.PlayerTurnStart();
    }
}
