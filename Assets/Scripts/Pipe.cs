using System.Collections;
using UnityEngine;

public class Pipe : MonoBehaviour
{
    public Transform connection;
    public KeyCode enterKeyCode = KeyCode.S;
    public Vector3 enterDirection = Vector3.down;
    public Vector3 exitDirection = Vector3.zero;

    private AudioSource audioSource;
    public AudioClip pipeSound;

    private Timer timer;

    private Music music;

    public bool toSubArea = false;


    private void Awake()
    {
        if (gameObject.GetComponent<AudioSource>() != null)
        {
            audioSource = gameObject.GetComponent<AudioSource>();
        }
        music = Camera.main.GetComponent<Music>();
        timer = Camera.main.GetComponent<Timer>();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (connection != null && other.CompareTag("Player"))
        {
            if (Input.GetKey(enterKeyCode) && other.TryGetComponent(out Player player)) {
                StartCoroutine(Enter(player));
            }
        }
    }

    private IEnumerator Enter(Player player)
    {
        player.movement.enabled = false;

        Vector3 enteredPosition = transform.position + enterDirection;
        Vector3 enteredScale = Vector3.one * 0.5f;

        if (music.hurryWarningPlaying)
        {
            music.gameObject.GetComponent<AudioSource>().Pause();
        } else
        {
            music.StopMusic();
        }

        timer.stopTime = true;
        audioSource.PlayOneShot(pipeSound);

        yield return Move(player.transform, enteredPosition, enteredScale);
        yield return new WaitForSeconds(1f);

        var sideSrolling = Camera.main.GetComponent<SideScrollingCamera>();
        sideSrolling.SetUnderground(connection.position.y < sideSrolling.undergroundThreshold);

        music.subArea = toSubArea;
        timer.stopTime = false;

        if (music.hurryWarningPlaying)
        {
            music.gameObject.GetComponent<AudioSource>().UnPause();
        }
        else
        {
            music.PlayMusic();
        }

        if (exitDirection != Vector3.zero)
        {
            player.transform.position = connection.position - exitDirection;
            audioSource.PlayOneShot(pipeSound);
            yield return Move(player.transform, connection.position + exitDirection, Vector3.one);
        }
        else
        {
            player.transform.position = connection.position;
            player.transform.localScale = Vector3.one;
        }

        player.movement.enabled = true;
    }

    private IEnumerator Move(Transform player, Vector3 endPosition, Vector3 endScale)
    {
        float elapsed = 0f;
        float duration = 1f;

        Vector3 startPosition = player.position;
        Vector3 startScale = player.localScale;

        while (elapsed < duration)
        {
            float t = elapsed / duration;

            player.position = Vector3.Lerp(startPosition, endPosition, t);
            player.localScale = Vector3.Lerp(startScale, endScale, t);
            elapsed += Time.deltaTime;

            yield return null;
        }

        player.position = endPosition;
        player.localScale = endScale;
    }

}
