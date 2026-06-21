using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class NewMovement : MonoBehaviour
{
    private const float GroundCheckDistance = 0.08f;
    private const float WallCheckDistance = 0.04f;
    private const float GroundNormalThreshold = 0.5f;
    private const float WallNormalThreshold = 0.5f;
    private const float InputDeadZone = 0.01f;

    [Header("Move")]
    [FormerlySerializedAs("maxMoveSpeed")]
    [SerializeField] private float maxGroundMoveSpeed = 8f;
    [FormerlySerializedAs("maxAirMoveSpeed")]
    [SerializeField] private float maxAirMoveSpeed = 12f;
    [FormerlySerializedAs("acceleration")]
    [SerializeField] private float moveAcceleration = 60f;
    [SerializeField] private float groundDeceleration = 80f;
    [SerializeField] private float jumpPower = 12f;
    [SerializeField] private float jumpBufferTime = 0.12f;

    [Header("Recoil")]
    [SerializeField] private float recoilDeceleration = 16f;
    [SerializeField] private float airHorizontalRecoilDeceleration = 2f;
    [SerializeField] private float airVerticalRecoilDeceleration = 5f;

    [Header("Ground")]
    [FormerlySerializedAs("groundLayer")]
    [SerializeField] private LayerMask groundLayer = 1 << 3;

    [Header("Wall")]
    [SerializeField] private LayerMask wallLayer = 1 << 7;

    [Header("Trail")]
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField, Min(0f)] private float trailSpeedThreshold = 18f;

    [Header("Audio")]
    [SerializeField] private AudioSource oneShotAudioSource;
    [SerializeField] private AudioSource windAudioSource;
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private AudioClip pickupClip;
    [SerializeField] private AudioClip landingClip;
    [SerializeField] private AudioClip windClip;
    [SerializeField, Min(0f)] private float windAirborneDelay = 0.3f;
    [SerializeField, Min(0f)] private float windSpeedThreshold = 18f;
    [SerializeField, Min(0f)] private float windFullVolumeSpeed = 30f;
    [SerializeField, Range(0f, 1f)] private float jumpVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float pickupVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float landingVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float windMinVolume = 0.15f;
    [SerializeField, Range(0f, 1f)] private float windMaxVolume = 0.6f;

    private readonly RaycastHit2D[] groundHits = new RaycastHit2D[4];
    private readonly RaycastHit2D[] wallHits = new RaycastHit2D[4];

    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private ContactFilter2D groundFilter;
    private ContactFilter2D wallFilter;
    private Vector2 moveInput;
    private Vector2 recoilVelocity;
    private Vector2 appliedRecoilVelocity;
    private float moveVelocityX;
    private float moveAccelerationX;
    private float jumpBufferTimer;
    private float airborneTimer;
    private bool groundStateInitialized;
    private bool wasGrounded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();

        if (trailRenderer == null)
        {
            trailRenderer = GetComponent<TrailRenderer>();
        }

        UpdateTrail();

        groundFilter = new ContactFilter2D
        {
            useLayerMask = true,
            useTriggers = false
        };
        groundFilter.SetLayerMask(groundLayer);

        wallFilter = new ContactFilter2D
        {
            useLayerMask = true,
            useTriggers = false
        };
        wallFilter.SetLayerMask(wallLayer);

        EnsureAudioSources();
    }

    private void FixedUpdate()
    {
        bool grounded = IsGrounded();
        bool landed = groundStateInitialized && !wasGrounded && grounded;
        Vector2 baseVelocity = rb.linearVelocity - appliedRecoilVelocity;

        UpdateJumpBufferTimer();
        UpdateMoveVelocity(grounded);
        bool jumped = TryJump(grounded, ref baseVelocity);
        UpdateRecoilVelocity(grounded && !jumped);
        ResolveWallVelocity();

        appliedRecoilVelocity = recoilVelocity;
        rb.linearVelocity = new Vector2(moveVelocityX, baseVelocity.y) + appliedRecoilVelocity;

        UpdateTrail();
        UpdateWindAudio(grounded, landed);

        wasGrounded = grounded;
        groundStateInitialized = true;
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            jumpBufferTimer = jumpBufferTime;
        }
    }

    public void AddRecoil(Vector2 value)
    {
        Vector2 previousRecoilVelocity = recoilVelocity;
        recoilVelocity += value;

        Vector2 recoilDelta = recoilVelocity - previousRecoilVelocity;
        rb.linearVelocity += recoilDelta;
        appliedRecoilVelocity = recoilVelocity;
    }

    public void PlayPickupSound()
    {
        PlayOneShot(pickupClip, pickupVolume);
    }

    private void UpdateMoveVelocity(bool grounded)
    {
        float inputDirection = Mathf.Abs(moveInput.x) > InputDeadZone ? Mathf.Sign(moveInput.x) : 0f;
        float maxSpeed = grounded ? maxGroundMoveSpeed : maxAirMoveSpeed;
        moveAccelerationX = 0f;

        if (inputDirection != 0f)
        {
            if (moveVelocityX != 0f && Mathf.Sign(moveVelocityX) != inputDirection)
            {
                moveVelocityX = 0f;
                moveAccelerationX = 0f;
                return;
            }

            if (Mathf.Abs(moveVelocityX) > maxSpeed)
            {
                moveVelocityX = Mathf.MoveTowards(
                    moveVelocityX,
                    inputDirection * maxSpeed,
                    groundDeceleration * Time.fixedDeltaTime);
                return;
            }

            moveAccelerationX = inputDirection * moveAcceleration;
            moveVelocityX = Mathf.Clamp(
                moveVelocityX + moveAccelerationX * Time.fixedDeltaTime,
                -maxSpeed,
                maxSpeed);

            return;
        }

        moveAccelerationX = 0f;

        if (grounded)
        {
            moveVelocityX = Mathf.MoveTowards(moveVelocityX, 0f, groundDeceleration * Time.fixedDeltaTime);
        }
    }

    private bool TryJump(bool grounded, ref Vector2 baseVelocity)
    {
        if (jumpBufferTimer <= 0f)
        {
            return false;
        }

        if (grounded)
        {
            baseVelocity.y = jumpPower;
            jumpBufferTimer = 0f;
            PlayOneShot(jumpClip, jumpVolume);
            return true;
        }

        return false;
    }

    private void UpdateWindAudio(bool grounded, bool landed)
    {
        if (windAudioSource == null || windClip == null)
        {
            return;
        }

        bool wasWindPlaying = windAudioSource.isPlaying;

        if (landed && wasWindPlaying)
        {
            PlayOneShot(landingClip, landingVolume);
        }

        airborneTimer = grounded ? 0f : airborneTimer + Time.fixedDeltaTime;

        float speed = rb.linearVelocity.magnitude;
        bool airborneLongEnough = airborneTimer >= windAirborneDelay;
        bool fastEnough = speed >= windSpeedThreshold;
        bool shouldPlay = airborneLongEnough || fastEnough;

        if (shouldPlay)
        {
            if (windAudioSource.clip != windClip)
            {
                windAudioSource.clip = windClip;
            }

            windAudioSource.loop = true;
            float fullVolumeSpeed = Mathf.Max(windSpeedThreshold + 0.01f, windFullVolumeSpeed);
            float volumeFactor = Mathf.InverseLerp(windSpeedThreshold, fullVolumeSpeed, speed);
            windAudioSource.volume = Mathf.Lerp(windMinVolume, windMaxVolume, volumeFactor)
                * GameSettings.SfxVolume;

            if (!windAudioSource.isPlaying)
            {
                windAudioSource.Play();
            }

            return;
        }

        if (windAudioSource.isPlaying)
        {
            windAudioSource.Stop();
        }
    }

    private void UpdateTrail()
    {
        if (trailRenderer != null)
        {
            trailRenderer.emitting = rb.linearVelocity.magnitude >= trailSpeedThreshold;
        }
    }

    private void PlayOneShot(AudioClip clip, float volume)
    {
        if (oneShotAudioSource == null || clip == null)
        {
            return;
        }

        oneShotAudioSource.PlayOneShot(clip, volume * GameSettings.SfxVolume);
    }

    private void EnsureAudioSources()
    {
        if (oneShotAudioSource == null)
        {
            oneShotAudioSource = gameObject.AddComponent<AudioSource>();
        }

        if (windAudioSource == null)
        {
            windAudioSource = gameObject.AddComponent<AudioSource>();
        }

        oneShotAudioSource.playOnAwake = false;
        windAudioSource.playOnAwake = false;
        windAudioSource.loop = true;
    }

    private void UpdateJumpBufferTimer()
    {
        if (jumpBufferTimer <= 0f)
        {
            return;
        }

        jumpBufferTimer -= Time.fixedDeltaTime;
    }

    private void OnDisable()
    {
        moveInput = Vector2.zero;
        moveVelocityX = 0f;
        recoilVelocity = Vector2.zero;
        appliedRecoilVelocity = Vector2.zero;
        airborneTimer = 0f;
        groundStateInitialized = false;

        if (trailRenderer != null)
        {
            trailRenderer.emitting = false;
        }

        windAudioSource?.Stop();
    }

    private void UpdateRecoilVelocity(bool grounded)
    {
        if (grounded)
        {
            recoilVelocity.y = 0f;
        }

        float verticalDeceleration = grounded ? recoilDeceleration : airVerticalRecoilDeceleration;
        float horizontalDeceleration = grounded ? groundDeceleration : airHorizontalRecoilDeceleration;

        recoilVelocity = new Vector2(
            Mathf.MoveTowards(recoilVelocity.x, 0f, horizontalDeceleration * Time.fixedDeltaTime),
            Mathf.MoveTowards(recoilVelocity.y, 0f, verticalDeceleration * Time.fixedDeltaTime));
    }

    private bool IsGrounded()
    {
        int count = playerCollider.Cast(Vector2.down, groundFilter, groundHits, GroundCheckDistance);

        for (int i = 0; i < count; i++)
        {
            if (groundHits[i].normal.y > GroundNormalThreshold)
            {
                return true;
            }
        }

        return false;
    }

    private void ResolveWallVelocity()
    {
        float totalVelocityX = moveVelocityX + recoilVelocity.x;

        if (Mathf.Abs(totalVelocityX) <= InputDeadZone)
        {
            return;
        }

        float direction = Mathf.Sign(totalVelocityX);

        if (!IsTouchingWall(direction))
        {
            return;
        }

        if (moveVelocityX != 0f && Mathf.Sign(moveVelocityX) == direction)
        {
            moveVelocityX = 0f;
            moveAccelerationX = 0f;
        }

        if (recoilVelocity.x != 0f && Mathf.Sign(recoilVelocity.x) == direction)
        {
            recoilVelocity.x = 0f;
        }
    }

    private bool IsTouchingWall(float direction)
    {
        Vector2 castDirection = direction > 0f ? Vector2.right : Vector2.left;
        int count = playerCollider.Cast(castDirection, wallFilter, wallHits, WallCheckDistance);

        for (int i = 0; i < count; i++)
        {
            float normalX = wallHits[i].normal.x;

            if (direction > 0f && normalX < -WallNormalThreshold)
            {
                return true;
            }

            if (direction < 0f && normalX > WallNormalThreshold)
            {
                return true;
            }
        }

        return false;
    }
}
