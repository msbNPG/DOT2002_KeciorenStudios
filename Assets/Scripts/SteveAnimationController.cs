using UnityEngine;

public class SteveAnimationController : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // 1'e basınca Happy (Threshold 1)
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            animator.SetFloat("AnimState", 1f);
        }
        // 2'ye basınca Sad (Threshold 2)
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            animator.SetFloat("AnimState", 2f);
        }
        // 3'e basınca Dance (Threshold 3)
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            animator.SetFloat("AnimState", 3f);
        }
        // 4'e basınca Idle (Threshold 0)
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            animator.SetFloat("AnimState", 0f);
        }
    }
}