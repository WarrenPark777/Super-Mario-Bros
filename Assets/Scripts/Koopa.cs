using UnityEngine;

public class Koopa : MonoBehaviour
{
    public Sprite shellSprite;
    public float shellSpeed = 12f;

    private bool shelled;
    private bool pushed;

    private AudioSource audioSource;
    public AudioClip stompSound;
    public AudioClip shellKickSound;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!shelled && collision.gameObject.CompareTag("Player") && collision.gameObject.TryGetComponent(out Player player))
        {
            if (player.starpower) {
                Hit();
            } else if (collision.transform.DotTest(transform, Vector2.down)) {
                EnterShell();
            }  else {
                player.Hit();
            }
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Fireball"))
        {
            Destroy(collision.gameObject);
            Hit();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (shelled && other.CompareTag("Player") && other.TryGetComponent(out Player player))
        {
            if (!pushed)
            {
                Vector2 direction = new(transform.position.x - other.transform.position.x, 0f);
                PushShell(direction);
            }
            else
            {
                if (player.starpower) {
                    Hit();
                } else {
                    player.Hit();
                }
            }
        }
        else if (!shelled && other.gameObject.layer == LayerMask.NameToLayer("Shell"))
        {
            Hit();
        }
    }

    private void EnterShell()
    {
        shelled = true;

        audioSource.PlayOneShot(stompSound);

        GetComponent<SpriteRenderer>().sprite = shellSprite;
        GetComponent<AnimatedSprite>().enabled = false;
        GetComponent<EntityMovement>().enabled = false;
    }

    private void PushShell(Vector2 direction)
    {
        pushed = true;

        audioSource.PlayOneShot(shellKickSound);

        GetComponent<Rigidbody2D>().isKinematic = false;

        EntityMovement movement = GetComponent<EntityMovement>();
        movement.direction = direction.normalized;
        movement.speed = shellSpeed;
        movement.enabled = true;

        gameObject.layer = LayerMask.NameToLayer("Shell");
    }

    public void Hit()
    {
        audioSource.PlayOneShot(shellKickSound);
        GetComponent<AnimatedSprite>().enabled = false;
        GetComponent<DeathAnimation>().enabled = true;
        GameManager.Instance.AddScore(100);
        Destroy(gameObject, 3f);
    }

    private void OnBecameInvisible()
    {
        if (pushed){
            Invoke(nameof(DestroyKoopa), 3.0f);
            }
    }

    private void DestroyKoopa()
    {
        Destroy(gameObject);
    }
    private void OnBecameVisible()
    {
        if(pushed)
            CancelInvoke(nameof(DestroyKoopa));
    }





}
