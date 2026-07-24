#!/usr/bin/env python3
"""Download free pixel art sprites and convert to C++ embedded arrays."""
import struct, zlib, io, os, sys, math, random

OUT_DIR = os.path.join(os.path.dirname(__file__), "..", "jni", "sprites_data.h")

def png_to_rgba(raw_data):
    """Parse PNG and extract RGBA pixels."""
    # Simple PNG parser - handles basic PNG files
    if raw_data[:8] != b'\x89PNG\r\n\x1a\n':
        return None, None, None
    
    pos = 8
    width = height = 0
    idat_data = b''
    
    while pos < len(raw_data):
        length = struct.unpack('>I', raw_data[pos:pos+4])[0]
        chunk_type = raw_data[pos+4:pos+8]
        chunk_data = raw_data[pos+8:pos+8+length]
        
        if chunk_type == b'IHDR':
            width = struct.unpack('>I', chunk_data[0:4])[0]
            height = struct.unpack('>I', chunk_data[4:8])[0]
        elif chunk_type == b'IDAT':
            idat_data += chunk_data
        
        pos += 12 + length
        if chunk_type == b'IEND':
            break
    
    if not idat_data or width == 0 or height == 0:
        return None, None, None
    
    try:
        raw = zlib.decompress(idat_data)
    except:
        return None, None, None
    
    # Extract RGBA
    rgba = bytearray(width * height * 4)
    row_size = width * 4 + 1  # +1 for filter byte
    
    for y in range(height):
        row_start = y * row_size
        if row_start + row_size > len(raw):
            break
        filter_byte = raw[row_start]
        row_data = raw[row_start+1:row_start+row_size]
        
        # Apply filter
        if filter_byte == 0:  # None
            filtered = row_data
        elif filter_byte == 1:  # Sub
            filtered = bytearray(row_data)
            for i in range(4, len(filtered)):
                filtered[i] = (filtered[i] + filtered[i-4]) & 0xFF
        elif filter_byte == 2:  # Up
            filtered = bytearray(row_data)
            if y > 0:
                prev = memoryview(rgba)[(y-1)*width*4:(y-1)*width*4+width*4]
                for i in range(len(filtered)):
                    filtered[i] = (filtered[i] + prev[i]) & 0xFF
        else:
            filtered = row_data
        
        rgba[y*width*4:y*width*4+len(filtered)] = filtered
    
    return bytes(rgba), width, height


