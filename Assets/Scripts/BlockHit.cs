using System.Collections;
using UnityEngine;

public class BlockHit : MonoBehaviour
{
    public GameObject item;
    [Tooltip("Used in case the item must change based off of a condition (eg. if mario is small)")]
    public GameObject secondaryItem = null;
    public Sprite emptyBlock;
    public int maxHits = -1;
    private bool animating;

    public bool brick = false;
    public ParticleSystem breakParticle;

    private AudioSource audioSource;
    public AudioClip bumpSound;
    public AudioClip coinSound;

    //public bool hidden = false;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!animating && maxHits != 0)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                CheckForBlockHit(collision.gameObject);
            }
            else if (collision.gameObject.layer == LayerMask.NameToLayer("Shell"))
            {
                if (brick)
                {
                    Break(breakParticle);
                } else
                {
                    Hit(collision.gameObject.GetComponent<Player>());
                }
            }
        }

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!animating && maxHits != 0)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                CheckForBlockHit(collision.gameObject);
            }
        }
    }


    private void CheckForBlockHit(GameObject collision)
    {
        Player player = collision.GetComponent<Player>();

        if (collision.transform.DotTest(transform, Vector2.up))
        {
            if (!player.small && brick)
            {
                Break(breakParticle);
            }
            else
            {
                Hit(collision.GetComponent<Player>());
            }
        }
    }

    private void Hit(Player player)
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.enabled = true; // show if hidden
        gameObject.layer = LayerMask.NameToLayer("Default");
        gameObject.GetComponent<BoxCollider2D>().isTrigger = false; // hidden blocks should have collision after being discovered

        audioSource.PlayOneShot(bumpSound);

        maxHits--;

        if (maxHits == 0) {
            spriteRenderer.sprite = emptyBlock;
        }

        if (item != null) {
            if (item.name == "BlockCoin")
            {
                audioSource.PlayOneShot(coinSound);
            }
            if (item.name == "FireFlower" && player.small)
            {
                Instantiate(secondaryItem, transform.position, Quaternion.identity);
            } else
            {
                Instantiate(item, transform.position, Quaternion.identity);
            }
        }

        StartCoroutine(Animate());
    }

    private IEnumerator Animate()
    {
        animating = true;

        Vector3 restingPosition = transform.localPosition;
        Vector3 animatedPosition = restingPosition + Vector3.up * 0.5f;

        yield return Move(restingPosition, animatedPosition);
        CheckForThingsAbove();
        yield return Move(animatedPosition, restingPosition);

        animating = false;
        
    }

    private IEnumerator Move(Vector3 from, Vector3 to)
    {
        float elapsed = 0f;
        float duration = 0.125f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;

            transform.localPosition = Vector3.Lerp(from, to, t);
            elapsed += Time.deltaTime;

            yield return null;
        }

        transform.localPosition = to;
    }

    private void Break(ParticleSystem breakParticle)
    {
        Instantiate(breakParticle, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    private void CheckForThingsAbove()
    {
        Vector2 center = (Vector2)transform.position + Vector2.up * 0.5f;
        Vector2 size = new Vector2(1f, 0.25f);

        Collider2D[] hits = Physics2D.OverlapBoxAll(center, size, 0f);

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                if (hit.TryGetComponent(out Goomba goomba))
                {
                    goomba.Hit();
                }
                if (hit.TryGetComponent(out Koopa koopa))
                {
                    koopa.Hit();
                }
            }
            else if (hit.CompareTag("Powerup"))
            {
                Rigidbody2D rb = hit.attachedRigidbody;
                if (rb != null)
                {
                    // FLIP their X velocity
                    rb.linearVelocity = new Vector2(-rb.linearVelocity.x, rb.linearVelocity.y);
                }
            }
        }
    }

}
