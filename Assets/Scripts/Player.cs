using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SocialPlatforms.Impl;

public class Player : MonoBehaviour
{
    public bool ai;

    [HideInInspector] public int score = 0;
    [SerializeField] float turnSpeed, moveSpeed, power;
    [SerializeField] Transform mesh;

    List<Transform> players;
    PlayerInput playerInput;
    CapsuleCollider coll;
    Rigidbody rb;
    Animator animator;
    GameObject lastTouch;
    Transform locationTraveled;

    Vector2 input;
    Vector3 looked2Point; 

    private void Start()
    {
        if (ai)
        {
            players = GameManager.instance.playerList;
            locationTraveled = GetRandomPlayerPosition();
        }
        else
        {
            playerInput = GetComponent<PlayerInput>();
        }
        coll = GetComponent<CapsuleCollider>();
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }
    private void Update()
    {
        Movement();
    }
    /// <summary>
    /// yeni girdi sistemi kullanarak aldýðým girdilere göre bir yöne akýcý bir þekilde bakmasýný saðladýktan sonra o yöne doðru ilerlemesini saðlýyorum
    /// </summary>
    private void Movement()
    {
        if (ai)
        {
            if (Vector3.Distance(locationTraveled.position, transform.position) < 1)
            {
                locationTraveled = GetRandomPlayerPosition();
            }
            looked2Point = locationTraveled.position - transform.position;
            looked2Point.y=transform.position.y;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(looked2Point), turnSpeed * Time.deltaTime);
            rb.MovePosition(moveSpeed * Time.deltaTime * transform.forward + transform.position);
            animator.SetFloat("IdleWalkRun", moveSpeed);
        }
        else
        {
            input = playerInput.actions["Move"].ReadValue<Vector2>();
            if (input.x > 0.02f || input.y > 0.02f || input.y < -0.02f || input.y < -0.02f)
            {
                looked2Point.x = transform.position.x + input.x * 100; looked2Point.z = transform.position.z + input.y * 100;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(looked2Point), turnSpeed * Time.deltaTime);
                rb.MovePosition(Mathf.Sqrt(Mathf.Pow(input.x, 2) + Mathf.Pow(input.y, 2)) * moveSpeed * Time.deltaTime * transform.forward + transform.position);
                //rb.AddForce(Mathf.Sqrt(Mathf.Pow(input.x, 2) + Mathf.Pow(input.y, 2)) * moveSpeed * Time.deltaTime * transform.forward, ForceMode.VelocityChange);
            }
            animator.SetFloat("IdleWalkRun", moveSpeed * Mathf.Sqrt(Mathf.Pow(input.x, 2) + Mathf.Pow(input.y, 2)));
        }
    }
    /// <summary>
    /// oyuncu listesini günceller
    /// </summary>
    public void PlayerListUpdate()
    {
        bool isThere = false;
        players = GameManager.instance.playerList;
        for (int i = 0; i < players.Count; i++)
        {
            Transform player = players[i];
            if (player == locationTraveled)
            {
                isThere = true;
                break;
            }
        }
        if (!isThere)
        {
            locationTraveled=GetRandomPlayerPosition();
        }
    }
    /// <summary>
    /// ai gideceði rastgele karakterin konumunu seçiyor
    /// </summary>
    /// <returns></returns>
    private Transform GetRandomPlayerPosition()
    {
        if (GameManager.instance.playerList.Count > 1)
        {
            int randomIndex = Random.Range(0, players.Count);
            if (players[randomIndex] == transform)
            {
                randomIndex = (randomIndex + 1) % players.Count;
            }
            return players[randomIndex];
        }
        else { return transform; }
    }
    /// <summary>
    /// birini düþürdüyse gerekli artýrmalarý yapar
    /// </summary>
    public void ManDropped()
    {
        //coll.radius *= 2;
        //coll.height *= 2;
        //coll.center *= 2;
        //mesh.localScale *= 2; 
        power *= 1.5f;
        moveSpeed *= 1.5f;
        transform.localScale *= 1.5f;
        score += 1000;
        if(ai is false)
        {
            GameManager.instance.AddScore(1000);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Food"))
        {
            //coll.radius *= 1.1f;
            //coll.height *= 1.1f;
            //mesh.localScale *= 1.1f;
            power *= 1.1f;
            moveSpeed *= 1.1f;
            transform.localScale *= 1.1f;
            score += 100;
            if (ai is false)
            {
                GameManager.instance.AddScore(100);
            }
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("Fall"))
        {
            if (ai)
            {
                GameManager.instance.PlayerListChange(transform);
                Destroy(gameObject);
            }
            else
            {
                GameManager.instance.GameOver();
            }
            if (lastTouch != null)
            {
                lastTouch.GetComponent<Player>().ManDropped();
            }
        }
        else if (other.CompareTag("TriggerPoint"))
        {
            other.GetComponentInParent<Rigidbody>().AddForce(3 * power * transform.forward, ForceMode.Impulse);
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<Rigidbody>().AddForce(2 * power * transform.forward, ForceMode.Impulse);
            lastTouch = collision.gameObject;
        }
    }

}