def generate_procedural_sprites():
    """Generate beautiful procedural pixel art sprites and return them as (name, width, height, rgba_data)."""
    sprites = []
    
    def rgba(r, g, b, a=255):
        return bytes([r, g, b, a])
    
    def make_sprite(w, h, painter):
        data = bytearray(w * h * 4)
        for y in range(h):
            for x in range(w):
                r, g, b, a = painter(x, y)
                idx = (y * w + x) * 4
                data[idx:idx+4] = bytes([r, g, b, a])
        return bytes(data)
    
    # ============ Player 32x48 (6 frames) ============
    def player_painter(frame):
        def paint(x, y):
            # Animation offsets
            legA = legB = armA = armB = 0
            if 1 <= frame <= 4:
                la = [2, 0, -2, 0]; lb = [-2, 0, 2, 0]
                aa = [-2, 0, 2, 0]; ab = [2, 0, -2, 0]
                legA, legB, armA, armB = la[frame-1], lb[frame-1], aa[frame-1], ab[frame-1]
            elif frame == 5:
                legA, legB, armA, armB = -4, 4, 4, -4
            
            # Hair (dark brown, layered)
            if 10 <= x <= 21 and 1 <= y <= 6:
                if y <= 2: return (45, 22, 14, 255)
                if y <= 4 and (x <= 10 or x >= 21): return (55, 28, 18, 255)
                return (55, 30, 18, 255)
            if (8 <= x <= 12 or 19 <= x <= 23) and 3 <= y <= 6:
                return (65, 35, 22, 255)
            # Hair highlight
            if 10 <= x <= 21 and 5 <= y <= 6:
                return (75, 42, 28, 255)
            
            # Face
            if 10 <= x <= 21 and 7 <= y <= 17:
                # Shadow on sides
                if x <= 11 or x >= 20:
                    return (225, 175, 140, 255)
                if x == 10 or x == 21:
                    return (210, 160, 125, 255)
                return (248, 198, 163, 255)
            
            # Eyes
            if 13 <= x <= 14 and 11 <= y <= 13:
                if x == 13 and y == 11: return (255, 255, 255, 255)
                if x == 14 and y == 12: return (90, 180, 255, 255)
                return (25, 30, 50, 255)
            if 17 <= x <= 18 and 11 <= y <= 13:
                if x == 17 and y == 11: return (255, 255, 255, 255)
                if x == 18 and y == 12: return (90, 180, 255, 255)
                return (25, 30, 50, 255)
            # Eyebrows
            if 12 <= x <= 15 and y == 10: return (50, 28, 15, 255)
            if 16 <= x <= 19 and y == 10: return (50, 28, 15, 255)
            # Mouth
            if 14 <= x <= 17 and y == 15: return (200, 90, 70, 255)
            # Nose
            if 15 <= x <= 16 and y == 13: return (220, 175, 145, 255)
            
            # Neck
            if 13 <= x <= 18 and 18 <= y <= 19:
                return (235, 188, 148, 255)
            
            # Shirt (blue)
            if 8 <= x <= 23 and 20 <= y <= 33:
                if y <= 21: return (58, 128, 192, 255)  # collar highlight
                if y >= 32: return (25, 72, 132, 255)    # bottom shadow
                if 14 <= x <= 17 and 22 <= y <= 31: return (32, 85, 145, 255)  # center line
                if 10 <= x <= 11 and 25 <= y <= 29: return (52, 118, 178, 255)  # pocket
                if 21 <= x <= 22 and 25 <= y <= 29: return (52, 118, 178, 255)  # pocket
                return (42, 102, 165, 255)
            
            # Arms (with animation)
            # Left arm
            ly = 20 + armA
            if 6 <= x <= 8 and ly <= y <= ly + 2:
                return (42, 102, 165, 255)
            if 6 <= x <= 8 and ly + 3 <= y <= ly + 11:
                return (248, 198, 163, 255)
            # Right arm
            ry = 20 + armB
            if 24 <= x <= 26 and ry <= y <= ry + 2:
                return (42, 102, 165, 255)
            if 24 <= x <= 26 and ry + 3 <= y <= ry + 11:
                return (248, 198, 163, 255)
            
            # Belt
            if 8 <= x <= 23 and 34 <= y <= 36:
                if 14 <= x <= 17: return (218, 178, 32, 255)  # buckle
                if x == 15 and y == 35: return (240, 200, 60, 255)  # buckle highlight
                return (105, 68, 35, 255)
            
            # Pants (dark grey-blue)
            if 9 <= x <= 14 and 37 <= y <= 42:
                if y == 37: return (72, 72, 92, 255)
                return (58, 58, 78, 255)
            if 17 <= x <= 22 and 37 <= y <= 42:
                if y == 37: return (72, 72, 92, 255)
                return (58, 58, 78, 255)
            
            # Boots (dark brown)
            if 7 <= x <= 14 and 43 <= y <= 47:
                if y == 43: return (72, 48, 28, 255)
                if y == 47: return (35, 22, 10, 255)
                return (52, 32, 18, 255)
            if 17 <= x <= 24 and 43 <= y <= 47:
                if y == 43: return (72, 48, 28, 255)
                if y == 47: return (35, 22, 10, 255)
                return (52, 32, 18, 255)
            
            return (0, 0, 0, 0)
        return paint
    
    for f in range(6):
        data = make_sprite(32, 48, player_painter(f))
        sprites.append((f"player_{f}", 32, 48, data))
    
    # ============ Grass Tile 32x32 ============
    def grass_painter(x, y):
        # Dirt base
        r, g, b = 98, 82, 38
        # Grass top
        if y <= 6:
            r, g, b = 102, 158, 58
        if y <= 3:
            r, g, b = 128, 188, 82
        if y <= 1:
            r, g, b = 142, 205, 95
        # Grass blades
        if y == 5 or y == 6:
            if x % 3 == 0:
                r, g, b = 138, 212, 105
        # Dirt texture
        if y > 7:
            if (x + y) % 7 == 0:
                r, g, b = 82, 68, 28
            if (x * 3 + y) % 11 == 0:
                r, g, b = 115, 98, 52
        # Edge shadow
        if y >= 28:
            a = (y - 28) * 60
            return (r, g, b, max(0, 255 - a))
        if x >= 28:
            a = (x - 28) * 50
            return (r, g, b, max(0, 255 - a))
        return (r, g, b, 255)
    sprites.append(("grass", 32, 32, make_sprite(32, 32, grass_painter)))
    
    # ============ Dirt Tile 32x32 ============
    def dirt_painter(x, y):
        r, g, b = 128, 98, 22
        # Slight variation
        if (x + y) % 5 == 0: r, g, b = 118, 88, 18
        if (x * 3 + y) % 7 == 0: r, g, b = 148, 115, 35
        if (x + y * 2) % 9 == 0: r, g, b = 165, 128, 48
        if y >= 28:
            a = (y - 28) * 60
            return (r, g, b, max(0, 255 - a))
        return (r, g, b, 255)
    sprites.append(("dirt", 32, 32, make_sprite(32, 32, dirt_painter)))
    
    # ============ Stone Tile 32x32 ============
    def stone_painter(x, y):
        r, g, b = 128, 128, 133
        # Highlight top
        if y <= 4:
            r, g, b = 155, 155, 160
        if y <= 2:
            r, g, b = 168, 168, 173
        # Cracks
        if (abs(x - 10) <= 2 and abs(y - 8) <= 1): r, g, b = 105, 105, 110
        if (abs(x - 20) <= 3 and abs(y - 14) <= 1): r, g, b = 105, 105, 110
        if (abs(x - 14) <= 3 and abs(y - 22) <= 1): r, g, b = 105, 105, 110
        # Texture spots
        if (x * 7 + y * 13) % 17 == 0: r, g, b = 140, 140, 145
        if (x * 11 + y * 5) % 19 == 0: r, g, b = 115, 115, 120
        if y >= 28:
            a = (y - 28) * 60
            return (r, g, b, max(0, 255 - a))
        return (r, g, b, 255)
    sprites.append(("stone", 32, 32, make_sprite(32, 32, stone_painter)))
    
    # ============ Ore tiles (Iron, Coal, Gold) ============
    for ore_type, mr, mg, mb, hr, hg, hb in [
        ("iron", 200, 144, 98, 225, 170, 125),
        ("coal", 35, 35, 35, 52, 52, 52),
        ("gold", 255, 215, 0, 255, 240, 140),
    ]:
        def ore_painter(x, y, mr=mr, mg=mg, mb=mb, hr=hr, hg=hg, hb=hb):
            r, g, b = 128, 128, 133  # stone base
            if y <= 4: r, g, b = 155, 155, 160
            # Ore veins
            if y >= 6 and y <= 24:
                if (x + y) % 5 < 2 and (x * 3 + y) % 7 < 3:
                    r, g, b = mr, mg, mb
                if (x + y) % 7 == 0:
                    r, g, b = hr, hg, hb
            # Gold sparkles
            if ore_type == "gold" and (x * 13 + y * 7) % 23 == 0:
                return (255, 255, 255, 255)
            if y >= 28:
                a = (y - 28) * 60
                return (r, g, b, max(0, 255 - a))
            return (r, g, b, 255)
        sprites.append((ore_type, 32, 32, make_sprite(32, 32, ore_painter)))
    
    # ============ Water Tile 32x32 ============
    def water_painter(x, y):
        r, g, b = 30, 95, 180
        if y <= 18:
            r, g, b = 40, 125, 212
        if y <= 10:
            r, g, b = 48, 145, 228
        # Wave lines
        if (y + int(math.sin(x * 0.5) * 2)) % 10 == 0:
            r, g, b = 255, 255, 255
            return (r, g, b, 90)
        return (r, g, b, 220)
    sprites.append(("water", 32, 32, make_sprite(32, 32, water_painter)))
    
    # ============ Tree 32x64 ============
    def tree_painter(x, y):
        # Trunk
        if 12 <= x <= 19 and 30 <= y <= 63:
            if x <= 13: return (108, 75, 42, 255)
            if x >= 18: return (65, 40, 16, 255)
            return (92, 60, 28, 255)
        # Bark texture
        if 14 <= x <= 15:
            if y == 38 or y == 46 or y == 54: return (80, 48, 22, 255)
        if 16 <= x <= 17:
            if y == 42 or y == 50: return (80, 48, 22, 255)
        # Canopy (layered circles)
        for cx, cy, r, col in [
            (16, 22, 14, (42, 112, 30)),
            (16, 18, 11, (55, 142, 48)),
            (16, 14, 8, (68, 162, 60)),
            (16, 10, 5, (82, 178, 75)),
            (16, 7, 3, (95, 192, 88)),
        ]:
            dx, dy = x - cx, y - cy
            if dx * dx + dy * dy <= r * r:
                return (*col, 255)
        # Shadow areas
        if 8 <= x <= 10 and 26 <= y <= 30:
            return (32, 92, 20, 255)
        return (0, 0, 0, 0)
    sprites.append(("tree", 32, 64, make_sprite(32, 64, tree_painter)))
    
    # ============ Berry Bush 32x32 ============
    def berry_painter(x, y):
        for cx, cy, r, col in [
            (16, 16, 13, (42, 122, 48)),
            (16, 14, 10, (55, 152, 62)),
            (16, 12, 7, (68, 170, 76)),
        ]:
            dx, dy = x - cx, y - cy
            if dx * dx + dy * dy <= r * r:
                return (*col, 255)
        # Berries
        for bx, by in [(8, 12), (22, 14), (15, 20), (20, 9)]:
            if (x - bx) ** 2 + (y - by) ** 2 <= 7:
                return (210, 28, 98, 255)
            if (x - bx) ** 2 + (y - by) ** 2 <= 4:
                return (228, 42, 112, 255)
        # Berry highlights
        for bx, by in [(7, 11), (21, 13), (14, 19), (19, 8)]:
            if (x - bx) ** 2 + (y - by) ** 2 <= 2:
                return (245, 65, 130, 255)
        return (0, 0, 0, 0)
    sprites.append(("berry", 32, 32, make_sprite(32, 32, berry_painter)))
    
    # ============ Clue Marker 32x32 ============
    def clue_painter(x, y):
        r = 10
        dx, dy = x - 16, y - 16
        dist = dx * dx + dy * dy
        if dist <= r * r:
            if dist <= 20: return (255, 245, 180, 240)
            if dist <= 50: return (255, 230, 100, 220)
            return (255, 215, 0, 180)
        # Question mark
        if 14 <= x <= 17 and y == 10: return (80, 40, 10, 230)
        if x == 17 and 11 <= y <= 13: return (80, 40, 10, 230)
        if 14 <= x <= 16 and y == 14: return (80, 40, 10, 230)
        if x == 13 and 18 <= y <= 19: return (80, 40, 10, 255)
        return (0, 0, 0, 0)
    sprites.append(("clue", 32, 32, make_sprite(32, 32, clue_painter)))
    
    return sprites


