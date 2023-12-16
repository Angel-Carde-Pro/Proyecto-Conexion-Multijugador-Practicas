using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchPlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    public float speed = 12f;

    private Vector3 velocity;
    public float gravity = -9.81f;

    public bool isGrounded;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    public float jumpHeight = 2f;

    public Transform playerCamera;

    //Van a guardar indices si es -1 es por que no esta asignado
    private int leftFingerID, rightFingerID;
    //Para calcular unicamente la mitad de la pantalla
    private float halfScreen;

    private Vector2 moveInput;
    //Se inicializa en la posicion en la que se haya hecho el toque en la pantalla
    private Vector2 moveTouchStartPosition;

    private Vector2 lookInput;
    [SerializeField] private float cameraSensibility;
    //Para limitar el rango de la camara
    private float cameraPitch;
    public PhotonView photonView;

    private void Start()
    {
        leftFingerID = -1;
        rightFingerID = -1;
        halfScreen = Screen.width / 2f;
    }


    void Update()
    {
        if (PhotonNetwork.InRoom && !photonView.IsMine)
        {
            return;
        }

        GetTouchInput();

        if (leftFingerID != -1)
        {
            Move();
        }

        if (rightFingerID != -1)
        {
            LookAround();
        }
    }

    private void GetTouchInput()
    {
        /*Input.touchCount: Para detectar si algun dedo esta tocando
            Debug.Log($"Ahora {Input.touchCount} dedos estan tocando la pantalla"); Para debuguear cuantos dedos estan tocando la pantalla.
        */
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch t = Input.GetTouch(i);
            //phase sirve para saber en q fase esta por ejmplo mantiene pulsado y o lo mueve
            //El toque empieza
            if (t.phase == TouchPhase.Began)
            {
                //Si es menor es por que el toque se lo hizo en la parte izquierda de la pantalla
                if (t.position.x < halfScreen && leftFingerID == -1)
                {
                    leftFingerID = t.fingerId;
                    moveTouchStartPosition = t.position;
                }
                else if (t.position.x > halfScreen && rightFingerID == -1)
                {
                    rightFingerID = t.fingerId;
                }
            }
            if (t.phase == TouchPhase.Canceled)
            {

            }
            //Cuando comienze a moverse
            if (t.phase == TouchPhase.Moved)
            {
                if (leftFingerID == t.fingerId)
                {
                    moveInput = t.position - moveTouchStartPosition;
                }
                else if (rightFingerID == t.fingerId)
                {
                    lookInput = t.deltaPosition * cameraSensibility * Time.deltaTime;
                }

            }

            //Mantiene pulsando
            if (t.phase == TouchPhase.Stationary)
            {
                if (rightFingerID == t.fingerId)
                {
                    lookInput = Vector2.zero;
                }
            }
            //Finaliza
            if (t.phase == TouchPhase.Ended)
            {
                if (leftFingerID == t.fingerId)
                {
                    leftFingerID = -1;
                }
                else if (rightFingerID == t.fingerId)
                {
                    rightFingerID = -1;
                }
            }
        }
    }

    private void Move()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2;
        }

        Vector3 move = transform.right * moveInput.normalized.x + transform.forward * moveInput.normalized.y;

        controller.Move(move * speed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2 * gravity);
        }
    }

    //Limitarla rotacion de la camera
    private void LookAround()
    {
        cameraPitch -= lookInput.y;
        //Con clamp limitamos
        cameraPitch = Mathf.Clamp(cameraPitch, -90, 90);
        //Para indicarle la rotacion sobre cada uno de los ejes (y y z no se van a mover por q eso se encarga el movimiento izquierdo)
        playerCamera.localRotation = Quaternion.Euler(cameraPitch, 0, 0);

        //Va a rotar sobre el eje verticar
        transform.Rotate(Vector3.up, lookInput.x);
    }
}
