using UnityEngine;

public sealed class TutorialController : MonoBehaviour
{
    [SerializeField] private GameObject tutorial;

    private void Start()
    {
        EnableTutorial();
    }

    public void EnableTutorial()
    {
        if (tutorial != null)
        {
            tutorial.SetActive(true);
        }
    }

    public void DisableTutorial()
    {
        if (tutorial != null)
        {
            tutorial.SetActive(false);
        }
    }
}
