using UnityEngine;

/// <summary>
/// 玩家控制器 - 2D移动 + 攀爬梯子 + 水中减速
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("移动")]
    public float moveSpeed = 4f;
    public float waterSpeedMultiplier = 0.4f;  // 水中移动速度倍率
    public float climbSpeed = 2f;              // 攀爬速度

    [Header("组件")]
    public Rigidbody2D rb;
    public SpriteRenderer spriteRenderer;
    public Animator animator;

    [Header("状态")]
    public bool isClimbing = false;
    public bool isInWater = false;
    public bool isInMineshaft = false;

    private Vector2 movement;
    private WorldManager world;

    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        world = WorldManager.Instance;

        // 加载存档位置
        var gm = GameManager.Instance;
        if (gm != null && gm.currentUserData != null)
        {
            transform.position = new Vector3(gm.currentUserData.posX, gm.currentUserData.posY, 0);
        }
    }

    void Update()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");

        // 水中检测
        isInWater = world != null && transform.position.y < world.waterLevel;

        // 攀爬时只能上下移动
        if (isClimbing)
        {
            movement = new Vector2(0, moveY);
        }
        else
        {
            movement = new Vector2(moveX, moveY).normalized;
        }

        // 水中减速
        float speed = isInWater ? moveSpeed * waterSpeedMultiplier : moveSpeed;
        if (isClimbing) speed = climbSpeed;

        // 翻转
        if (moveX > 0.1f) spriteRenderer.flipX = false;
        else if (moveX < -0.1f) spriteRenderer.flipX = true;

        // 动画
        if (animator != null)
        {
            animator.SetBool("IsMoving", movement.magnitude > 0.1f);
            animator.SetBool("IsClimbing", isClimbing);
            animator.SetBool("IsInWater", isInWater);
        }
    }

    void FixedUpdate()
    {
        float speed = isInWater ? moveSpeed * waterSpeedMultiplier : moveSpeed;
        if (isClimbing) speed = climbSpeed;
        rb.velocity = movement * speed;

        // 水中上浮力
        if (isInWater && !isClimbing)
        {
            float buoyancy = 1.5f;
            rb.AddForce(Vector2.up * buoyancy, ForceMode2D.Force);
        }
    }

    /// <summary>
    /// D-Pad 移动输入
    /// </summary>
    public void OnDpadMove(Vector2 dir)
    {
        movement = dir.normalized;
    }

    public void OnDpadUp()
    {
        movement = Vector2.zero;
    }

    /// <summary>
    /// 进入矿井
    /// </summary>
    public void EnterMineshaft(Mineshaft shaft)
    {
        isInMineshaft = true;
        isClimbing = true;
        Vector3 targetPos = new Vector3(shaft.position.x, shaft.position.y - (shaft.depth * 10f), 0);
        transform.position = targetPos;
    }

    /// <summary>
    /// 离开矿井
    /// </summary>
    public void ExitMineshaft()
    {
        isInMineshaft = false;
        isClimbing = false;
    }
}