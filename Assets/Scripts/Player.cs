using UnityEngine;
using System.Collections;
using Cinemachine;
using Tools.Audio;

[RequireComponent(typeof(Controller2D))]
public class Player : MonoBehaviour
{

    [Header("Movements")]
    public float maxJumpHeight = 4;
    public float minJumpHeight = 1;
    public float timeToJumpApex = .4f;
    float accelerationTimeAirborne = .2f;
    float accelerationTimeGrounded = .1f;
    float moveSpeed = 6;

    float gravity;
    float maxJumpVelocity;

    float minJumpVelocity;

    public int maxJumps;
    int currentJump;

    Vector3 velocity;
    float velocityXSmoothing;

    Controller2D controller;

    [Header("Audio")]
    [SerializeField]
    private AudioManager audioManager = null;

    private AudioSourcePlayer audioPlayer = null;

    [SerializeField]
    private Sound playerLanding = null;

    private Animator animator = null;

    private SpriteRenderer sr = null;

    private CinemachineImpulseSource impulseSource;

    void Start()
    {
        audioPlayer = AudioSourcePlayer.AddAsComponent(gameObject, audioManager);
        controller = GetComponent<Controller2D>();
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        impulseSource = GetComponent<CinemachineImpulseSource>();

        gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);
        print("Gravity: " + gravity + "  Jump Velocity: " + maxJumpVelocity);
    }

    void Update()
    {

        if (velocity.y < -14 && controller.collisions.below)
        {
            Debug.Log("Pong");
            animator.SetBool("isJumping", false);
            animator.SetTrigger("triggerBounce");
            impulseSource.GenerateImpulse(velocity);
            audioPlayer.PlaySound(playerLanding);
        }

        if (controller.collisions.above || controller.collisions.below)
        {
            velocity.y = 0;
            if (controller.collisions.below)
            {
                currentJump = 0;
                animator.SetBool("isJumping", false);
            }
        }

        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (Input.GetButtonDown("Jump") && (controller.collisions.below || currentJump < maxJumps))
        {
            currentJump += 1;
            velocity.y = maxJumpVelocity;
            animator.SetBool("isJumping", true);
        }

        if (Input.GetButtonUp("Jump"))
        {
            if (velocity.y > minJumpVelocity)
            {
                velocity.y = minJumpVelocity;
            }
        }

        float targetVelocityX = input.x * moveSpeed;

        if (input.x != 0)
        {
            animator.SetBool("isRunning", true);
            if (input.x < 0)
            {
                sr.flipX = true;
            }
            else
            {
                sr.flipX = false;
            }
        }
        else
        {
            animator.SetBool("isRunning", false);
        }
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
