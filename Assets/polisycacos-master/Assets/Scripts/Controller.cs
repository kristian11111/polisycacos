﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Controller : MonoBehaviour
{
    //GameObjects
    public GameObject board;
    public GameObject[] cops = new GameObject[2];
    public GameObject robber;
    public Text rounds;
    public Text finalMessage;
    public Button playAgainButton;

    //Otras variables
    Tile[] tiles = new Tile[Constants.NumTiles];
    private int roundCount = 0;
    private int state;
    private int clickedTile = -1;
    private int clickedCop = 0;
                    
    void Start()
    {        
        InitTiles();
        InitAdjacencyLists();
        state = Constants.Init;
        Debug.Log("controller reiniciado");
    }
        
    //Rellenamos el array de casillas y posicionamos las fichas
    void InitTiles()
    {
        for (int fil = 0; fil < Constants.TilesPerRow; fil++)
        {
            GameObject rowchild = board.transform.GetChild(fil).gameObject;            

            for (int col = 0; col < Constants.TilesPerRow; col++)
            {
                GameObject tilechild = rowchild.transform.GetChild(col).gameObject;                
                tiles[fil * Constants.TilesPerRow + col] = tilechild.GetComponent<Tile>();                         
            }
        }
                
        cops[0].GetComponent<CopMove>().currentTile=Constants.InitialCop0;
        cops[1].GetComponent<CopMove>().currentTile=Constants.InitialCop1;
        robber.GetComponent<RobberMove>().currentTile=Constants.InitialRobber;           
    }

    public void InitAdjacencyLists()
    {
        //Matriz de adyacencia
        int[,] matriu = new int[8, 8];

        

        // Inicializar matriz a 0's
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                matriu[i, j] = 0;
            }
        }

        for (int numTile = 0; numTile < Constants.NumTiles; numTile++)
        {
            int tileActual = 0;
            // Rellenar con 1's las casillas adyacentes y llenar la lista "adjacency"
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    
                    if (tileActual == numTile)
                    {
                        // Comprobar y añadir los índices de las casillas adyacentes si están dentro de los límites de la matriz
                        if (i - 1 >= 0)
                        {
                            matriu[i - 1, j] = 1; // Casilla arriba
                            tiles[numTile].adjacency.Add((i - 1) * 8 + j); // Índice de casilla arriba
                        }
                        if (i + 1 < 8)
                        {
                            matriu[i + 1, j] = 1; // Casilla abajo
                            tiles[numTile].adjacency.Add((i + 1) * 8 + j); // Índice de casilla abajo
                        }
                        if (j - 1 >= 0)
                        {
                            matriu[i, j - 1] = 1; // Casilla izquierda
                            tiles[numTile].adjacency.Add(i * 8 + (j - 1)); // Índice de casilla izquierda
                        }
                        if (j + 1 < 8)
                        {
                            matriu[i, j + 1] = 1; // Casilla derecha
                            tiles[numTile].adjacency.Add(i * 8 + (j + 1)); // Índice de casilla derecha
                        }

                        // 2 Comprobar y añadir los índices de las casillas adyacentes si están dentro de los límites de la matriz
                        if (i - 2 >= 0)
                        {
                            matriu[i - 2, j] = 1; // Casilla arriba
                            tiles[numTile].adjacency.Add((i - 2) * 8 + j); // Índice de casilla arriba
                        }
                        if (i + 2 < 8)
                        {
                            matriu[i + 2, j] = 1; // Casilla abajo
                            tiles[numTile].adjacency.Add((i + 2) * 8 + j); // Índice de casilla abajo
                        }
                        if (j - 2 >= 0)
                        {
                            matriu[i, j - 2] = 1; // Casilla izquierda
                            tiles[numTile].adjacency.Add(i * 8 + (j - 2)); // Índice de casilla izquierda
                        }
                        if (j + 2 < 8)
                        {
                            matriu[i, j + 2] = 1; // Casilla derecha
                            tiles[numTile].adjacency.Add(i * 8 + (j + 2)); // Índice de casilla derecha
                        }
                        // 3 Comprobar y añadir los índices de las casillas adyacentes si están dentro de los límites de la matriz
                        if (i - 1 >= 0 && j - 1 >= 0)
                        {
                            matriu[i - 1, j-1] = 1; // Casilla abajo - izquierda
                            tiles[numTile].adjacency.Add((i - 1) * 8 + j-1); // Índice de casilla arriba
                        }
                        if (i + 1 < 8 && j - 1 >= 0)
                        {
                            matriu[i + 1, j - 1] = 1; // Casilla arriba - izquierda
                            tiles[numTile].adjacency.Add((i + 1) * 8 + j-1); // Índice de casilla abajo
                        }
                        if (i - 1 >= 0 && j + 1 <8)
                        {
                            matriu[i-1, j +1] = 1; // Casilla arriba - derecha
                            tiles[numTile].adjacency.Add((i-1) * 8 + (j + 1)); // Índice de casilla izquierda
                        }
                        if (i + 1 < 8 && j + 1 < 8)
                        {
                            matriu[i+1, j + 1] = 1; // Casilla abajo - derecha
                            tiles[numTile].adjacency.Add((i+1) * 8 + (j + 1)); // Índice de casilla derecha
                        }
                    }
                    tileActual++;
                }
            }  
        }
        



        




    }

    //Reseteamos cada casilla: color, padre, distancia y visitada
    public void ResetTiles()
    {        
        foreach (Tile tile in tiles)
        {
            tile.Reset();
        }
    }

    public void ClickOnCop(int cop_id)
    {
        switch (state)
        {
            case Constants.Init:
            case Constants.CopSelected:                
                clickedCop = cop_id;
                clickedTile = cops[cop_id].GetComponent<CopMove>().currentTile;
                tiles[clickedTile].current = true;

                ResetTiles();
                FindSelectableTiles(true);

                state = Constants.CopSelected;                
                break;            
        }
    }

    public void ClickOnTile(int t)
    {                     
        clickedTile = t;

        switch (state)
        {            
            case Constants.CopSelected:
                //Si es una casilla roja, nos movemos
                if (tiles[clickedTile].selectable)
                {                  
                    cops[clickedCop].GetComponent<CopMove>().MoveToTile(tiles[clickedTile]);
                    cops[clickedCop].GetComponent<CopMove>().currentTile=tiles[clickedTile].numTile;
                    tiles[clickedTile].current = true;   
                    
                    state = Constants.TileSelected;
                }                
                break;
            case Constants.TileSelected:
                state = Constants.Init;
                break;
            case Constants.RobberTurn:
                state = Constants.Init;
                break;
        }
    }

    public void FinishTurn()
    {
        switch (state)
        {            
            case Constants.TileSelected:
                ResetTiles();

                state = Constants.RobberTurn;
                RobberTurn();
                break;
            case Constants.RobberTurn:                
                ResetTiles();
                IncreaseRoundCount();
                if (roundCount <= Constants.MaxRounds)
                    state = Constants.Init;
                else
                    EndGame(false);
                break;
        }

    }

    public void RobberTurn()
    {
        clickedTile = robber.GetComponent<RobberMove>().currentTile;
        tiles[clickedTile].current = true;
        FindSelectableTiles(false);

        /*TODO: Cambia el código de abajo para hacer lo siguiente
        - Elegimos una casilla aleatoria entre las seleccionables que puede ir el caco
        - Movemos al caco a esa casilla
        - Actualizamos la variable currentTile del caco a la nueva casilla
        */


        List<int> adyacenciaMejoradaRobber = new List<int>();

        // comprobar para cada tile de adyacencia del robber si coincide con un tile en la lista de adyacencia de algun cop
        for (int i = 0; i < tiles[robber.GetComponent<RobberMove>().currentTile].adjacency.Count; i++)
        {
            if (!tiles[cops[0].GetComponent<CopMove>().currentTile].adjacency.Contains(tiles[robber.GetComponent<RobberMove>().currentTile].adjacency[i])
                &&
                !tiles[cops[1].GetComponent<CopMove>().currentTile].adjacency.Contains(tiles[robber.GetComponent<RobberMove>().currentTile].adjacency[i]))
            {
                if (cops[0].GetComponent<CopMove>().currentTile != tiles[robber.GetComponent<RobberMove>().currentTile].adjacency[i]
                    &&
                    cops[1].GetComponent<CopMove>().currentTile != tiles[robber.GetComponent<RobberMove>().currentTile].adjacency[i])
                {
                    adyacenciaMejoradaRobber.Add(tiles[robber.GetComponent<RobberMove>().currentTile].adjacency[i]);
                }    
            }
        }
        if (adyacenciaMejoradaRobber.Count == 0)
        {
            adyacenciaMejoradaRobber.Add(tiles[robber.GetComponent<RobberMove>().currentTile].adjacency[0]);
        }
       

        int casillaALaQueSeMueve = Random.Range(0, adyacenciaMejoradaRobber.Count-1);
        robber.GetComponent<RobberMove>().MoveToTile(tiles[adyacenciaMejoradaRobber[casillaALaQueSeMueve]]);
        robber.GetComponent<RobberMove>().currentTile = adyacenciaMejoradaRobber[casillaALaQueSeMueve];
    }

    public void EndGame(bool end)
    {
        if(end)
            finalMessage.text = "You Win!";
        else
            finalMessage.text = "You Lose!";
        playAgainButton.interactable = true;
        state = Constants.End;
    }

    public void PlayAgain()
    {
        cops[0].GetComponent<CopMove>().Restart(tiles[Constants.InitialCop0]);
        cops[1].GetComponent<CopMove>().Restart(tiles[Constants.InitialCop1]);
        robber.GetComponent<RobberMove>().Restart(tiles[Constants.InitialRobber]);
                
        ResetTiles();

        playAgainButton.interactable = false;
        finalMessage.text = "";
        roundCount = 0;
        rounds.text = "Rounds: ";

        state = Constants.Restarting;
    }

    public void InitGame()
    {
        state = Constants.Init;
         
    }

    public void IncreaseRoundCount()
    {
        roundCount++;
        rounds.text = "Rounds: " + roundCount;
    }

    public void FindSelectableTiles(bool cop)
    {
                 
        int indexcurrentTile;        

        if (cop==true)
            indexcurrentTile = cops[clickedCop].GetComponent<CopMove>().currentTile;
        else
            indexcurrentTile = robber.GetComponent<RobberMove>().currentTile;

        //La ponemos rosa porque acabamos de hacer un reset
        tiles[indexcurrentTile].current = true;

        //Cola para el BFS
        Queue<Tile> nodes = new Queue<Tile>();

        //TODO: Implementar BFS. Los nodos seleccionables los ponemos como selectable=true
        //Tendrás que cambiar este código por el BFS
        

        for (int i = 0; i < tiles[indexcurrentTile].adjacency.Count; i++)
        {
            if (tiles[tiles[indexcurrentTile].adjacency[i]] != tiles[cops[0].GetComponent<CopMove>().currentTile] 
                && 
                tiles[tiles[indexcurrentTile].adjacency[i]] != tiles[cops[1].GetComponent<CopMove>().currentTile])
            {
                tiles[tiles[indexcurrentTile].adjacency[i]].selectable = true;
            }
        }



    }
    
   
    

    

   

       
}
