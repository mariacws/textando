using UnityEngine;

public class SimpleScript : MonoBehaviour
{
    private void DestroySelf()
    {
        Destroy(gameObject);
    }

    void Start()
    {
        // Call DestroySelf after 5 seconds
        Invoke("DestroySelf", 5f);
    }
}
