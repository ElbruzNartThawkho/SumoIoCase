using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public TextMeshProUGUI alivePlayerText, scoreText, countdownText, rankText, scoreResText, winLoseText;
    public List<Transform> playerList;
    public GameObject plane;
    public Vector3 planeCenter;
    public float halfPlaneWidth, halfPlaneLength;


    private float nextSpawnTime;
    private int score = 0, alivePlayerCount;
    private bool isSpawning = true;

    [SerializeField] private GameObject food, gameOverScreen, gameOverCam, player;
    [SerializeField] private float minSpawnInterval = 8, maxSpawnInterval = 16, countdownTimer = 120;

    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        PlaneBorder();UpdateCountdownText(); UpdateAlivePlayerText();
        nextSpawnTime = Time.time + Random.Range(minSpawnInterval, maxSpawnInterval);
        StartCoroutine(SpawnObjectCoroutine());
        StartCoroutine(CountdownCoroutine());
    }
    /// <summary>
    /// ana karakter puaný artýrýr ve yazýyý günceller
    /// </summary>
    public void AddScore(int score)
    {
        this.score += score;
        scoreText.SetText(score.ToString());
    }
    /// <summary>
    /// oyun bitiþ ekranýný gösterir ve gerekli iþlemleri yapar
    /// </summary>
    public void GameOver()
    {
        if (gameOverScreen.activeSelf is false)
        {
            if (isSpawning)
            {
                rankText.SetText(alivePlayerCount.ToString());
                PlayerListChange(player.transform);
                Destroy(player); winLoseText.SetText("Lose");
            }
            else if(playerList.Count == 1)
            {
                if(player!=null)
                {
                    rankText.SetText(alivePlayerCount.ToString());
                    winLoseText.SetText("Win");
                }
                else
                {
                    Time.timeScale = 0;
                }
            }
            else
            {
                int rank = 1;
                playerList.Remove(player.transform);
                foreach (Transform v in playerList)
                {
                    if (v.GetComponent<Player>().score > player.GetComponent<Player>().score)
                    {
                        rank++;
                    }
                }
                rankText.SetText(rank.ToString());
                winLoseText.SetText(rank == 1 ? "Win" : "Lose");
            }
            scoreResText.SetText(score.ToString());
            gameOverScreen.SetActive(true);
            gameOverCam.SetActive(true);
        }
        else
        {
            Time.timeScale = 0;
        }
    }
    /// <summary>
    /// oyuncu listesi deðiþtiðinde kalan oyuncularýn listesini günceller
    /// </summary>
    public void PlayerListChange(Transform playerDeleted)
    {
        if (playerList.Count == 2)
        {
            isSpawning = false;
            GameOver();
        }
        playerList.Remove(playerDeleted);
        foreach (Transform v in playerList)
        {
            v.GetComponent<Player>().PlayerListUpdate();
        }
        UpdateAlivePlayerText();
    }
    /// <summary>
    /// plane sýnýrlarýný alýyor
    /// </summary>
    public void PlaneBorder()
    {
        halfPlaneWidth = plane.GetComponent<Renderer>().bounds.size.x / 2; halfPlaneLength = plane.GetComponent<Renderer>().bounds.size.z / 2;
        planeCenter = plane.transform.position;
    }
    /// <summary>
    /// belirli aralýklarda ,rastgele konum ve yönlerde food oluþturur
    /// isSpawning deðiþkenini false ise oluþturmaz
    /// </summary>
    /// <returns></returns>
    private IEnumerator SpawnObjectCoroutine()
    {
        WaitForSeconds wait = new WaitForSeconds(minSpawnInterval);

        while (isSpawning)
        {
            if (Time.time >= nextSpawnTime)
            {
                Vector3 randomPosition = new Vector3(
                    Random.Range(planeCenter.x - halfPlaneWidth, planeCenter.x + halfPlaneWidth),
                    1,
                    Random.Range(planeCenter.z - halfPlaneLength, planeCenter.z + halfPlaneLength)
                );
                Quaternion randomRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                Instantiate(food, randomPosition, randomRotation);
                nextSpawnTime = Time.time + Random.Range(minSpawnInterval, maxSpawnInterval);
            }
            yield return wait;
        }
    }
    /// <summary>
    /// süre bitene kadar sayar
    /// </summary>
    /// <returns></returns>
    private IEnumerator CountdownCoroutine()
    {
        while (countdownTimer > 0)
        {
            yield return new WaitForSeconds(1f);
            countdownTimer--;
            UpdateCountdownText();
        }
        isSpawning = false;
        GameOver();
    }
    /// <summary>
    /// sayaç yazýsýný uygun olarak günceller
    /// </summary>
    private void UpdateCountdownText()
    {
        int minutes = Mathf.FloorToInt(countdownTimer / 60);
        int seconds = Mathf.FloorToInt(countdownTimer % 60);
        countdownText.SetText(string.Format("{0:00}:{1:00}", minutes, seconds));
    }
    /// <summary>
    /// canlý oyuncu yazýsýný uygun olarak günceller
    /// </summary>
    private void UpdateAlivePlayerText()
    {
        alivePlayerCount=playerList.Count;
        alivePlayerText.SetText(alivePlayerCount.ToString());
    }
    /// <summary>
    /// sahneyi açar
    /// </summary>
    /// <param name="name"> sahne adý</param>
    public void OpenScene(string name)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(name);
    }
    public void ResumePauseBtn(int x)
    {
        Time.timeScale = x;
    }
}
