using Photon.Pun;
using UnityEngine;


public class ExitLevel : MonoBehaviourPun
{
    public string sceneToLoad;
    [SerializeField] private int playersInDoor = 0;
    public LayerMask playerLayers;

    private void Start()
    {
        playerLayers = LayerMask.GetMask("player", "playerNonPush");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PhotonView pv = collision.GetComponent<PhotonView>();
        if (collision.gameObject.layer == playerLayers && pv != null)
        {
            playersInDoor++;
            CheckWinCondition();
        }
    }




    private void OnTriggerExit2D(Collider2D collision)
    {
        PhotonView pv = collision.GetComponent<PhotonView>();
        if (collision.gameObject.layer == playerLayers && pv != null)
        {
            playersInDoor--;
        }
    }

    private void CheckWinCondition()
    {
        if (PhotonNetwork.IsMasterClient && playersInDoor >= 2 && sceneToLoad != "exitGame")
        {
            Debug.Log("Master Client is loading the next room...");
            PhotonNetwork.LoadLevel(sceneToLoad);
        }
        else if(PhotonNetwork.IsMasterClient && playersInDoor >= 2 && sceneToLoad == "exitGame")
        {
            Application.Quit();
        }
    }
}