def write_sprites_header(sprites):
    """Write sprites as C++ header with embedded RGBA data."""
    lines = []
    lines.append("// sprites_data.h - Auto-generated beautiful pixel art sprites")
    lines.append("#pragma once")
    lines.append("#include <cstdint>")
    lines.append("")
    lines.append("struct SpriteData {")
    lines.append("    const char* name;")
    lines.append("    int w, h;")
    lines.append("    const uint8_t* data;")
    lines.append("};")
    lines.append("")
    
    # Write each sprite as a static const array
    for name, w, h, data in sprites:
        lines.append(f"// {name} {w}x{h}")
        lines.append(f"static const uint8_t _sprite_{name}[] = {{")
        # Write as hex bytes, 16 per line
        for i in range(0, len(data), 16):
            chunk = data[i:i+16]
            hex_str = ", ".join(f"0x{b:02X}" for b in chunk)
            if i + 16 < len(data):
                lines.append(f"    {hex_str},")
            else:
                lines.append(f"    {hex_str}")
        lines.append("};")
        lines.append("")
    
    # Sprite registry
    lines.append(f"static const SpriteData SPRITE_REGISTRY[] = {{")
    for name, w, h, data in sprites:
        lines.append(f'    {{"{name}", {w}, {h}, _sprite_{name}}},')
    lines.append("};")
    lines.append(f"static const int SPRITE_COUNT = {len(sprites)};")
    lines.append("")
    
    # Helper to get sprite by name
    lines.append("inline const SpriteData* getSprite(const char* name) {")
    lines.append("    for (int i = 0; i < SPRITE_COUNT; i++)")
    lines.append('        if (__builtin_strcmp(SPRITE_REGISTRY[i].name, name) == 0)')
    lines.append("            return &SPRITE_REGISTRY[i];")
    lines.append("    return nullptr;")
    lines.append("}")
    
    with open(OUT_DIR, 'w') as f:
        f.write('\n'.join(lines))
    
    print(f"Generated {OUT_DIR} with {len(sprites)} sprites ({sum(len(d) for _,_,_,d in sprites)} bytes)")


if __name__ == "__main__":
    sprites = generate_procedural_sprites()
    write_sprites_header(sprites)