#!/usr/bin/env python3
"""生成像素风精灵图 - 迷茫蓝海版"""
import struct, zlib, os

def create_png(width, height, pixels):
    def chunk(chunk_type, data):
        c = chunk_type + data
        crc = struct.pack('>I', zlib.crc32(c) & 0xffffffff)
        return struct.pack('>I', len(data)) + c + crc
    header = b'\x89PNG\r\n\x1a\n'
    ihdr = chunk(b'IHDR', struct.pack('>IIBBBBB', width, height, 8, 6, 0, 0, 0))
    raw = b''
    for y in range(height):
        raw += b'\x00'
        for x in range(width):
            idx = (y * width + x) * 4
            raw += bytes(pixels[idx:idx+4])
    idat = chunk(b'IDAT', zlib.compress(raw))
    iend = chunk(b'IEND', b'')
    return header + ihdr + idat + iend

def rgba(r,g,b,a=255): return [r,g,b,a]
T = rgba(0,0,0,0)

def draw(pixels, s, data, color_map):
    for y, row in enumerate(data):
        for x, ch in enumerate(row):
            if ch in color_map:
                idx = (y * s + x) * 4
                pixels[idx:idx+4] = color_map[ch]

# ======== 玩家 (32x32) ========
def make_player():
    s = 32
    pixels = [0]*(s*s*4)
    cm = {
        'H': rgba(139,90,43), 'S': rgba(100,120,100), 'P': rgba(50,50,130),
        'B': rgba(0,0,0), 'W': rgba(255,255,255), 'K': rgba(255,200,150),
        'R': rgba(200,50,50),  # 红色围巾（失忆标志）
    }
    data = [
        "...................TTTT...................",
        "................TTTTTTTTTTTT...............",
        ".............TTTTTTTTTTTTTTTT..............",
        "............TTTTTTTTTTTTTTTTTT.............",
        "...........TTTTTTTTTTTTTTTTTTTT............",
        "..........TTTTTTTTTTTTTTTTTTTTTT...........",
        "..........TTTTTWWTTTTWWTTTTTTTTT...........",
        "..........TTTTTWWTTTTWWTTTTTTTTT...........",
        "..........TTTTTTTTTTTTTTTTTTTTTT...........",
        "..........TTTTTTTTTTTTTTTTTTTTTT...........",
        "..........TTTTTTBBBBBBBBTTTTTTTT...........",
        ".........RRRRRRBBBBBBBBRRRRRRR.............",
        ".........RRRRRRRRRRRRRRRRRRRRR.............",
        "...........SSSSSSSSSSSSSSSSSSS.............",
        "...........SSSSSSSSSSSSSSSSSSS.............",
        "...........SSSSSSSSSSSSSSSSSSS.............",
        "...........SSSSSSSSSSSSSSSSSSS.............",
        "...........SSSSSSSSSSSSSSSSSSS.............",
        "...........SSSSSSSSSSSSSSSSSSS.............",
        "...........SSSSSSSSSSSSSSSSSSS.............",
        "............PPPSSSSSSSSSSSPPP..............",
        "............PPPSSSSSSSSSSSPPP..............",
        "............PPPPPSSSSSSSPPPPP..............",
        ".............PPPPSSSSSSSPPPP...............",
        ".............PPPPPPSSSPPPPPP...............",
        "..............PPPPPPPPPPPPP................",
        "..............PPPPPPPPPPPPP................",
        "..............PPPPPPPPPPPPP................",
        "..............PPPPPPPPPPPPP................",
        ".............BBBBBBBBBBBBBB................",
        "............BBBBBBBBBBBBBBBB...............",
        "...........BBBBBBBBBBBBBBBBBB..............",
    ]
    draw(pixels, s, data, cm)
    return pixels

