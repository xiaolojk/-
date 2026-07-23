using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 故事线索系统 - 失忆主角在岛上发现线索拼凑真相
/// </summary>
public class StoryManager : MonoBehaviour
{
    public static StoryManager Instance { get; private set; }

    [Header("日记")]
    public List<StoryClue> foundClues = new List<StoryClue>();
    public List<StoryClue> allClues = new List<StoryClue>();

    [Header("主角状态")]
    public string playerMemoryFragment = "";   // 恢复的记忆碎片
    public int cluesFound = 0;
    public int totalClues = 15;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        GenerateClues();
    }

    void GenerateClues()
    {
        allClues = new List<StoryClue>
        {
            new StoryClue { clueId = "C01", title = "湿透的笔记", 
                content = "海水...在上涨...他们说那些反应堆...全都...来不及了...", type = StoryClue.ClueType.Note },
            new StoryClue { clueId = "C02", title = "无线电信号", 
                content = "嗞...这里是...嗞...幸存者...请...任何...收到...嗞...北纬30度...", type = StoryClue.ClueType.RadioSignal },
            new StoryClue { clueId = "C03", title = "破损的身份证", 
                content = "姓名：[模糊]...编号：ORG-7742...所属：欧米伽7号避难所...", type = StoryClue.ClueType.Wreckage },
            new StoryClue { clueId = "C04", title = "船体残骸", 
                content = "一艘大型船只的残骸...船身上写着'海神号'...", type = StoryClue.ClueType.Wreckage },
            new StoryClue { clueId = "C05", title = "录音带", 
                content = "（播放键）...我...是...谁...？不，等等，我记得...那个岛...核电站...", type = StoryClue.ClueType.Recording },
            new StoryClue { clueId = "C06", title = "墙上涂鸦", 
                content = "粗糙的画：一个圆形（太阳？）→ 融化 → 波浪 → 骷髅符号", type = StoryClue.ClueType.Graffiti },
            new StoryClue { clueId = "C07", title = "遗骸旁的日记", 
                content = "第47天...辐射病越来越严重...如果...找到高地...也许...", type = StoryClue.ClueType.Corpse },
            new StoryClue { clueId = "C08", title = "科学家的笔记", 
                content = "实验日志：核聚变后的海水具有极强的放射性...半衰期...至少300年...", type = StoryClue.ClueType.Note },
            new StoryClue { clueId = "C09", title = "求救信号", 
                content = "SOS...SOS...这里是'方舟'...坐标...嗞...我们...嗞...发动...", type = StoryClue.ClueType.RadioSignal },
            new StoryClue { clueId = "C10", title = "地图碎片", 
                content = "一张残破的世界地图...大部分陆地被涂成了蓝色...只有几个小点...", type = StoryClue.ClueType.Wreckage },
            new StoryClue { clueId = "C11", title = "你的日记本", 
                content = "你找到了自己的日记...第一页写着：'我叫...'然后被水浸透了...", type = StoryClue.ClueType.Note },
            new StoryClue { clueId = "C12", title = "避难所蓝图", 
                content = "Omega 7避难所...坐标：未知...入口：'在最大岛屿的...地下'", type = StoryClue.ClueType.Wreckage },
            new StoryClue { clueId = "C13", title = "孩子的画", 
                content = "稚嫩的蜡笔画：一家三口站在太阳下...旁边写着'妈妈会回来的'", type = StoryClue.ClueType.Graffiti },
            new StoryClue { clueId = "C14", title = "最后的广播", 
                content = "这里是...人类联合政府...最后一次广播...请所有幸存者...前往...高地...", type = StoryClue.ClueType.RadioSignal },
            new StoryClue { clueId = "C15", title = "真相碎片", 
                content = "你终于明白了...你曾是核电站的工程师...灾难发生那天...你试图阻止...", type = StoryClue.ClueType.Recording },
        };
    }

    public void FindClue(StoryClue clue)
    {
        if (clue.isFound) return;
        clue.isFound = true;
        foundClues.Add(clue);
        cluesFound++;
        Debug.Log($"发现线索: {clue.title}");
    }

    public StoryClue GetRandomUndiscoveredClue()
    {
        var undiscovered = allClues.FindAll(c => !c.isFound);
        if (undiscovered.Count == 0) return null;
        return undiscovered[Random.Range(0, undiscovered.Count)];
    }

    public string GetProgressSummary()
    {
        return $"记忆恢复: {cluesFound}/{totalClues}";
    }
}