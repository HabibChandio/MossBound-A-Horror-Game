using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    public PlayerController playerController;

    [Header("Navigation")]
    public NavMeshAgent agent;
    public Transform[] waypoints;
    public float waypointWaitTime = 2f;
    private Transform currentWaypoint;
    private float waitTimer = 0f;

    [Header("Player Detection")]
    public Transform player;
    public float visionRange = 10f;
    public float visionAngle = 60f;
    public LayerMask obstacles;
    private Vector3 lastPlayerPosition;
    private bool playerDetected = false;

    [Header("Animations")]
    public Animator animator;
    public AnimationClip jumpscareClip;

    [Header("Audio Sources")]
    public AudioSource constantAudioSource;
    public AudioSource movementAudioSource;

    [Header("Audio Clips")]
    public AudioClip breathingLoop;
    public AudioClip crawlSound;
    public AudioClip fastCrawlSound;
    public AudioClip alertSound;

    [Header("Audio Volume")]
    [Range(0f, 1f)] public float breathingVolume = 0.6f;
    [Range(0f, 1f)] public float crawlVolume = 0.8f;
    [Range(0f, 1f)] public float fastCrawlVolume = 1f;
    [Range(0f, 1f)] public float alertVolume = 1f;

    [Header("Audio Pitch (Speed)")]
    [Range(0.1f, 3f)] public float breathingPitch = 1f;
    [Range(0.1f, 3f)] public float crawlPitch = 1f;
    [Range(0.1f, 3f)] public float fastCrawlPitch = 1f;
    [Range(0.1f, 3f)] public float alertPitch = 1f;

    [Header("Movement Speeds")]
    public float crawlSpeed = 1.5f;
    public float fastCrawlSpeed = 4f;

    [Header("Jumpscare")]
    public JumpscareController jumpscareController;

    public Transform eye;

    private enum State { Patrolling, Investigating, Chasing }
    private State currentState = State.Patrolling;


    void Start()
    {
        if (constantAudioSource != null && breathingLoop != null)
        {
            constantAudioSource.clip = breathingLoop;
            constantAudioSource.loop = true;
            constantAudioSource.volume = breathingVolume;
            constantAudioSource.pitch = breathingPitch;
            constantAudioSource.Play();
        }

        ChooseRandomWaypoint();
    }


    void Update()
    {
        switch (currentState)
        {
            case State.Patrolling:
                Patrol();
                break;
            case State.Investigating:
                Investigate();
                break;
            case State.Chasing:
                ChasePlayer();
                break;
        }

        HandleMovementAudio();
        CheckPlayerVision();
    }

    void ChooseRandomWaypoint()
    {
        currentWaypoint = waypoints[Random.Range(0, waypoints.Length)];
        agent.speed = crawlSpeed;
        agent.SetDestination(currentWaypoint.position);
    }

    void Patrol()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waypointWaitTime)
            {
                waitTimer = 0f;
                ChooseRandomWaypoint();
            }
        }
    }

    void CheckPlayerVision()
    {
        Vector3 direction = player.position - eye.position;
        float distance = direction.magnitude;

        if (distance > visionRange) return;

        float angle = Vector3.Angle(eye.forward, direction);
        if (angle > visionAngle / 2f) return;

        if (Physics.Raycast(eye.position, direction.normalized, out RaycastHit hit, visionRange, ~obstacles))
        {
            if (hit.collider.CompareTag("Player"))
            {
                // Player detected
                playerDetected = true;
                currentState = State.Chasing;
                lastPlayerPosition = player.position;
            }
        }
    }

    void ChasePlayer()
    {
        if (!playerDetected) return;

        agent.speed = fastCrawlSpeed;
        agent.SetDestination(player.position);

        if (playerController.isHiding)
        {
            agent.SetDestination(lastPlayerPosition);
            currentState = State.Investigating;
            playerDetected = false;
        }
    }

    public void InvestigateNoise(Vector3 pos)
    {
        if (currentState != State.Chasing)
        {
            currentState = State.Investigating;
            lastPlayerPosition = pos;

            agent.SetDestination(pos);
            agent.speed = crawlSpeed;

            PlayOneShot(alertSound, alertVolume, alertPitch);
        }
    }

    void Investigate()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            currentState = State.Patrolling;
            ChooseRandomWaypoint();
        }
    }

    void HandleMovementAudio()
    {
        if (currentState == State.Chasing)
        {
            animator.SetBool("FastCrawl", true);
            animator.SetBool("Crawl", false);
            PlayLoop(movementAudioSource, fastCrawlSound, fastCrawlVolume, fastCrawlPitch);
        }
        else if (currentState == State.Investigating || currentState == State.Patrolling)
        {
            float speed = agent.velocity.magnitude;

            if (speed > crawlSpeed * 0.1f)
            {
                animator.SetBool("Crawl", true);
                animator.SetBool("FastCrawl", false);
                PlayLoop(movementAudioSource, crawlSound, crawlVolume, crawlPitch);
            }
            else
            {
                animator.SetBool("Crawl", false);
                animator.SetBool("FastCrawl", false);
                if (movementAudioSource != null) movementAudioSource.Stop();
            }
        }
    }

    void PlayLoop(AudioSource src, AudioClip clip, float volume, float pitch)
    {
        if (src == null || clip == null) return;
        if (src.clip == clip && src.isPlaying) return;

        src.clip = clip;
        src.volume = volume;
        src.pitch = pitch;
        src.loop = true;
        src.Play();
    }

    void PlayOneShot(AudioClip clip, float volume, float pitch)
    {
        if (clip == null || movementAudioSource == null) return;

        movementAudioSource.pitch = pitch;
        movementAudioSource.PlayOneShot(clip, volume);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (playerController.isHiding) return;
            jumpscareController?.StartJumpscare();
        }
    }
}