# ======== 放射性海水 tile (16x16) ========
def make_water_danger():
    s = 16
    pixels = [0]*(s*s*4)
    cm = {
        'B': rgba(20,80,140), 'D': rgba(10,40,80), 'G': rgba(0,200,100,60),
        'L': rgba(40,120,200), 'R': rgba(200,40,0,40),
    }
    data = [
        "BBBBBBBBBBBBBBBB",
        "BBBBBBBDDBBBBBDB",
        "BBBBBBBDDBBBBBDB",
        "BBBBDBBBBBBBBBBB",
        "BBBBBBBBDBBBBBBB",
        "BBBBBBBBBBDBBBBB",
        "BBDBBBBBBBBBBBBB",
        "BBBBBBBBBBBBDBBB",
        "BBBBBBDBBBBBBBBB",
        "BBBBBBBBBBBBBBDB",
        "BBBBBBBBDBBBBBBB",
        "BBDBBBBBBBBBBBBB",
        "BBBBBBBBBBDBBBBB",
        "BBBBBBDBBBBBBBBB",
        "BBBBBBBBDBBBBBBB",
        "BBBBBBBBBBBBBBBB",
    ]
    draw(pixels, s, data, cm)
    return pixels

# ======== 梯子 (16x32) ========
def make_ladder():
    w, h = 16, 32
    pixels = [0]*(w*h*4)
    cm = {'W': rgba(139,90,43), 'R': rgba(100,60,20)}
    for y in range(h):
        for x in range(w):
            idx = (y * w + x) * 4
            if x < 3 or x >= w-3:
                pixels[idx:idx+4] = cm['W']
            elif y % 6 < 2:
                pixels[idx:idx+4] = cm['R']
    return pixels, w, h

# ======== 铲子 (32x32) ========
def make_shovel():
    s = 32
    pixels = [0]*(s*s*4)
    cm = {'W': rgba(139,90,43), 'G': rgba(128,128,128), 'D': rgba(80,80,80)}
    data = [
        ".............GGGGGGGG..................",
        ".............GGGGGGGGGG................",
        ".............GGGGGGGGGG................",
        ".............GGGGGGGGGG................",
        ".............GGGGGGGGGG................",
        "..............GGGGGGGG.................",
        "..............GGGGGGGG.................",
        "...............GGGGGG..................",
        "...............GGGGGG..................",
        "................WWWW...................",
        "................WWWW...................",
        "................WWWW...................",
        "................WWWW...................",
        "................WWWW...................",
        "................WWWW...................",
        "................WWWW...................",
        "................WWWW...................",
        "................WWWW...................",
        "................WWWW...................",
        "................WWWW...................",
        "................WWWW...................",
        "................WWWW...................",
        "................WWWW...................",
        "................WWWW...................",
        "................WWWW...................",
        "................WWWW...................",
        "...............DDDDDD...................",
        "..............DDDDDDDD..................",
        ".............DDDDDDDDDD.................",
        "............DDDDDDDDDDDD................",
        "...........DDDDDDDDDDDDDD...............",
        "..........DDDDDDDDDDDDDDDD..............",
    ]
    draw(pixels, s, data, cm)
    return pixels

# ======== 镐子 (32x32) ========
def make_pickaxe():
    s = 32
    pixels = [0]*(s*s*4)
    cm = {'W': rgba(139,90,43), 'G': rgba(128,128,128), 'D': rgba(80,80,80)}
    data = [
        "...........GGGGGGGGGGGGGGGG............",
        "..........GGGGGGGGGGGGGGGGGG...........",
        ".........GGGGGGGGGGGGGGGGGGGG..........",
        "..........GGGGGGGGGGGGGGGGGG...........",
        "...........GGGGGGGGGGGGGGGG............",
        "............GGGGGGGGGGGGGG.............",
        ".............GGGGGGGGGGGG..............",
        "..............GGGGGGGGGG...............",
        "...............GGGGGGGG................",
        "................WWWW...................",
        "................WWWW...................",
        "................WWWW...................",
        "................WWWW...................",
        "................WWWW...................",
        "................WWWW...................",
        "................WWWW...................",
        "................WWWW...................",
        "................WWWW...................",
        "................WWWW...................",
        "................WWWW...................",
        "................WWWW...................",
        "................WWWW...................",
        "................WWWW...................",
        "................WWWW...................",
        "................WWWW...................",
        "................WWWW...................",
        "...............DDDDDD...................",
        "..............DDDDDDDD..................",
        ".............DDDDDDDDDD.................",
        "............DDDDDDDDDDDD................",
        "...........DDDDDDDDDDDDDD...............",
        "..........DDDDDDDDDDDDDDDD..............",
    ]
    draw(pixels, s, data, cm)
    return pixels

