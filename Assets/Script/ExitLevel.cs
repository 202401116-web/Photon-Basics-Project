using Photon.Pun;
using UnityEngine;


public class ExitLevel : MonoBehaviourPun
{
    public string sceneToLoad;
    private int playersInDoor = 0;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PhotonView pv = collision.GetComponent<PhotonView>();
        if (collision.gameObject.layer == LayerMask.NameToLayer("player") && pv != null && pv.IsMine)
        {
            playersInDoor++;
            CheckWinCondition();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        PhotonView pv = collision.GetComponent<PhotonView>();
        if (collision.gameObject.layer == LayerMask.NameToLayer("player") && pv != null && pv.IsMine)
        {
            playersInDoor--;
        }
    }

    private void CheckWinCondition()
    {
        if (PhotonNetwork.IsMasterClient && playersInDoor >= 2)
        {
            Debug.Log("Master Client is loading the next room...");
            PhotonNetwork.LoadLevel(sceneToLoad);
        }
    }
}
