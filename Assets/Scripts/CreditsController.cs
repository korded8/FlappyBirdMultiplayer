using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class CreditsController : MonoBehaviour
{
    public TMP_Text skipCredtisText;
    public GameObject creditsPanel;

    private bool anyKeyPressed = true;
    private bool creditsEnded = false;
    private float skipCooldown = 3f;
    private float speed;
    private float time = 45f;

    private void Start()
    {
        speed = (creditsPanel.GetComponent<RectTransform>().rect.height + 1080) / time;

        StartCoroutine(ChangeSkipCreditsText());
        StartCoroutine(MoveCredits());
    }

    private void Update()
    {
        if (Keyboard.current.anyKey.wasPressedThisFrame && anyKeyPressed)
            SceneManager.LoadScene("Menu");

        if (creditsEnded)
            SceneManager.LoadScene("Menu");

        if (Keyboard.current.anyKey.wasPressedThisFrame)
        {
            float r = skipCredtisText.color.r;
            float g = skipCredtisText.color.g;
            float b = skipCredtisText.color.b;
            float a = 1;

            skipCredtisText.color = new Color(r, g, b, a);

            anyKeyPressed = true;
            StartCoroutine(ChangeSkipCreditsText());
        }
    }

    private IEnumerator MoveCredits()
    {
        float elapsed = 0f;
        while (true)
        {
            elapsed += Time.deltaTime;
            creditsPanel.transform.position += Vector3.up * speed * Time.deltaTime;

            if (elapsed > time && !creditsEnded)
                creditsEnded = true;
            yield return null;
        }
    }

    private IEnumerator ChangeSkipCreditsText()
    {
        float elapsed = 0f;

        while (elapsed < skipCooldown)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / skipCooldown);

            float r = skipCredtisText.color.r;
            float g = skipCredtisText.color.g;
            float b = skipCredtisText.color.b;
            float a = Mathf.Lerp(1, 0, t);

            skipCredtisText.color = new Color(r, g, b, a);

            yield return null;
        }

        anyKeyPressed = false;
    }
}