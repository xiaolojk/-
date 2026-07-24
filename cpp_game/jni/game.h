// game.h - 共享声明
#pragma once
#include <cstdint>
#include <EGL/egl.h>
#include <GLES2/gl2.h>
#include <android/input.h>
#include <vector>
#include <cstring>

// 常量
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

struct Game {
    uint8_t tiles[WORLD_H][WORLD_W];
    uint8_t walls[WORLD_H][WORLD_W];
    Player p;
    float camX=0,camY=0,gameTime=0,dayNight=0;
    // 输入: 键盘(调试)
    bool keys[256]={};
    // 触控按钮状态
    bool touchLeft=false, touchRight=false, touchJump=false, touchMine=false, touchInteract=false;
    int mineHold=0; // 0=none 1=按钮 2=点屏
    int touchIdLeft=-1,touchIdRight=-1,touchIdJump=-1,touchIdMine=-1,touchIdInteract=-1,touchIdCanvas=-1;
    float canvasMineX=0,canvasMineY=0;
    // 挖掘
    int mineTX=-1, mineTY=-1; float mineProg=0;
    // 库存
    int inv[I_COUNT]={};
    // 线索
    Clue clues[15]; int clueCount=0, foundClues=0;
    // 粒子
    std::vector<Particle> parts;
    // UI
    bool initialized=false, paused=false, showInv=false, showCraft=false, showMenu=false, showMain=true;
    int selectedSlot=0;
    float toast=0; char toastText[160]={};
    // 面板显示列表
    int invDisplay[24]; int invDisplayCount=0;
    // 屏幕
    int scrW=0, scrH=0;
    // EGL
    EGLDisplay eglDisplay=EGL_NO_DISPLAY;
    EGLSurface eglSurface=EGL_NO_SURFACE;
    EGLContext eglContext=EGL_NO_CONTEXT;
    // GL
    GLuint prog=0, aPos=0, aUV=0, aColor=0, uTex=0, uW=0, uH=0, texWhite=0;
    GLuint texPlayer[6]={}; // 0=idle 1-4=walk 5=jump
    GLuint texGrass=0,texDirt=0,texStone=0,texIron=0,texCoal=0,texGold=0,texWater=0,texTree=0,texBerry=0,texClue=0;
};

extern Game* g;

// 按钮(比例坐标 0-1, 左上原点): 0left 1right 2jump 3mine 4interact 5inv 6craft 7menu
struct Btn { float x,y,w,h; const char* label; };
extern Btn BTNS[8];
inline bool btnHit(const Btn& b,float px,float py,float sw,float sh){
    return px>=b.x*sw && px<=(b.x+b.w)*sw && py>=b.y*sh && py<=(b.y+b.h)*sh;
}

// 函数声明
void initGame();
void update(float dt);
void render();
void setupGL();
void loadTextures();
void handleInput(AInputEvent* ev);
void showToast(const char* msg);
void tryInteract();
void tryUseItem(int id);
void tryCraft(int idx);
// 内部辅助（render需要）
int getTileAt(int tx,int ty);
float mineTime(int t);
