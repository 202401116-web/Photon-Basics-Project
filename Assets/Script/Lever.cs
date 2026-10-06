using Photon.Pun;
using UnityEngine;
using Photon.Realtime;

[RequireComponent(typeof(Rigidbody2D), typeof(PhotonView))]
public class Lever : MonoBehaviourPun
{
    [SerializeField] private GameObject gate;
    [SerializeField] private SpriteRenderer gateRenderer;
    [SerializeField] private BoxCollider2D gateCollider;
    bool isOpen;


    void Start()
    {
        Transform gateTransform = transform.parent.Find("gate");

        if (gateTransform != null)
        {
            gate = gateTransform.gameObject;
        }
    }

    [PunRPC]
    public void Switched(bool state)
    {
        if (gate != null)
        {
            gate.SetActive(state);
        }
        else
        {
            Debug.LogWarning("PathBlocker is not assigned in NetworkSwitchTrigger!");
        }
    }
}
