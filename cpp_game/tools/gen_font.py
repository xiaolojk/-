#!/usr/bin/env python3
"""Generate compact Chinese pixel bitmap font (1bpp encoding)."""
from PIL import Image, ImageDraw, ImageFont
import os

OUT = os.path.join(os.path.dirname(__file__), "..", "jni", "font_data.h")
FONT_PATH = "/tmp/cjk.ttf"
CHAR_SIZE = 16  # 16x16 pixels per character

TEXTS = [
    "蓝色迷海点击开始生存冒险精美版",
    "暂停继续重新开始退出设置",
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
]

chars = set()
for t in TEXTS:
    for c in t:
        chars.add(c)
chars = sorted(chars)
print(f"Total unique characters: {len(chars)}")

font = ImageFont.truetype(FONT_PATH, CHAR_SIZE - 2)

def render_char_bits(ch):
    img = Image.new('L', (CHAR_SIZE, CHAR_SIZE), 0)
    draw = ImageDraw.Draw(img)
    bbox = draw.textbbox((0, 0), ch, font=font)
    w = bbox[2] - bbox[0]
    h = bbox[3] - bbox[1]
    x = (CHAR_SIZE - w) // 2 - bbox[0]
    y = (CHAR_SIZE - h) // 2 - bbox[1]
    draw.text((x, y), ch, fill=255, font=font)
    px = img.load()
    bits = []
    for row in range(CHAR_SIZE):
        row_bits = 0
        for col in range(CHAR_SIZE):
            if px[col, row] > 80:
                row_bits |= (1 << (CHAR_SIZE - 1 - col))
        bits.append(row_bits)
    return bits

# Generate C++ header with 1bpp encoding
# Each char: 16 rows * 2 bytes = 32 bytes
lines = []
lines.append("// font_data.h - Compact Chinese pixel bitmap font (1bpp)")
lines.append("#pragma once")
lines.append("#include <cstdint>")
lines.append("")
lines.append(f"static const int FONT_CHAR_SIZE = {CHAR_SIZE};")
lines.append(f"static const int FONT_CHAR_COUNT = {len(chars)};")
lines.append(f"static const int FONT_BYTES_PER_CHAR = {CHAR_SIZE * 2};")
lines.append("")
lines.append("// Character lookup table: unicode codepoint -> font index")
lines.append("static const uint16_t FONT_UNICODES[FONT_CHAR_COUNT] = {")
for i in range(0, len(chars), 16):
    chunk = chars[i:i+16]
    hex_str = ", ".join(f"0x{ord(c):04X}" for c in chunk)
    lines.append(f"    {hex_str},")
lines.append("};")
lines.append("")
lines.append("// Font bitmap data: 16 rows x 16 bits per character (MSB=leftmost pixel)")
lines.append(f"static const uint16_t FONT_BITMAPS[FONT_CHAR_COUNT][{CHAR_SIZE}] = {{")
for ch in chars:
    bits = render_char_bits(ch)
    hex_str = ", ".join(f"0x{b:04X}" for b in bits)
    lines.append(f"    {{{hex_str}}},")
lines.append("};")
lines.append("")
lines.append("// Binary search lookup (FONT_UNICODES is sorted)")
lines.append("inline int fontLookup(uint16_t unicode) {")
lines.append("    int lo = 0, hi = FONT_CHAR_COUNT - 1;")
lines.append("    while (lo <= hi) {")
lines.append("        int mid = (lo + hi) / 2;")
lines.append("        if (FONT_UNICODES[mid] == unicode) return mid;")
lines.append("        if (FONT_UNICODES[mid] < unicode) lo = mid + 1;")
lines.append("        else hi = mid - 1;")
lines.append("    }")
lines.append("    return -1;")
lines.append("}")

with open(OUT, 'w') as f:
    f.write('\n'.join(lines))

total_bytes = len(chars) * CHAR_SIZE * 2 + len(chars) * 2
print(f"Generated {OUT}")
print(f"Chars: {len(chars)}, Data size: ~{total_bytes} bytes ({total_bytes/1024:.1f}KB)")
