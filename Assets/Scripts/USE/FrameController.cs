using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FrameController : MonoBehaviour
{
    [SerializeField] private GameObject frame;
    [SerializeField] private Button hideButton;
    [SerializeField] private Button showButton;

    private Animator frameAnimator;
    private float hideAnimationLength = 1f; // Установите длительность вашей HideAnimation

    void Start()
    {
        frameAnimator = frame.GetComponent<Animator>();
        hideButton.onClick.AddListener(HideFrame);
        showButton.onClick.AddListener(ShowFrame);
        showButton.gameObject.SetActive(false);
    }

    private void HideFrame()
    {
        if (!frameAnimator.GetCurrentAnimatorStateInfo(0).IsName("Hide"))
        {
            frameAnimator.SetTrigger("HideTrigger");
            StartCoroutine(ShowButtonAfterAnimation());
        }
    }

    private IEnumerator ShowButtonAfterAnimation()
    {
        yield return new WaitForSeconds(hideAnimationLength);
        showButton.gameObject.SetActive(true);
    }

    private void ShowFrame()
    {
        frameAnimator.SetTrigger("ShowTrigger");
        showButton.gameObject.SetActive(false);
    }
}