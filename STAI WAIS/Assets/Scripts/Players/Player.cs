using UnityEngine;

public class Player : MonoBehaviour
{
    //Para saber si toca turno
    private bool myTurn = false;
    public Faction faction;

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
        // Cargar los recursos actuales del GameManager.
        SincronizarDesdeGameManager();
        RecolectResources();
    }

    public void RecolectResources(){
        //Esta funcion es igual para los dos por lo tanto se pone aqui
        //Recorre todos los puestos de recursos del jugador y le genera los recursos necesarios
        Structure[] estructuras = FindObjectsByType<Structure>(FindObjectsSortMode.None);
        foreach (Structure estructura in estructuras) 
        {
            if (estructura == null) continue;
            // PRIMERO: Actualizar quién controla la estructura.
            estructura.ActualizarControl();
            // SEGUNDO: Comprobar si pertenece a este jugador.
            if (estructura.ControllingPLayer != this) continue;

            int cantidad = estructura.recursosPorTurno;
            //Añadir el recurso corresponidente
            switch (estructura.ResourceProduced)
            {
                case ResourceType.Chatarra:
                    chatarra += cantidad;
                    break;

                case ResourceType.Creditos:
                    creditos += cantidad;
                    break;

                case ResourceType.Tritio:
                    tritio += cantidad;
                    break;

                case ResourceType.CristalKyber:
                    cristal += cantidad;
                    break;
            }
            Debug.Log($"{name} obtiene {cantidad} de " +$"{estructura.ResourceProduced} de una estructura.");
        }
        // Guardar los recursos actualizados.
        SincronizarHaciaGameManager();
        Debug.Log(
            $"RECURSOS {faction}: " +
            $"Chatarra={chatarra}, " +
            $"Créditos={creditos}, " +
            $"Tritio={tritio}, " +
            $"Cristal={cristal}"
        );
    }
    private void SincronizarDesdeGameManager() 
    {
        if (gameManager == null) return;
        if (this is HumanPlayer) 
        {
            chatarra = gameManager.j_chatarra;
            creditos = gameManager.j_creditos;
            tritio = gameManager.j_tritio;
            cristal = gameManager.j_cristal;
        }
        else if (this is AIPlayer)
        {
            chatarra = gameManager.ia_chatarra;
            creditos = gameManager.ia_creditos;
            tritio = gameManager.ia_tritio;
            cristal = gameManager.ia_cristal;
        }
    }
    private void SincronizarHaciaGameManager() 
    {
        if (gameManager == null) return;

        if (this is HumanPlayer)
        {
            gameManager.j_chatarra = chatarra;
            gameManager.j_creditos = creditos;
            gameManager.j_tritio = tritio;
            gameManager.j_cristal = cristal;
        }
        else if (this is AIPlayer)
        {
            gameManager.ia_chatarra = chatarra;
            gameManager.ia_creditos = creditos;
            gameManager.ia_tritio = tritio;
            gameManager.ia_cristal = cristal;
        }
    }

    public void PlayerTurnEnd(){
        myTurn = false;
        if(otherPlayer != null)
            otherPlayer.PlayerTurnStart();
    }
}
