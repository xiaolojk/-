#!/usr/bin/env python3
"""Generate all assets for Lost Blue Sea APK - fixed version for Android 10 compatibility."""

import os, struct, math, random
import wave as wave_module
from PIL import Image, ImageDraw

OUT_DIR = "/workspace/apk_build/assets/public"
RES_DIR = "/workspace/apk_build/res"

os.makedirs(OUT_DIR, exist_ok=True)

# ============================================================
# COLOR PALETTE
# ============================================================
C = {
    '.': None, 'k': '#000000', 'K': '#1a1a2e', 'w': '#ffffff', 'W': '#e0e0e0',
    'r': '#e53935', 'R': '#c62828', 'b': '#1565c0', 'B': '#0d47a1',
    'g': '#4caf50', 'G': '#2e7d32', 'y': '#ffeb3b', 'Y': '#f9a825',
    'o': '#ff9800', 'O': '#e65100', 'p': '#9c27b0', 'P': '#6a1b9a',
    'c': '#00bcd4', 'C': '#00838f', 't': '#795548', 'T': '#4e342e',
    's': '#78909c', 'S': '#455a64', 'd': '#bcaaa4', 'D': '#8d6e63',
    'n': '#ffcc80', 'N': '#ffab40', 'e': '#ff8a65', 'E': '#ff5722',
    'a': '#81d4fa', 'A': '#4fc3f7', 'l': '#aed581', 'L': '#7cb342',
    'm': '#ce93d8', 'M': '#ab47bc', 'h': '#90a4ae', 'H': '#607d8b',
    '1': '#1b5e20', '2': '#33691e', '3': '#827717', '4': '#e65100',
    '5': '#bf360c', '6': '#3e2723', '7': '#263238', '8': '#1a237e',
    '9': '#4a148c', '0': '#880e4f', 'f': '#ff5252', 'z': '#00e676',
    'x': '#ffd740', 'v': '#7c4dff', 'j': '#69f0ae', 'q': '#40c4ff',
}

def create_pixel_image(data, colors, scale=1):
    h, w = len(data), max(len(row) for row in data)
    temp = Image.new('RGBA', (w, h), (0, 0, 0, 0))
    draw = ImageDraw.Draw(temp)
    for y, row in enumerate(data):
        for x, ch in enumerate(row):
            if ch in colors and colors[ch]:
                draw.point((x, y), fill=colors[ch])
    if scale > 1:
        return temp.resize((w * scale, h * scale), Image.NEAREST)
    return temp

def save_sprite(data, filename, scale=2):
    img = create_pixel_image(data, C, scale)
    path = os.path.join(OUT_DIR, filename)
    img.save(path, 'PNG')
    return path

# ============================================================
# APP ICON (32x32 base)
# ============================================================
APP_ICON = [
    'bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb',
    'bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb',
    'bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb',
    'bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb',
    'bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb',
    'bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb',
    'bbbbbbbbbbLLggggggLLbbbbbbbbbbbb',
    'bbbbbbbbLLggggggggggLLbbbbbbbbbb',
    'bbbbbbLLggggggggggggggLLbbbbbbbb',
    'bbbbbLLggggggggggggggggLLbbbbbbb',
    'bbbbLLggggggggggggggggggLLbbbbbb',
    'bbbLLggggggggggggggggggggLLbbbbb',
    'bbLLggggggggLLggggLLggggggLLbbbb',
    'bLLggggggggLLggggggLLggggggLLbbb',
    'LLggggggggggggggggggggggggggLLbb',
    'LLggggggggggggggggggggggggggLLbb',
    'LLggggggggggggggggggggggggggLLbb',
    'LLggggggggggggggggggggggggggLLbb',
    'LLggggggggggggggggggggggggggLLbb',
    'LLggggggggggggggggggggggggggLLbb',
    'LLggggggggggggggggggggggggggLLbb',
    'LLggggggggggggggggggggggggggLLbb',
    'LLggggggggggggggggggggggggggLLbb',
    'LLggggggggggggggggggggggggggLLbb',
    'LLggggggggggggggggggggggggggLLbb',
    'LLggggggggggggggggggggggggggLLbb',
    'LLggggggggggggggggggggggggggLLbb',
    'LLggggggggggggggggggggggggggLLbb',
    'LLggggggggggggggggggggggggggLLbb',
    'LLggggggggggggggggggggggggggLLbb',
    'LLggggggggggggggggggggggggggLLbb',
    'LLggggggggggggggggggggggggggLLbb',
]

