using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static bool allowMovement = false; //this identifies of playes can move

    [Header("Timer Settings")]
    public float levelDuration = 10f;
    private float currentTime;
    private bool timerRunning = false;

    [Header("UI")]
    public TMP_Text timerText; // drag a TextMeshPro text here
    public GameObject gameOverScreen; //this is referenced to the game over screen object

    void Awake()
    {
        // Make sure this applies before joining any room
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentTime = levelDuration;

        allowMovement = false; //locked by default
        CheckPlayers(); //this calls the checkplayes function
    }

    void Update()
    {
        if (timerRunning)
        {
            currentTime -= Time.deltaTime;
            if (currentTime <= 0)
            {
                currentTime = 0f;
                timerRunning = false;
                OnTimerEnd();
            }
            UpdateTimerUI(); //formats the timer display
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        CheckPlayers();
        TryStartTimer();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        CheckPlayers();
        if (PhotonNetwork.CurrentRoom.PlayerCount < 2) //change the value depending on the required player count
        {
            // Stop timer if less than 2 players
            timerRunning = false;
            Debug.Log("Timer stopped - waiting for 2 players again.");
        }
    }

    //this checks if the required no of players is met
    void CheckPlayers()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2) //change player count depending on the required no of players
        {
            allowMovement = true;
            Debug.Log("Two players in room → Movement enabled!");
        }
        else
        {
            allowMovement = false;
            Debug.Log("Waiting for 2 players → Movement disabled.");
        }
    }

    void TryStartTimer()
    {
        // Only master client decides when to start
        if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount == 2 && !timerRunning)
        {
            photonView.RPC("RPC_StartTimer", RpcTarget.AllBuffered, PhotonNetwork.Time);
        }
    }

    [PunRPC]
    void RPC_StartTimer(double startTime)
    {
        currentTime = levelDuration;
        timerRunning = true;
        Debug.Log("Timer started!");
    }

    //formats the timer display
    void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60);
            int seconds = Mathf.FloorToInt(currentTime % 60);
            timerText.text = $"{minutes:00}:{seconds:00}";
        }
    }

    //this identifies wat happens if the timer ended
    void OnTimerEnd()
    {
        Debug.Log("Timer ended!");
        // Add logic: end game, show results, load scene, etc.
        allowMovement = false;
        gameOverScreen.gameObject.SetActive(true);
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }
}