using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ExteriorToggle : MonoBehaviour
{
    [SerializeField] private GameObject exteriorBackdropRoot;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.transform.root.CompareTag("Player") && exteriorBackdropRoot != null)
        {
            exteriorBackdropRoot.SetActive(false);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.transform.root.CompareTag("Player") && exteriorBackdropRoot != null)
        {
            exteriorBackdropRoot.SetActive(true);
        }
    }
}