# Generate icons at all densities as PNG (no adaptive XML)
DENSITIES = {
    'mdpi': 48, 'hdpi': 72, 'xhdpi': 96, 'xxhdpi': 144, 'xxxhdpi': 192,
}
base_icon = create_pixel_image(APP_ICON, C, 1)
for density, size in DENSITIES.items():
    d = os.path.join(RES_DIR, f'mipmap-{density}')
    os.makedirs(d, exist_ok=True)
    scaled = base_icon.resize((size, size), Image.NEAREST)
    scaled.save(os.path.join(d, 'ic_launcher.png'), 'PNG')
    print(f"  mipmap-{density}: {size}x{size}")

# Also create drawable dirs for splash
for density in DENSITIES:
    os.makedirs(os.path.join(RES_DIR, f'drawable-{density}'), exist_ok=True)

# Remove anydpi-v26 if it exists (causes issues)
import shutil
anydpi = os.path.join(RES_DIR, 'mipmap-anydpi-v26')
if os.path.exists(anydpi):
    shutil.rmtree(anydpi)
    print("  Removed mipmap-anydpi-v26 (causes parse errors on some devices)")

# ============================================================
# GAME SPRITES
# ============================================================
print("\nGenerating sprites...")

PLAYER = [
    '......kkkkkk......','....kkkkkkkkk....','...kkkkkkkkkkk...','..kkkkkkkkkkkkk..',
    '..kkkkkkkkkkkkk..','..kkkk....kkkkk..','..kkk......kkkk..','..kkk......kkk...',
    '...kkk....kkk....','....kkkkkkk.....','....kkkkkk......','....kkkkkkk.....',
    '...kkkkkkkkk....','..kkkkkkkkkkk...','..kkkk....kkkk..','..kkkk....kkkk..',
    '..kkkk....kkkk..','...kkk....kkk...','...kkk....kkk...','....kkk..kkk....',
    '....kkkkkkk.....','.....kkkkkk.....','.....kkkkkk.....','.....kkw.wkk....',
    '....kkkeeekk....','...kkkkeeeeekk...','..kkkkkkeeekk...','..kkkkkkkkkkk...',
    '..kkkkkkkkkkk...','..kkkkbbbbkkk...','..kkkkbbbbkkk...','...kkkkkkkkk....',
]
save_sprite(PLAYER, 'player.png')

# Tile sprites
GRASS = ['gggggggggggggggggggggggggggggggg'] * 32
for i in range(3, 29):
    GRASS[i] = 'g'*32
    if i % 5 == 0:
        GRASS[i] = 'g'*10 + 'GGG' + 'g'*19
save_sprite(GRASS, 'grass_tile.png')

DIRT = ['TTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTT'] * 32
for i in range(3, 29):
    if i % 4 == 0:
        DIRT[i] = 'T'*4 + 't'*6 + 'T'*22
save_sprite(DIRT, 'dirt_tile.png')

STONE = ['ssssssssssssssssssssssssssssssss'] * 32
for i in range(4, 28):
    if i % 6 < 3:
        STONE[i] = 's'*4 + 'S'*10 + 's'*18
save_sprite(STONE, 'stone_tile.png')

WATER = ['bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb'] * 32
for i in range(4, 28):
    if i % 6 < 3:
        WATER[i] = 'b'*4 + 'B'*10 + 'b'*18
save_sprite(WATER, 'water_tile.png')

