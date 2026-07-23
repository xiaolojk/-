using UnityEngine;

/// <summary>
/// 玩家移动控制 - 2D 俯视角/平台移动，支持触屏虚拟摇杆和键盘
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("移动设置")]
    public float moveSpeed = 5f;
    public Rigidbody2D rb;

    [Header("动画")]
    public Animator animator;
    public SpriteRenderer spriteRenderer;

    private Vector2 movement;
    private bool isMoving = false;

    void Start()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        // 加载存档位置
        var gm = GameManager.Instance;
        if (gm != null && gm.currentUserData != null)
        {
            transform.position = new Vector3(gm.currentUserData.posX, gm.currentUserData.posY, 0);
        }
    }

    void Update()
    {
        // 键盘输入
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");

        movement = new Vector2(moveX, moveY).normalized;
        isMoving = movement.magnitude > 0.1f;

        // 翻转朝向
        if (moveX > 0.1f)
        {
            spriteRenderer.flipX = false;
        }
        else if (moveX < -0.1f)
        {
            spriteRenderer.flipX = true;
        }

        // 动画
        if (animator != null)
        {
            animator.SetBool("IsMoving", isMoving);
            animator.SetFloat("MoveX", moveX);
            animator.SetFloat("MoveY", moveY);
        }
    }

    void FixedUpdate()
    {
        rb.velocity = movement * moveSpeed;
    }

    /// <summary>
    /// 虚拟摇杆输入 - 供 UI 按钮调用
    /// </summary>
    public void OnVirtualJoystickMove(Vector2 direction)
    {
        movement = direction.normalized;
        isMoving = direction.magnitude > 0.1f;
    }

    public void OnVirtualJoystickUp()
    {
        movement = Vector2.zero;
        isMoving = false;
    }
}