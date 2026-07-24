#!/usr/bin/env python3
"""Generate 24x24 8bpp anti-aliased Chinese bitmap font from SimHei.ttf."""
from PIL import Image, ImageDraw, ImageFont
import os, re

OUT = os.path.join(os.path.dirname(__file__), "..", "jni", "font_data.h")
CHAR_SIZE = 32  # 32x32 pixels per character - high-res anti-aliased

TEXTS = [
    "蓝色迷海点击开始生存冒险精美版",
    "暂停继续重新开始退出设置按键位置",
    "生命饥饿口渴辐射线索挖掘中",
    "辐射水",
    "跳挖用背包制作菜单",
    "泥土石头木头木板木棍镐子火把篝火",
    "绷带浆果熟淡水纯铁金煤锭筏防服净器",
    "材料不足海有太远了面前没东西",
    "查看线索按住采集你死不能直接使用空",
    "纸条核电站泄漏收音机最后的广播金属牌冰川监测站",
    "日记平面上升米涂鸦寻找方舟计划字不要喝",
    "地图碎片地下避难报告变异样本失控信亲爱的",
    "最近陆地笔记反向渗透净化船日志第天蓝图矿井设计图",
    "军方文件操作镜子记住你是谁",
    "音效音乐画质震动高低开关",
    "虚拟按键点击返回量亮度",
    "出品已收集共个天夜获得消耗耐久距离",
    " Orgc Studio 0123456789",
    ":%/!.,+-?()[]",
    "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz",
    "拖拽按钮到想要的位置",
    "重置默认",
]

# Use SimHei for proper Chinese rendering
FONT_PATH = "/root/.pyenv/versions/3.14.4/lib/python3.14/site-packages/mplfonts/fonts/SimHei.ttf"

chars = set()
for t in TEXTS:
    for c in t:
        chars.add(c)
chars = sorted(chars)
print(f"Total unique characters: {len(chars)}")

font = ImageFont.truetype(FONT_PATH, CHAR_SIZE - 4)

def render_char_gray(ch):
    """Render character as 16x16 grayscale (8bpp) for anti-aliased display."""
    img = Image.new('L', (CHAR_SIZE, CHAR_SIZE), 0)
    draw = ImageDraw.Draw(img)
    bbox = draw.textbbox((0, 0), ch, font=font)
    w = bbox[2] - bbox[0]
    h = bbox[3] - bbox[1]
    x = (CHAR_SIZE - w) // 2 - bbox[0]
    y = (CHAR_SIZE - h) // 2 - bbox[1]
    draw.text((x, y), ch, fill=255, font=font)
    gray = []
    px = img.load()
    for row in range(CHAR_SIZE):
        for col in range(CHAR_SIZE):
            gray.append(px[col, row])
    return gray

# Generate C++ header with 8bpp encoding
lines = []
lines.append("// font_data.h - Anti-aliased Chinese bitmap font (8bpp, 32x32, SimHei)")
lines.append("#pragma once")
lines.append("#include <cstdint>")
lines.append("")
lines.append(f"static const int FONT_CHAR_SIZE = {CHAR_SIZE};")
lines.append(f"static const int FONT_CHAR_COUNT = {len(chars)};")
lines.append(f"static const int FONT_BYTES_PER_CHAR = {CHAR_SIZE * CHAR_SIZE};")
lines.append("")
lines.append("// Character lookup table: unicode codepoint -> font index")
lines.append("static const uint16_t FONT_UNICODES[FONT_CHAR_COUNT] = {")
for i in range(0, len(chars), 16):
    chunk = chars[i:i+16]
    hex_str = ", ".join(f"0x{ord(c):04X}" for c in chunk)
    lines.append(f"    {hex_str},")
lines.append("};")
lines.append("")
lines.append(f"// Font bitmap data: {CHAR_SIZE}x{CHAR_SIZE} grayscale pixels per character (8bpp anti-aliased)")
lines.append(f"static const uint8_t FONT_BITMAPS[FONT_CHAR_COUNT][{CHAR_SIZE*CHAR_SIZE}] = {{")
for ch in chars:
    gray = render_char_gray(ch)
    hex_str = ", ".join(f"0x{v:02X}" for v in gray)
    lines.append(f"    {{{hex_str}}},")
lines.append("};")
lines.append("")
lines.append("// Binary search lookup (FONT_UNICODES is sorted)")
lines.append("inline int fontLookup(uint16_t unicode) {")
lines.append("    int lo = 0, hi = FONT_CHAR_COUNT - 1;")
lines.append("    while (lo <= hi) {")
lines.append("    int mid = (lo + hi) / 2;")
lines.append("    if (FONT_UNICODES[mid] == unicode) return mid;")
lines.append("    if (FONT_UNICODES[mid] < unicode) lo = mid + 1;")
lines.append("    else hi = mid - 1;")
lines.append("    }")
lines.append("    return -1;")
lines.append("}")

with open(OUT, 'w') as f:
    f.write('\n'.join(lines))

total_bytes = len(chars) * CHAR_SIZE * CHAR_SIZE + len(chars) * 2
print(f"Generated {OUT}")
print(f"Chars: {len(chars)}, Data size: ~{total_bytes} bytes ({total_bytes/1024:.1f}KB)")