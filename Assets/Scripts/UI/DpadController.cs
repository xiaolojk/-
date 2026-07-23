using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// 屏幕虚拟方向键 - 手机端 D-Pad 控制
/// </summary>
public class DpadController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Header("D-Pad 按钮")]
    public Button btnUp;
    public Button btnDown;
    public Button btnLeft;
    public Button btnRight;
    public Button btnAction;    // A键 - 采集/交互
    public Button btnDig;       // 挖掘键

    private PlayerController player;
    private ResourceGathering gathering;
    private MiningSystem mining;

    private Vector2 currentDirection = Vector2.zero;

    void Start()
    {
        player = FindObjectOfType<PlayerController>();
        gathering = FindObjectOfType<ResourceGathering>();
        mining = FindObjectOfType<MiningSystem>();

        // 绑定方向键
        SetupDPadButton(btnUp, Vector2.up);
        SetupDPadButton(btnDown, Vector2.down);
        SetupDPadButton(btnLeft, Vector2.left);
        SetupDPadButton(btnRight, Vector2.right);

        // 动作键
        btnAction.onClick.AddListener(OnActionPress);
        btnDig.onClick.AddListener(OnDigPress);
    }

    void SetupDPadButton(Button btn, Vector2 dir)
    {
        var trigger = btn.gameObject.AddComponent<EventTrigger>();

        var down = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        down.callback.AddListener((_) => { currentDirection += dir; UpdatePlayerMove(); });
        trigger.triggers.Add(down);

        var up = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        up.callback.AddListener((_) => { currentDirection -= dir; UpdatePlayerMove(); });
        trigger.triggers.Add(up);
    }

    void UpdatePlayerMove()
    {
        if (player != null)
            player.OnDpadMove(currentDirection);
    }

    public void OnPointerDown(PointerEventData eventData) { }
    public void OnPointerUp(PointerEventData eventData) { }
    public void OnDrag(PointerEventData eventData) { }

    void OnActionPress()
    {
        // 采集/交互
        if (gathering != null)
        {
            // 根据位置判断采集什么
            if (player != null)
            {
                if (player.isInWater)
                    gathering.GatherResource(ResourceType.Water);
                else
                    gathering.GatherResource(ResourceType.Wood);
            }
        }
    }

    void OnDigPress()
    {
        if (mining != null)
            mining.TryDig();
    }
}