# ======== 洞穴入口 (32x32) ========
def make_cave_entrance():
    s = 32
    pixels = [0]*(s*s*4)
    cm = {
        'S': rgba(100,100,100), 'D': rgba(60,60,60),
        'B': rgba(0,0,0), 'G': rgba(80,120,60),
    }
    data = [
        "GGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGG",
        "GGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGG",
        "GGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGG",
        "GGGGGGGGGGGGGSSSSGGGGGGGGGGGGGGGGGGGGG",
        "GGGGGGGGGGGSSSSSSSSGGGGGGGGGGGGGGGGGGG",
        "GGGGGGGGGGSSSSSSSSSSGGGGGGGGGGGGGGGGGG",
        "GGGGGGGGGSSSSSSSSSSSSGGGGGGGGGGGGGGGGG",
        "GGGGGGGGSSSSSSSSSSSSSSGGGGGGGGGGGGGGGG",
        "GGGGGGGSSSSSSSSSSSSSSSSGGGGGGGGGGGGGGG",
        "GGGGGGSSSSSSSSSSSSSSSSSSGGGGGGGGGGGGGG",
        "GGGGGGSSSSSSSSSSSSSSSSSSGGGGGGGGGGGGGG",
        "GGGGGSSSSSSSSSSSSSSSSSSSSGGGGGGGGGGGGG",
        "GGGGGSSSSSSSSSSSSSSSSSSSSGGGGGGGGGGGGG",
        "GGGGGSSSSSSSSSSSSSSSSSSSSGGGGGGGGGGGGG",
        "GGGGGSSSSSSSSSSSSSSSSSSSSGGGGGGGGGGGGG",
        "GGGGGGSSSSSSSSSSSSSSSSSSGGGGGGGGGGGGGG",
        "GGGGGGSSSSSSSSSSSSSSSSSSGGGGGGGGGGGGGG",
        "GGGGGGGSSSSSSSSSSSSSSSSGGGGGGGGGGGGGGG",
        "GGGGGGGGSSSSSSSSSSSSSSGGGGGGGGGGGGGGGG",
        "GGGGGGGGGSSSSSSSSSSSSGGGGGGGGGGGGGGGGG",
        "GGGGGGGGGGSSSSSSSSSSGGGGGGGGGGGGGGGGGG",
        "GGGGGGGGGGGSSSSSSSSGGGGGGGGGGGGGGGGGGG",
        "GGGGGGGGGGGGSSSSSSGGGGGGGGGGGGGGGGGGGG",
        "GGGGGGGGGGGGGSSSSGGGGGGGGGGGGGGGGGGGGG",
        "GGGGGGGGGGGGGGSSGGGGGGGGGGGGGGGGGGGGGG",
        "GGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGG",
        "GGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGG",
        "GGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGG",
        "GGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGG",
        "GGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGG",
        "GGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGG",
        "GGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGG",
    ]
    draw(pixels, s, data, cm)
    return pixels

# ======== 地下泥土 (16x16) ========
def make_dirt_ug():
    s = 16
    pixels = [0]*(s*s*4)
    cm = {'D': rgba(80,50,30), 'E': rgba(60,35,20), 'S': rgba(100,70,40)}
    for y in range(s):
        for x in range(s):
            idx = (y*s+x)*4
            if (x+y)%3==0: pixels[idx:idx+4]=cm['E']
            elif (x+y)%5==0: pixels[idx:idx+4]=cm['S']
            else: pixels[idx:idx+4]=cm['D']
    return pixels

# ======== 地下石头 (16x16) ========
def make_stone_ug():
    s = 16
    pixels = [0]*(s*s*4)
    cm = {'G': rgba(100,100,100), 'D': rgba(70,70,70), 'L': rgba(130,130,130)}
    for y in range(s):
        for x in range(s):
            idx = (y*s+x)*4
            if (x+y)%4==0: pixels[idx:idx+4]=cm['D']
            elif (x+y)%7==0: pixels[idx:idx+4]=cm['L']
            else: pixels[idx:idx+4]=cm['G']
    return pixels