TREE = ['gggggggggggggggggggggggggggggggg'] * 32
for i in range(2, 12):
    t = 'g' * (16 - i) + 'G' * (i * 2) + 'g' * (16 - i)
    TREE[i] = t[:32]
for i in range(12, 32):
    TREE[i] = 'g'*12 + 'TTTTTTTT' + 'g'*12
save_sprite(TREE, 'tree_tile.png')

CAVE = ['gggggggggggggggggggggggggggggggg'] * 32
for i in range(8, 24):
    w = 2 + (i - 8) * 2 if i < 16 else 2 + (24 - i) * 2
    CAVE[i] = 'g'*((32-w)//2) + 'k'*w + 'g'*((32-w)//2)
save_sprite(CAVE, 'cave_entrance.png')

# Item sprites
PICKAXE = ['.'*32]*32
for i in range(8, 20):
    PICKAXE[i] = '.'*(8+i) + 's'*4 + '.'*(20-i)
for i in range(20, 32):
    PICKAXE[i] = '.'*9 + 't'*14 + '.'*9
save_sprite(PICKAXE, 'pickaxe.png')

SHOVEL = ['.'*32]*32
for i in range(4, 18):
    SHOVEL[i] = '.'*(4+i) + 't'*4 + '.'*(24-i)
for i in range(18, 32):
    SHOVEL[i] = '.'*9 + 's'*14 + '.'*9
save_sprite(SHOVEL, 'shovel.png')

LADDER = ['tttttttttttttttttttttttttttttttt'] * 32
for i in range(2, 30):
    if i % 4 < 2:
        LADDER[i] = 'tt..tttt..tttt..tttt..tttt..tttt'
save_sprite(LADDER, 'ladder.png')

CLUE = ['.'*32]*32
for i in range(1, 31):
    CLUE[i] = '..' + 'y'*28 + '..'
for i in range(10, 24):
    CLUE[i] = '..' + 'y'*6 + 'www' + 'y'*10 + 'www' + 'y'*6 + '..'
for i in range(13, 21):
    CLUE[i] = '..' + 'y'*8 + 'kkkkkkkk' + 'y'*12 + '..'
save_sprite(CLUE, 'clue_note.png')

# Ore sprites
ORE = ['.'*32]*32
for i in range(4, 28):
    bg = 's'*32
    if 6 <= i <= 26:
        w = min(28, 4 + (i-6)*2) if i <= 16 else min(28, 4 + (26-i)*2)
        bg = 's'*((32-w)//2) + 'S'*w + 's'*((32-w)//2)
    ORE[i] = bg[:32]
save_sprite(ORE, 'ore_vein.png')

GEM = [r[:] for r in ORE]
for i in range(8, 24):
    GEM[i] = GEM[i].replace('S', 'v')
save_sprite(GEM, 'gem_ore.png')

COAL = [r[:] for r in ORE]
for i in range(8, 24):
    COAL[i] = COAL[i].replace('S', 'k')
save_sprite(COAL, 'coal_ore.png')

IRON = [r[:] for r in ORE]
for i in range(8, 24):
    IRON[i] = IRON[i].replace('S', 'r')
save_sprite(IRON, 'iron_ore.png')

GOLD = [r[:] for r in ORE]
for i in range(8, 24):
    GOLD[i] = GOLD[i].replace('S', 'x')
save_sprite(GOLD, 'gold_ore.png')

# Other items
RAFT = ['.'*32]*32
for i in range(4, 28):
    RAFT[i] = '.'*3 + 't'*26 + '.'*3
save_sprite(RAFT, 'raft.png')

RADSUIT = ['.'*32]*32
for i in range(3, 29):
    RADSUIT[i] = '.'*9 + 'y'*14 + '.'*9
save_sprite(RADSUIT, 'rad_suit.png')

CAMPFIRE = ['.'*32]*32
for i in range(3, 15):
    CAMPFIRE[i] = '.'*(16-i) + 'r'*(i*2-5) + 'o'*(i*2-5) + 'r'*(i*2-5) + '.'*(16-i)
for i in range(15, 22):
    CAMPFIRE[i] = '.'*12 + 'r'*8 + '.'*12
for i in range(22, 32):
    CAMPFIRE[i] = '.'*(24-i) + 't'*(i*2-16) + '.'*(24-i)
save_sprite(CAMPFIRE, 'campfire.png')

WATER_FILTER = ['.'*32]*32
for i in range(3, 12):
    WATER_FILTER[i] = '.'*(16-i) + 's'*(i*2-5) + '.'*(16-i)
for i in range(12, 20):
    WATER_FILTER[i] = '.'*13 + 's'*6 + '.'*13
for i in range(20, 32):
    WATER_FILTER[i] = '.'*(24-i) + 'a'*(i*2-16) + '.'*(24-i)
save_sprite(WATER_FILTER, 'water_filter.png')

BANDAGE = ['.'*32]*32
for i in range(4, 28):
    BANDAGE[i] = '.'*4 + 'w'*24 + '.'*4
save_sprite(BANDAGE, 'bandage.png')

PLANK = ['.'*32]*32
for i in range(4, 28):
    PLANK[i] = '.'*3 + 't'*26 + '.'*3
save_sprite(PLANK, 'wood_plank.png')

# Underground
UG_DIRT = ['1'*32] * 32
save_sprite(UG_DIRT, 'dirt_ug.png')

UG_STONE = ['s'*32] * 32
for i in range(4, 28):
    if i % 6 < 3:
        UG_STONE[i] = 's'*4 + 'S'*10 + 's'*18
save_sprite(UG_STONE, 'stone_ug.png')

# Monsters
RAT = ['.'*32]*32
for i in range(4, 10):
    RAT[i] = '.'*(12-i) + 'k'*(i*2) + '.'*(12-i)
for i in range(10, 28):
    RAT[i] = '.'*10 + 'k'*12 + '.'*10
save_sprite(RAT, 'monster_rat.png')

CRAB = ['.'*32]*32
for i in range(3, 6):
    CRAB[i] = '.'*(11-i) + 'r'*i + '..' + 'r'*i + '.'*(11-i)
for i in range(6, 20):
    CRAB[i] = '.'*7 + 'r'*18 + '.'*7
for i in range(20, 23):
    CRAB[i] = '.'*(23-i) + 'r'*i + '..' + 'r'*i + '.'*(23-i)
save_sprite(CRAB, 'monster_crab.png')

# Icons
HEALTH = ['.'*24]*24
for i in range(1, 23):
    if 1 <= i <= 4 or 19 <= i <= 22:
        w = max(0, 8 - abs(12 - i) * 2)
        HEALTH[i] = '.'*(12-w//2) + 'r'*w + '.'*(12-w//2)
    else:
        HEALTH[i] = '.'*4 + 'r'*16 + '.'*4
save_sprite(HEALTH, 'icon_health.png')

HUNGER = ['.'*24]*24
for i in range(2, 22):
    HUNGER[i] = '.'*(12-i//2) + 'o'*i + '.'*(12-i//2)
save_sprite(HUNGER, 'icon_hunger.png')

RAD = ['.'*18]*18
for i in range(2, 16):
    RAD[i] = '.'*(9-i//2) + 'y'*i + '.'*(9-i//2)
save_sprite(RAD, 'icon_radiation.png')

# Backgrounds
for n in range(3):
    bg = ['.'*32]*32
    for y in range(32):
        row = ''
        for x in range(32):
            row += '7' if (x+y+n*7) % 5 == 0 else '.'
        bg[y] = row
    save_sprite(bg, f'bg_underground_{n}.png')

# Water danger
WD = ['.'*32]*32
for i in range(4, 28):
    w = min(28, i*2-8) if i <= 16 else min(28, 56-i*2)
    WD[i] = '.'*((32-w)//2) + 'r'*w + '.'*((32-w)//2)
for i in range(10, 22):
    w2 = min(20, i*2-20) if i <= 16 else min(20, 44-i*2)
    WD[i] = WD[i][:(32-w2)//2] + 'w'*w2 + WD[i][(32+w2)//2:]
save_sprite(WD, 'water_danger.png')

# Logo
LOGO = ['.'*64]*64
# "LOST BLUE" text
for i in range(8, 40):
    LOGO[i] = 'A'*64
LOGO[8] = '..AA..AA...AA...AAA...AAA...AAA...AAA......AAA...AA...AA.....'
LOGO[9] = '..AA..AA..AAAA..AAAA..AAAA..AAAA..AAAA.....AA...AA...AA.....'
LOGO[10]= '..AA..AA.AA..AA.AA..AA.AA....AA....AA..AA....AA..AA...AA.....'
LOGO[11]= '..AA..AA.AA..AA.AA..AA.AA....AA....AA..AA....AA..AA...AA.....'
LOGO[12]= '..AA..AA.AA..AA.AA..AA.AA....AA....AA..AA....AA..AA...AA.....'
LOGO[13]= '..AAAAAA.AAAAAA.AA..AA.AA....AA....AA..AA....AA..AAAAAAA.....'
LOGO[14]= '..AAAAAA.AAAAAA.AA..AA.AA....AA....AA..AA....AA..AAAAAAA.....'
LOGO[15]= '..AA..AA.AA..AA.AA..AA.AA....AA....AA..AA....AA..AA...AA.....'
LOGO[16]= '..AA..AA.AA..AA.AA..AA.AA....AA....AA..AA....AA..AA...AA.....'
LOGO[17]= '..AA..AA.AA..AA.AA..AA.AA....AA....AA..AA....AA..AA...AA.....'
LOGO[18]= '..AA..AA.AA..AA.AA..AA.AA....AA....AA..AA....AA..AA...AA.....'
LOGO[19]= '..AA..AA.AA..AA.AA..AA.AA....AA....AA..AA....AA..AA...AA.....'
LOGO[20]= '..AA..AA.AA..AA.AA..AA.AA....AA....AA..AA....AA..AA...AA.....'
LOGO[21]= '..AA..AA.AA..AA.AA..AA.AA. AAAA..AA..AA..AA.....AA..AA...AA.....'
LOGO[22]= '..AA..AA.AA..AA.AA..AA.AA..AAAA..AAAA..AAAA......AA..AA...AA.....'
LOGO[23]= '..AA..AA.AA..AA.AA..AA.AA...AAA...AA....AAA......AA..AA...AA.....'
# "ORGc" text
for i in range(42, 53):
    LOGO[i] = 'A'*64
LOGO[42] = '.....AAAAAAAA......AAAAAAAA....AA......AA...AAAAA...AA..AA......'
LOGO[43] = '.....AAAAAAAA......AAAAAAAA....AA......AA..AAAAAAA..AA..AA......'
LOGO[44] = '.....AA..AA........AA..AA......AA......AA..AA...AA..AA..AA......'
LOGO[45] = '.....AA..AA........AA..AA......AA......AA..AA...AA..AA..AA......'
LOGO[46] = '.....AA..AA........AA..AA......AA......AA..AA...AA..AA..AA......'
LOGO[47] = '.....AA..AA........AA..AA......AA......AA..AA...AA..AA..AA......'
LOGO[48] = '.....AAAAAA........AAAAAA......AA......AA..AAAAAAA..AAAAAA......'
LOGO[49] = '.....AAAAAA........AAAAAA......AA......AA..AAAAAAA..AAAAAA......'
LOGO[50] = '.....AA..AA........AA..AA......AA......AA..AA...AA..AA..AA......'
LOGO[51] = '.....AA..AA........AA..AA......AA......AA..AA...AA..AA..AA......'
LOGO[52] = '.....AA..AA........AA..AA......AA......AA..AA...AA..AA..AA......'
LOGO[53] = '.....AA..AA........AA..AA......AA......AA..AA...AA..AA..AA......'
LOGO[54] = '.....AA..AA........AA..AA......AA......AA..AA...AA..AA..AA......'
LOGO[55] = '.....AA..AA........AA..AA......AA......AA..AA...AA..AA..AA......'
LOGO[56] = '.....AAAAAAAA......AA..AA......AAAAAAAAAA..AA...AA..AA..AA......'
LOGO[57] = '.....AAAAAAAA......AA..AA......AAAAAAAAAA..AA...AA..AA..AA......'
save_sprite(LOGO, 'loading_logo.png')

# Large logo
LOGO_LG = ['.'*256]*128
for i in range(16, 80):
    LOGO_LG[i] = 'A'*256
for i in range(84, 106):
    LOGO_LG[i] = 'A'*256
save_sprite(LOGO_LG, 'logo_large.png')

# Splash background
splash = Image.new('RGBA', (800, 480), (0, 0, 0, 0))
draw = ImageDraw.Draw(splash)
for y in range(480):
    t = y / 480
    draw.line([(0, y), (800, y)], fill=(int(10+t*15), int(20+t*30), int(40+t*50), 255))
for _ in range(200):
    x, y = random.randint(0, 799), random.randint(0, 200)
    c = random.randint(150, 255)
    draw.point((x, y), fill=(c, c, c, 255))
draw.ellipse([600, 40, 720, 160], fill=(220, 220, 240, 255))
draw.ellipse([620, 35, 720, 155], fill=(10, 20, 40, 255))
for i in range(0, 800, 4):
    for j in range(320, 480):
        wave = math.sin(i*0.05 + j*0.1) * 3
        draw.point((i+int(wave), j), fill=(int(20+wave*2), int(40+wave*2), int(80+wave*3), 200))
island_pts = [(350,320),(380,290),(420,280),(460,285),(500,300),(530,320),(520,340),(490,350),(440,355),(390,350),(360,340)]
draw.polygon(island_pts, fill=(30, 50, 20, 255))
splash.save(os.path.join(OUT_DIR, 'splash_bg.png'), 'PNG')

# Island tile
ISLAND = ['b'*32]*32
for i in range(2, 10):
    w = i*2
    ISLAND[i] = 'b'*((32-w)//2) + 'g'*w + 'b'*((32-w)//2)
for i in range(10, 32):
    ISLAND[i] = 'g'*32
save_sprite(ISLAND, 'island_tile.png')

# Tile atlas
atlas = Image.new('RGBA', (256, 256), (0, 0, 0, 0))
tile_names = ['grass_tile','dirt_tile','stone_tile','water_tile','tree_tile',
              'cave_entrance','ore_vein','gem_ore','coal_ore','iron_ore','gold_ore']
for idx, name in enumerate(tile_names):
    try:
        tile = Image.open(os.path.join(OUT_DIR, f'{name}.png'))
        col, row = idx % 8, idx // 8
        atlas.paste(tile, (col*32, row*32))
    except: pass
atlas.save(os.path.join(OUT_DIR, 'tile_atlas.png'), 'PNG')

print(f"\nSprites generated in {OUT_DIR}")

# ============================================================
# AUDIO GENERATION
# ============================================================
def gen_wav(filename, samples, sr=22050):
    path = os.path.join(OUT_DIR, filename)
    with wave_module.open(path, 'w') as wf:
        wf.setnchannels(1); wf.setsampwidth(2); wf.setframerate(sr)
        raw = b''
        for s in samples:
            v = int(max(-1.0, min(1.0, s)) * 32767)
            raw += struct.pack('<h', v)
        wf.writeframes(raw)
    # Convert to OGG
    os.system(f'ffmpeg -y -i {path} -q:a 4 {path.replace(".wav",".ogg")} 2>/dev/null')
    os.remove(path)
    return path.replace('.wav','.ogg')

def synth(freq, dur, sr, wtype='square', duty=0.5, fade=0.02):
    n, fn = int(sr*dur), int(sr*fade)
    return [((1.0 if wtype=='square' and (t*freq%1.0)<duty else -1.0) if wtype=='square' else
             (4.0*abs(t*freq%1.0-0.5)-1.0) if wtype=='triangle' else
             (2.0*(t*freq%1.0)-1.0) if wtype=='saw' else
             math.sin(2*math.pi*t*freq) if wtype=='sine' else
             random.uniform(-1,1)) *
            (1.0 if i>=fn and i<=n-fn else (i/fn if i<fn else (n-i)/fn))
            for i, t in enumerate([i/sr for i in range(n)])]

print("\nGenerating audio...")

# Pickup
pu = []; [pu.extend(synth(f, 0.05, 22050, 'square', 0.5)) for f in [800,1200,1600,2000]]
gen_wav('sfx_pickup.wav', pu)

# Mining  
mn = synth(150, 0.3, 22050, 'noise', fade=0.03)
for i in range(6):
    hit = synth(80, 0.04, 22050, 'square', 0.5)
    for j in range(len(hit)):
        idx = int(i*0.05*22050)+j
        if idx < len(mn): mn[idx] = (mn[idx]+hit[j]*0.5)/1.5
gen_wav('sfx_mining.wav', mn)

# Splash
sp = []
for _ in range(8):
    sp.extend(synth(random.uniform(200,600), 0.08, 22050, 'noise', fade=0.01))
    sp.extend([0.0]*int(0.05*22050))
gen_wav('sfx_splash.wav', sp)

# Craft
cr = []; [cr.extend(synth(f, 0.1, 22050, 'triangle', 0.5, fade=0.02)) for f in [440,554,659,880]]
gen_wav('sfx_craft.wav', cr)

# Damage
dm = [d*0.7 for d in synth(100, 0.15, 22050, 'noise', fade=0.02)]
th = synth(60, 0.2, 22050, 'sine', fade=0.03)
for i in range(len(th)):
    if i < len(dm): dm[i] = (dm[i]+th[i]*0.5)/1.5
gen_wav('sfx_damage.wav', dm)

# Click
cl = synth(1600, 0.03, 22050, 'square', 0.3, fade=0.005)
cl.extend(synth(2000, 0.03, 22050, 'square', 0.3, fade=0.005))
gen_wav('sfx_click.wav', cl)

# Ambient ocean
oc = []
for _ in range(20):
    s = synth(random.uniform(100,300), random.uniform(0.3,0.8), 22050, 'noise', fade=0.1)
    for i in range(len(s)): s[i] *= math.sin(math.pi*i/len(s)) * 0.3
    oc.extend(s); oc.extend([0.0]*int(random.uniform(0.2,0.6)*22050))
gen_wav('ambient_ocean.wav', oc)

# BGM
bgm = []
for note in [55,55,55,55,65,65,55,55,73,73,65,55,49,49,55,55]:
    bgm.extend(synth(note, 0.5, 22050, 'sine', fade=0.1))
mel = []
for note in [0,0,0,0,110,110,0,0,146,146,130,110,0,0,110,98]:
    mel.extend(synth(note, 0.4, 22050, 'triangle', 0.5, fade=0.08) if note else [0.0]*int(0.5*22050))
for i in range(len(bgm)):
    m = mel[i]*0.3 if i < len(mel) else 0
    bgm[i] = (bgm[i]*0.4+m)/0.7
gen_wav('bgm_ambient.wav', bgm*4)

# ============================================================
# TOTAL SIZE
# ============================================================
total = sum(os.path.getsize(os.path.join(r,f)) for r,_,fs in os.walk(OUT_DIR) for f in fs)
total += sum(os.path.getsize(os.path.join(r,f)) for r,_,fs in os.walk(RES_DIR) for f in fs if f.endswith('.png'))
print(f"\nTotal assets: {total/1024:.1f} KB ({total/1024/1024:.2f} MB)")