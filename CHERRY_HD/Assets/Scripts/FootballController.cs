
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class FootballController : MonoBehaviour
{
    public Rigidbody footballBody;
    public float launchForce = 8f;

    public AudioSource footballAudio;
    public AudioClip netSounds;
    public AudioClip goalHit;


    public Transform spawnPoint;

    private bool hasBeenLaunched = false;
    private bool playerCanHit = true;

    public int Score { get; private set; }
    public TMP_Text scoreText;
    public TMP_Text successText;

    private bool setForReset = false;
    private bool hitNet = false;

    [SerializeField] public Animator penaltyTakerAnimator;

    private void Start()
    {
        UpdateScoreText();
    }

    private void Awake()
    {
        if (successText != null)
        {
            successText.gameObject.SetActive(false);
        }
        if (footballBody == null)
        {
            footballBody = GetComponent<Rigidbody>();
        }
        StartCoroutine(Shoot());

    }

    private IEnumerator Shoot()
    {
        float speedFactor = launchForce / 16;
        penaltyTakerAnimator.speed = speedFactor;
        penaltyTakerAnimator.Play("Penalty", 0, 0f);
        yield return new WaitForSeconds(0.5f / speedFactor);
        Vector3 target = FindTarget();
        footballAudio.PlayOneShot(goalHit, 0.4f);
        Launch(target);
    }

    Vector3 FindTarget()
    {
        float x = Random.Range(-1.3f, 1.7f);
        float y = Random.Range(0.1f, 2f);
        float z = -7.872f;
        return new Vector3(x, y, z);
    }

    public void Launch(Vector3 target)
    {
        if (hasBeenLaunched)
            return;
        playerCanHit = true;
        hasBeenLaunched = true;

        footballBody.isKinematic = false;
        footballBody.linearVelocity = Vector3.zero;
        footballBody.angularVelocity = Vector3.zero;

        Vector3 direction = (target - transform.position).normalized;

        footballBody.AddForce(direction * launchForce * 1 / 5, ForceMode.Impulse);
    }



    private IEnumerator ResetBall(float time)
    {
        yield return new WaitForSeconds(time);
        Debug.Log($"Ball Reset.");
        footballBody.useGravity = false;
        hasBeenLaunched = false;

        footballBody.linearVelocity = Vector3.zero;
        footballBody.angularVelocity = Vector3.zero;

        footballBody.isKinematic = true;

        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;

        footballBody.isKinematic = false;
        setForReset = false;
        hitNet = false;
        StartCoroutine(Shoot());
        Debug.Log($"Ball Reset Over.");
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("GoalNet"))
        {
            if (!hitNet)
            {
                hitNet = true;
                footballBody.useGravity = true;
                footballAudio.PlayOneShot(netSounds, Mathf.Min(launchForce / 20, 1));
                return;
            }

        }

        if (collision.collider.CompareTag("Glove"))
        {
            if (playerCanHit && !setForReset)
            {
                footballAudio.PlayOneShot(goalHit, Mathf.Min(launchForce / 20, 1));
                setForReset = true;
                footballBody.useGravity = true;
                playerCanHit = false;
                launchForce++;
                Score++;
                Vector3 ricochet = new(0, 0, 1);
                footballBody.AddForce(ricochet * launchForce * 1 / 5, ForceMode.Impulse);
                UpdateScoreText();
                StartCoroutine(ShowScoreText());
                StartCoroutine(ResetBall(2));

            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("GoalLine"))
        {
            Debug.Log("Good try");
            if (!setForReset)
            {
                setForReset = true;
                launchForce--;
                if (launchForce < 4f)
                {
                    launchForce = 4f;
                }
                StartCoroutine(ResetBall(1));

            }
        }
    }

    private IEnumerator ShowScoreText()
    {
        if (successText != null)
        {
            successText.gameObject.SetActive(true);
        }

        Debug.Log("Good job! Target reached. Score: " + Score);

        yield return new WaitForSeconds(1f);
        if (successText != null)
        {
            successText.gameObject.SetActive(false);
        }

    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + Score;
        }
    }

}