# ======== 线索笔记 (16x16) ========
def make_clue_note():
    s = 16
    pixels = [0]*(s*s*4)
    cm = {'W': rgba(240,230,200), 'Y': rgba(200,180,100), 'B': rgba(50,50,50)}
    data = [
        "WWWWWWWWWWWWWWWW",
        "WWWWWWWWWWWWWWWW",
        "WWWWWWWWWWWWWWWW",
        "WWWWWWWWWWWWWWWW",
        "WWBBWWWWWWWWBBWW",
        "WWBBWWWWWWWWBBWW",
        "WWBBWWWWWWWWBBWW",
        "WWBBWWWWWWWWBBWW",
        "WWWWWWWWWWWWWWWW",
        "WWWWWWWWWWWWWWWW",
        "WWWWWWWWWWWWWWWW",
        "WWWWWWWWWWWWWWWW",
        "WWWWWWWWWWWWWWWW",
        "YYYYYYYYYYYYYYYY",
        "YYYYYYYYYYYYYYYY",
        "YYYYYYYYYYYYYYYY",
    ]
    draw(pixels, s, data, cm)
    return pixels

# ======== 岛屿 tile (16x16) ========
def make_island_tile():
    s = 16
    pixels = [0]*(s*s*4)
    cm = {'G': rgba(100,180,80), 'D': rgba(85,160,65), 'S': rgba(180,160,120), 'L': rgba(115,190,95)}
    for y in range(s):
        for x in range(s):
            idx = (y*s+x)*4
            if y < 4: pixels[idx:idx+4]=cm['G']
            elif y < 6: pixels[idx:idx+4]=cm['L']
            else: pixels[idx:idx+4]=cm['S']
    return pixels

# ======== 加载画面 Logo (128x64) ========
def make_loading_logo():
    w, h = 128, 64
    pixels = [0]*(w*h*4)
    BG = rgba(5,5,15,255)
    GR = rgba(40,180,40,255)
    WT = rgba(220,255,220,255)
    for y in range(h):
        for x in range(w):
            idx = (y*w+x)*4
            pixels[idx:idx+4] = BG
    # 画 "Orgc" 大字
    # O
    for y in range(8, 56):
        for x in range(8, 28):
            if (x-18)**2 + (y-32)**2 < 100 and (x-18)**2 + (y-32)**2 > 36:
                idx = (y*w+x)*4
                pixels[idx:idx+4] = GR
    # r
    for y in range(12, 52):
        for x in range(32, 44):
            idx = (y*w+x)*4
            if x < 36 or y < 22:
                pixels[idx:idx+4] = GR
    # g
    for y in range(20, 52):
        for x in range(48, 66):
            if (x-57)**2 + (y-36)**2 < 100 and not ((x-57)**2 + (y-36)**2 < 36 and y < 36):
                idx = (y*w+x)*4
                pixels[idx:idx+4] = GR
    # c
    for y in range(16, 48):
        for x in range(70, 90):
            if (x-80)**2 + (y-32)**2 < 100 and (x-80)**2 + (y-32)**2 > 36 and x > 80:
                idx = (y*w+x)*4
                pixels[idx:idx+4] = GR
    return pixels, w, h

# ======== 生成 ========
out = "/workspace/PixelSurvival/Assets/Sprites"
os.makedirs(out, exist_ok=True)

sprites = [
    ("player", make_player(), 32, 32),
    ("water_danger", make_water_danger(), 16, 16),
    ("ladder", *make_ladder()),
    ("shovel", make_shovel(), 32, 32),
    ("pickaxe", make_pickaxe(), 32, 32),
    ("cave_entrance", make_cave_entrance(), 32, 32),
    ("dirt_ug", make_dirt_ug(), 16, 16),
    ("stone_ug", make_stone_ug(), 16, 16),
    ("clue_note", make_clue_note(), 16, 16),
    ("island_tile", make_island_tile(), 16, 16),
    ("loading_logo", *make_loading_logo()),
]

for name, pixels, w, h in sprites:
    png = create_png(w, h, pixels)
    path = os.path.join(out, f"{name}.png")
    with open(path, 'wb') as f:
        f.write(png)
    print(f"Created: {name}.png ({w}x{h})")

print(f"\nDone! {len(sprites)} sprites.")