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
    //[SerializeField] Transform mesh;

    List<Transform> players;
    PlayerInput playerInput;
    //CapsuleCollider coll;
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
            locationTraveled = GetNearestPlayerPosition();
        }
        else
        {
            playerInput = GetComponent<PlayerInput>();
        }
        //coll = GetComponent<CapsuleCollider>();
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }
    private void FixedUpdate()
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
            if(locationTraveled is null)
            {
                locationTraveled = GetNearestPlayerPosition();
            }
            else
            {
                looked2Point = locationTraveled.position - transform.position;
                looked2Point.y = 0;
            }
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(looked2Point), turnSpeed * Time.fixedDeltaTime);
            rb.MovePosition(moveSpeed * Time.fixedDeltaTime * transform.forward + transform.position);
            animator.SetFloat("IdleWalkRun", moveSpeed);
            //if (locationTraveled != null && Vector3.Distance(locationTraveled.position, transform.position) < transform.localScale.x / 2)
            //{
            //    locationTraveled = GetNearestPlayerPosition();
            //}
        }
        else
        {
            input = playerInput.actions["Move"].ReadValue<Vector2>();
            if (input.x > 0.02f || input.y > 0.02f || input.y < -0.02f || input.y < -0.02f)
            {
                looked2Point.x = transform.position.x + input.x * 100; looked2Point.z = transform.position.z + input.y * 100;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(looked2Point), turnSpeed * Time.deltaTime);
                rb.MovePosition(Mathf.Sqrt(Mathf.Pow(input.x, 2) + Mathf.Pow(input.y, 2)) * moveSpeed * Time.fixedDeltaTime * transform.forward + transform.position);
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
            locationTraveled=GetNearestPlayerPosition();
        }
    }
    /// <summary>
    /// ai gideceði karakterin konumunu seçiyor
    /// </summary>
    /// <returns></returns>
    private Transform GetNearestPlayerPosition()
    {
        if (GameManager.instance.playerList.Count > 1)
        {
            Transform nearestPlayer = null;
            float closestDistance = float.MaxValue;

            GameManager.instance.PlaneBorder();

            foreach (Transform player in players)
            {
                if (player == transform)
                    continue;

                float distance = Vector3.Distance(transform.position, player.position);
                if (distance < closestDistance && IsWithinPlaneBounds(player.position))
                {
                    closestDistance = distance;
                    nearestPlayer = player;
                }
            }

            return nearestPlayer;
        }
        else
        {
            return transform;
        }
    }
    /// <summary>
    /// seçtiði karakter plane içinde mi kontrol ediyor
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    private bool IsWithinPlaneBounds(Vector3 position)
    {
        Vector3 planeCenter = GameManager.instance.planeCenter;
        float halfPlaneWidth = GameManager.instance.halfPlaneWidth;
        float halfPlaneLength = GameManager.instance.halfPlaneLength;

        if (position.x >= planeCenter.x - halfPlaneWidth && position.x <= planeCenter.x + halfPlaneWidth &&
            position.z >= planeCenter.z - halfPlaneLength && position.z <= planeCenter.z + halfPlaneLength)
        {
            return true;
        }
        else
        {
            return false;
        }
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
        power *= 1.25f;
        moveSpeed *= 1.25f;
        transform.localScale *= 1.25f;
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
            other.transform.GetComponentInParent<Player>().lastTouch = gameObject;
            lastTouch = other.transform.parent.gameObject;
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<Rigidbody>().AddForce(power * transform.forward, ForceMode.Impulse);
            lastTouch = collision.gameObject;
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerListUpdate();
            locationTraveled = GetNearestPlayerPosition();
        }
    }
}
