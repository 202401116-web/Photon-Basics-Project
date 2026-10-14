using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Unity.VisualScripting;

[RequireComponent(typeof(Rigidbody2D), typeof(PhotonView))]
public class InteractableObject : MonoBehaviourPun, IPunObservable
{
    private Rigidbody2D rb;
    private PhotonView ownerView;
    [SerializeField] public bool isObject;
    private BoxCollider2D collisioner;

    [Header("Network sync variables")]
    private Vector3 networkPosition;
    private Quaternion networkRotation;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        collisioner = GetComponent<BoxCollider2D>();
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
            rb.bodyType = RigidbodyType2D.Kinematic; //disables physics while grabbed
            collisioner.isTrigger = true;
            Debug.Log("Grabbed");
        }

    }

    [PunRPC]
    public void RPC_Release()
    {
        ownerView = null;
        rb.bodyType = RigidbodyType2D.Dynamic;
        collisioner.isTrigger = false;
        Debug.Log("Dropped");
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

    private void OnCollisionStay2D(Collision2D collision)
    {
        // 1. Dynamic Actor Check (Safe replacement for your ActorNumber check)
        if (PhotonNetwork.LocalPlayer.ActorNumber != 2) return;

        Rigidbody2D targetRigidbody = collision.collider.attachedRigidbody;
        PhotonView targetPhotonView = collision.collider.GetComponent<PhotonView>();

        if (targetRigidbody == null || targetRigidbody.bodyType == RigidbodyType2D.Kinematic) return;

        if (targetPhotonView != null)
        {
            // 2. If we don't own it yet, request ownership and EXIT early.
            // Do NOT apply force this frame because our physics updates will be rejected by the network.
            if (!targetPhotonView.IsMine)
            {
                targetPhotonView.RequestOwnership();
                return;
            }
        }

        // 3. If we safely reached this point, we are officially the owner on the network!
        // Calculate the push direction using Vector2 for accurate 2D physics.
        Vector2 pushDirection = (Vector2)collision.collider.transform.position - (Vector2)transform.position;
        pushDirection.y = 0; // Keep the push strictly horizontal
        pushDirection.Normalize();

        // 4. Apply continuous force safely
        targetRigidbody.AddForce(pushDirection * 2, ForceMode2D.Force);
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