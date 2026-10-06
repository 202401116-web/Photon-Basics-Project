using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkSwitchTrigger : MonoBehaviourPun
{
    [Header("Objects to control")]
    public GameObject pathBlocker;
    public bool isActivated = false;

    void OnTriggerEnter2D(Collider2D actor)
    {
        // Make sure only the local player activates the switch
        PhotonView actorPV = actor.GetComponent<PhotonView>();
        if ((actor.CompareTag("Player") || actor.CompareTag("Activator")) && actorPV != null && actorPV.IsMine && !isActivated)
        {
            photonView.RPC("RPC_ToggleBlocker", RpcTarget.AllBuffered, false);
            isActivated = true;
        }
    }

    void OnTriggerExit2D(Collider2D actor)
    {
        PhotonView actorPV = actor.GetComponent<PhotonView>();
        if (actor.CompareTag("Player") && actorPV != null && actorPV.IsMine)
        {
            photonView.RPC("RPC_ToggleBlocker", RpcTarget.AllBuffered, true);
            isActivated = false;
        }
    }

    [PunRPC]
    public void RPC_ToggleBlocker(bool state)
    {
        if (pathBlocker != null)
        {
            pathBlocker.SetActive(state);
        }
        else
        {
            Debug.LogWarning("PathBlocker is not assigned in NetworkSwitchTrigger!");
        }
    }

}
