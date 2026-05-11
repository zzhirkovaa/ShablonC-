using UnityEngine;

public class UIButtonSound : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clickSound;

    public void PlayClick()
    {
        if (audioSource == null || clickSound == null)
        {
            Debug.LogWarning("Нет AudioSource или AudioClip");
            return;
        }

        audioSource.PlayOneShot(clickSound);
    }
}