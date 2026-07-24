// game.h - 共享声明 (v6.0 像素化中文版)
#pragma once
#include <cstdint>
#include <EGL/egl.h>
#include <GLES2/gl2.h>
#include <android/input.h>
#include <vector>
#include <cstring>

// 虚拟分辨率（像素艺术固定分辨率，放大到屏幕）
static const int VW=480, VH=270;
static const int TILE=32;
static const int WORLD_W=150, WORLD_H=80;
static const int SURFACE_Y=38;
static const int ISLAND_S=20, ISLAND_E=130;
static const float GRAVITY=800.f, JUMP_V=-400.f, MOVE_SPD=180.f;

enum Tile : uint8_t { T_AIR=0, T_GRASS, T_DIRT, T_STONE, T_IRON, T_COAL, T_GOLD, T_WATER };
enum Wall : uint8_t { W_NONE=0, W_TREE=101, W_BERRY=102, W_CLUE=103 };

enum Item { I_NONE=0,I_DIRT,I_STONE,I_WOOD,I_PLANK,I_STICK,I_PICK,I_TORCH,I_CAMP,I_BANDAGE,
            I_BERRY,I_CBERRY,I_FWATER,I_PWATER,I_IRON,I_GOLD,I_COAL,I_INGOT,I_IPICK,
            I_RAFT,I_RSUIT,I_FILTER, I_COUNT };

struct Player {
    float x=0,y=0,vx=0,vy=0;
    float w=22,h=44;
    float hp=100,maxHp=100,hunger=100,maxHunger=100,thirst=100,maxThirst=100,rad=0,maxRad=100;
    bool onGround=false, inWater=false, hasRadSuit=false;
    int facing=1, animFrame=0;
    float animTimer=0, invinceTimer=0;
};

struct Particle { float x,y,vx,vy,life; uint8_t r,g,b; };
struct Clue { int id,x,y; bool found; const char* text; };

struct Recipe { const char* name; int icon; int req[4][2]; int give[2]; };
extern Recipe RECIPES[];
extern const int RECIPE_COUNT;

// 设置项
struct Settings {
    int pixelScale=3;      // 像素放大倍数 (1=原生 2/3/4=像素风)
    bool soundOn=true;
    bool musicOn=true;
    bool vibrationOn=true;
    bool touchControl=true; // true=虚拟按键 false=点击
};

// 游戏状态
enum GameState { STATE_SPLASH, STATE_MAIN, STATE_PLAYING, STATE_PAUSED, STATE_SETTINGS };

struct Game {
    uint8_t tiles[WORLD_H][WORLD_W];
    uint8_t walls[WORLD_H][WORLD_W];
    Player p;
    float camX=0,camY=0,gameTime=0,dayNight=0;
    bool keys[256]={};
    bool touchLeft=false, touchRight=false, touchJump=false, touchMine=false, touchInteract=false;
    int mineHold=0;
    int touchIdLeft=-1,touchIdRight=-1,touchIdJump=-1,touchIdMine=-1,touchIdInteract=-1,touchIdCanvas=-1;
    float canvasMineX=0,canvasMineY=0;
    int mineTX=-1, mineTY=-1; float mineProg=0;
    int inv[I_COUNT]={};
    Clue clues[15]; int clueCount=0, foundClues=0;
    std::vector<Particle> parts;
    // UI状态
    GameState state=STATE_SPLASH;
    float splashTimer=0;
    bool showInv=false, showCraft=false, showMenu=false, showSettings=false;
    int selectedSlot=0;
    float toast=0; char toastText[160]={};
    int invDisplay[24]; int invDisplayCount=0;
    // 屏幕尺寸（真实物理分辨率）
    int scrW=0, scrH=0;
    // 虚拟分辨率渲染偏移（letterbox）
    int viewX=0, viewY=0, viewW=0, viewH=0;
    // 设置
    Settings settings;
    // EGL
    EGLDisplay eglDisplay=EGL_NO_DISPLAY;
    EGLSurface eglSurface=EGL_NO_SURFACE;
    EGLContext eglContext=EGL_NO_CONTEXT;
    // GL
    GLuint prog=0, aPos=0, aUV=0, aColor=0, uTex=0, uW=0, uH=0, texWhite=0;
    // FBO（像素化渲染）
    GLuint fbo=0, fboTex=0;
    // 字体纹理
    GLuint texFont=0;
    // 游戏纹理
    GLuint texPlayer[6]={};
    GLuint texGrass=0,texDirt=0,texStone=0,texIron=0,texCoal=0,texGold=0,texWater=0,texTree=0,texBerry=0,texClue=0;
    // 兼容
    bool initialized=false, paused=false, showMain=false;
};

extern Game* g;

// 按钮(虚拟分辨率坐标): 0left 1right 2jump 3mine 4interact 5inv 6craft 7menu
struct Btn { float x,y,w,h; const char* label; };
extern Btn BTNS[8];
inline bool btnHit(const Btn& b,float px,float py){
    return px>=b.x && px<=(b.x+b.w) && py>=b.y && py<=(b.y+b.h);
}

// 函数声明
void initGame();
void update(float dt);
void render();
void setupGL();
void loadTextures();
void loadFont();
void handleInput(AInputEvent* ev);
void showToast(const char* msg);
void tryInteract();
void tryUseItem(int id);
void tryCraft(int idx);
int getTileAt(int tx,int ty);
float mineTime(int t);
