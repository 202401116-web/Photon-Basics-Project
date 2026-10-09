using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
public class Player2DController : MonoBehaviourPun, IPunObservable
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 7f;
    public SpriteRenderer sprite;

    private Rigidbody2D rb;
    public bool isGrounded = false;
    [SerializeField] Animator animator;

    [Header("Interaction Settings")]
    public Transform interactPoint; //this is referenced to an empty child in front of the player
    public float interactRange = 1f;
    private InteractableObject grabbedObject = null;
    [SerializeField]private LayerMask interactLayer;

    [Header("Network sync variables")]
    private Vector3 networkPosition;
    private Quaternion networkRotation;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        networkPosition = transform.position;
        networkRotation = transform.rotation;
        interactLayer = 1 << LayerMask.NameToLayer("Interactables");
        animator = GetComponent<Animator>();

        if(PhotonNetwork.LocalPlayer.ActorNumber == 1 && photonView.IsMine)
        {
            RuntimeAnimatorController controller = Resources.Load<RuntimeAnimatorController>("Animations/Etsy_Anim");
            if (controller != null)
            {
                Debug.Log("Got Animations!");
                animator.runtimeAnimatorController = controller;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine) //for the local player
        {

            if (GameManager.allowMovement)
            {
                HandleMovement();
                HandleInteraction();
            }
            else
            {
                rb.linearVelocity = Vector2.zero;
            }
        }
        else //for the remote players
        {
            transform.position = Vector3.Lerp(transform.position, networkPosition, Time.deltaTime * 10f);
            transform.rotation = Quaternion.Lerp(transform.rotation, networkRotation, Time.deltaTime * 10f);
        }
    }

    void HandleMovement()
    {
        float move = Input.GetAxis("Horizontal");

        // Move left/right
        rb.linearVelocity = new Vector2(move * moveSpeed, rb.linearVelocity.y);

        // Flip character when moving left/right
        if (move > 0.1f)
            transform.rotation = Quaternion.Euler(0, 0, 0);
        else if (move < -0.1f)
            transform.rotation = Quaternion.Euler(0, 180, 0);

        // Jump
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            isGrounded = false;
        }

        if(move != 0)
        {
            animator.SetBool("isRunning", true);
        }
        else
        {
            animator.SetBool("isRunning", false);
        }
    }

    void HandleInteraction()
    {
        Collider2D hit = Physics2D.OverlapCircle(interactPoint.position, interactRange, interactLayer);
        if (hit != null)
        {
            if (Input.GetKeyDown(KeyCode.E)) //this is the key for grab or release
            {
                if (grabbedObject == null)
                {
                    if (hit != null && hit.TryGetComponent<InteractableObject>(out var interacterer))
                    {
                        grabbedObject = interacterer;
                        grabbedObject.photonView.RPC("RPC_SetGrabbed", RpcTarget.AllBuffered, photonView.ViewID);
                    }
                }
                else
                {
                    grabbedObject.photonView.RPC("RPC_Release", RpcTarget.AllBuffered);
                    grabbedObject = null;
                }
                if (hit.TryGetComponent<Lever>(out var lever))
                {
                    Debug.Log("Switcheroo");
                    lever.photonView.RPC("Switched", RpcTarget.AllBuffered, false);
                }
            }
        }

        if (grabbedObject != null)
        {
            grabbedObject.transform.position = Vector3.Lerp(
                grabbedObject.transform.position,
                interactPoint.position,
                Time.deltaTime * 10f);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //simple ground check
        if (collision.contacts[0].normal.y > 0.5f)
        {
            isGrounded = true;
        }
    }

    void OnDrawGizmos()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(interactPoint.position, interactRange);
    }

    // 🔹 Photon built-in sync method
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting) // Local player → send data
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else // Remote player → receive data
        {
            networkPosition = (Vector3)stream.ReceiveNext();
            networkRotation = (Quaternion)stream.ReceiveNext();
        }
    }
}