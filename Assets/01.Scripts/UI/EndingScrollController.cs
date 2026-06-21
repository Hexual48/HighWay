using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class EndingScrollController : MonoBehaviour
{
    [Header("Scroll")]
    [SerializeField, Min(0.01f)] private float moveSpeed = 1.5f;
    [SerializeField, Min(0.01f)] private float fadeDuration = 1.5f;

    [Header("Scene Transition")]
    [SerializeField, Min(0f)] private float holdDuration = 5f;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private SpriteRenderer[] spriteRenderers;

    private void Awake()
    {
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        SetAlpha(0f);
    }

    private IEnumerator Start()
    {
        float fadeElapsed = 0f;

        while (transform.position.y < 0f)
        {
            float deltaTime = Time.unscaledDeltaTime;
            Vector3 position = transform.position;
            position.y = Mathf.MoveTowards(position.y, 0f, moveSpeed * deltaTime);
            transform.position = position;

            fadeElapsed += deltaTime;
            SetAlpha(Mathf.Clamp01(fadeElapsed / fadeDuration));

            yield return null;
        }

        Vector3 targetPosition = transform.position;
        targetPosition.y = 0f;
        transform.position = targetPosition;
        SetAlpha(1f);

        yield return new WaitForSecondsRealtime(holdDuration);
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void SetAlpha(float alpha)
    {
        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
        {
            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
        }
    }
}
