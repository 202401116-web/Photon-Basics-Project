using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

[RequireComponent(typeof(Rigidbody2D), typeof(PhotonView))]
public class InteractableObject : MonoBehaviourPun, IPunObservable
{
    private Rigidbody2D rb;
    private PhotonView ownerView;

    [Header("Network sync variables")]
    private Vector3 networkPosition;
    private Quaternion networkRotation;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        networkPosition = transform.position;
        networkRotation = transform.rotation;
    }

    [PunRPC]
    public void RPC_SetGrabbed(int playeViewID)
    {
        PhotonView playerView = PhotonView.Find(playeViewID);
        if (playerView != null)
        {
            ownerView = playerView;
            rb.isKinematic = true; //disables physics while grabbed
        }

    }

    [PunRPC]
    public void RPC_Release()
    {
        ownerView = null;
        rb.isKinematic = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (ownerView == null)
        {
            if (!photonView.IsMine)
            {
                transform.position = Vector3.Lerp(transform.position, networkPosition, Time.deltaTime * 10f);
                transform.rotation = Quaternion.Lerp(transform.rotation, networkRotation, Time.deltaTime * 10f);
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            networkPosition = (Vector3)stream.ReceiveNext();
            networkRotation = (Quaternion)stream.ReceiveNext();
        }
    }
}