using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    static GameManager current;

    public enum Difficulty {Easy = 1, Medium = 2, Hard = 3 }
    public enum Missions { Home = 0, Mission1, Mission2, Mission3, Mission3Boss }

    float totalGameTime;                        //Length of the total game time
    bool isGameOver;                            //Is the game currently over?
    int score = 0;
    int initialBombs = 10;
    int bombs;
    int heavyMachineAmmo = 0;
    Difficulty difficulty = Difficulty.Medium;
    float bgmAudio = 1f;
    float sfxAudio = 1f;
    Missions currentMission = Missions.Home;
    float mission1Points = 0f;
    float mission2Points = 0f;
    float mission3Points = 0f;

    [Header("Layers")]
    public LayerMask enemyLayer;
    public LayerMask buildingLayer;
    public LayerMask walkableLayer;
    public LayerMask playerLayer;

    void Awake()
    {
        //If a Game Manager exists and this isn't it...
        if (current != null && current != this)
        {
            //...destroy this and exit. There can only be one Game Manager
            Destroy(gameObject);
            return;
        }

        //Set this as the current game manager
        current = this;

        //Persist this object between scene reloads
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {

        GameReset();
    }

    void Update()
    {
        //If the game is over, exit
        if (isGameOver)
            return;
        //Update the total game time and tell the UI Manager to update
        totalGameTime += Time.deltaTime;
        // UIManager.UpdateTimeUI(totalGameTime); // todo implement or delete
    }

  

    public static bool ToggleGodMode()
    {
        var player = GetPlayer();
        if (player)
        {
            var health = player.GetComponent<Health>();
            health.immortal = !health.immortal;
            if (health.immortal)
            {
                SetBombs(200);
                return true;
            }
            else
            {
                SetBombs();
                return false;
            }
        }
        return false;
    }

    public static void AddScore(float amount)
    {
        //If there is no current Game Manager, exit
        if (current == null)
            return;

        current.score += (int)amount;
    }

    public static int GetScore()
    {
        //If there is no current Game Manager, return 0
        if (current == null)
            return 0;

        //Return the state of the game
        return current.score;
    }

    public static int GetBombs()
    {
        //If there is no current Game Manager, return 0
        if (current == null)
            return 10;

        //Return the state of the game
        return current.bombs;
    }

    public static void RemoveBomb()
    {
        //If there is no current Game Manager, exit
        if (current == null)
            return;

        current.bombs--;
    }

    public static void AddAmmo()
    {
        //If there is no current Game Manager, exit
        if (current == null)
            return;

        current.bombs += 10;
        //current.heavyMachineAmmo += 120;

    }

    public static void SetBombs(int bombs = 10)
    {
        if (current == null)
            return;
        current.bombs = bombs;
        /*UIManager.UpdateBombsUI();*/
    }

    public static bool IsGameOver()
    {
        //If there is no current Game Manager, return false
        if (current == null)
            return false;

        //Return the state of the game
        return current.isGameOver;
    }

    public static void PlayerDied()
    {
        //If there is no current Game Manager, exit
        if (current == null)
            return;

        //The game is now over
        current.isGameOver = true;
    }


    public static LayerMask GetBuildingLayer()
    {
        //If there is no current Game Manager, exit
        if (current == null)
            return 0;

        return current.buildingLayer;
    }

    public static GameObject GetPlayer() 
    {
        if (current == null)
            return null;
        return GameObject.FindGameObjectWithTag("Player");
    }

    public static GameObject GetPlayer(GameObject player)
    {
        if (current == null)
            return null;
        if (player.GetComponent<PlayerController>()) // return itself
            return player;
        else if (player.transform.parent.gameObject.GetComponent<PlayerController>()) // return parent
            return player.transform.parent.gameObject;
        return GameObject.FindGameObjectWithTag("Player"); // return uncached finded by tag
    }

    public static GameObject GetPlayer(Collider2D collider)
    {
        return GetPlayer(collider.gameObject);
    }

    public static GameObject GetPlayer(Collision2D collision)
    {
        return GetPlayer(collision.collider);
    }

    public static bool IsPlayer(GameObject player)
    {
        //return (GetPlayerLayer() & (1<<player.layer)) != 0;
        return GetPlayerLayer() == (1<<player.layer);
    }

    public static bool IsPlayer(Collider2D collider)
    {
        return IsPlayer(collider.gameObject);
    }

    public static bool IsPlayer(Collision2D collision)
    {
        return IsPlayer(collision.collider);
    }

    public static LayerMask GetEnemyLayer()
    {
        if (current == null)
            return LayerMask.NameToLayer("Enemy");
        return current.enemyLayer;
    }

    public static LayerMask GetWalkableLayer()
    {
        if (current == null)
            return LayerMask.NameToLayer("Walkable");
        return current.walkableLayer;
    }

    public static int GetPlayerLayer()
    {
        if (current == null)
            return LayerMask.NameToLayer("Player");
        return current.playerLayer.value;
    }

    public static bool CanTriggerThrowable(Collider2D collider)
    {
        if (current == null)
            return false;
        var tag = collider.tag;
        return tag == "Enemy" || tag == "Building" || tag == "Walkable" || IsPlayer(collider) || tag == "Roof" || tag == "Bridge" || tag == "EnemyBomb";
    }

    public void SetDifficultyMode(int difficulty)
    {
        SetDifficultyMode((Difficulty)difficulty);
    }

    public static void SetDifficultyMode(Difficulty difficulty)
    {
        if (current == null)
            return;
        current.difficulty = difficulty;
    }

    public static Difficulty GetDifficultyMode()
    {
        if (current == null)
            return 0;
        return current.difficulty;
    }



    public static float GetBgmAudio()
    {
        if (current == null)
            return 0f;
        return current.bgmAudio;
    }


    public static float GetSfxAudio()
    {
        if (current == null)
            return 0f;
        return current.sfxAudio;
    }

    public static void GameReset()
    {
        if (!current)
            return;
        // reset values
        Time.timeScale = 1;
        current.isGameOver = false;
        current.score = 0;
        current.totalGameTime = 0;
        current.bombs = current.initialBombs;
        
    }

    public static bool CanTriggerEnemyBombs(string tag)
    {
        //If there is no current Game Manager, exit
        if (current == null)
            return false;

        return tag == "Player" || tag == "Walkable" || tag == "Marco Boat" || tag == "Bridge";
    }

}
