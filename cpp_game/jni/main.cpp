// main.cpp - 游戏逻辑 + 入口 (v6.0 像素中文版)
#include "game.h"
#include <android_native_app_glue.h>
#include <android/log.h>
#include <time.h>
#include <cmath>
#include <cstring>
#include <cstdio>
#include <cstdlib>

#define LOGI(...) __android_log_print(ANDROID_LOG_INFO,"LBS",__VA_ARGS__)

Game* g=nullptr;

// 按钮(虚拟分辨率坐标480x270): 0左 1右 2跳 3挖 4用 5背包 6制作 7菜单
Btn BTNS[8] = {
    {10,  220, 60, 45, "←"},
    {78,  220, 60, 45, "→"},
    {146, 220, 60, 45, "跳"},
    {354, 220, 60, 45, "挖"},
    {420, 220, 50, 45, "用"},
    {180, 4,   60, 16, "背包"},
    {244, 4,   60, 16, "制作"},
    {308, 4,   60, 16, "菜单"},
};

Recipe RECIPES[] = {
    {"木板",    I_PLANK,  {{I_WOOD,1},{-1,0},{0,0},{0,0}},        {I_PLANK,4}},
    {"木棍",    I_STICK,  {{I_PLANK,1},{-1,0},{0,0},{0,0}},       {I_STICK,4}},
    {"火把",    I_TORCH,  {{I_STICK,1},{I_COAL,1},{-1,0},{0,0}},  {I_TORCH,4}},
    {"篝火",    I_CAMP,   {{I_STICK,4},{I_PLANK,2},{I_STONE,1},{-1,0}},{I_CAMP,1}},
    {"绷带",    I_BANDAGE,{{I_STICK,1},{I_PLANK,1},{-1,0},{0,0}}, {I_BANDAGE,1}},
    {"镐子",    I_PICK,   {{I_STICK,2},{I_PLANK,2},{I_STONE,1},{-1,0}},{I_PICK,1}},
    {"净水器",  I_FILTER, {{I_STONE,3},{I_PLANK,2},{I_STICK,2},{-1,0}},{I_FILTER,1}},
    {"木筏",    I_RAFT,   {{I_PLANK,6},{I_STICK,4},{-1,0},{0,0}}, {I_RAFT,1}},
    {"防辐射服",I_RSUIT,  {{I_PLANK,4},{I_STONE,2},{I_STICK,2},{-1,0}},{I_RSUIT,1}},
    {"熟浆果",  I_CBERRY, {{I_BERRY,3},{I_CAMP,1},{-1,0},{0,0}},  {I_CBERRY,1}},
    {"纯净水",  I_PWATER, {{I_FWATER,3},{I_FILTER,1},{-1,0},{0,0}},{I_PWATER,1}},
    {"铁锭",    I_INGOT,  {{I_IRON,3},{I_CAMP,1},{I_COAL,1},{-1,0}},{I_INGOT,1}},
    {"铁镐",    I_IPICK,  {{I_INGOT,2},{I_STICK,2},{I_PLANK,1},{-1,0}},{I_IPICK,1}},
};
const int RECIPE_COUNT=sizeof(RECIPES)/sizeof(Recipe);

// ==================== 世界生成 ====================
static int findGroundY(int x){
    for(int y=0;y<WORLD_H;y++) if(g->tiles[y][x] && g->tiles[y][x]!=T_WATER) return y;
    return SURFACE_Y;
}
static void carveCave(int cx,int cy,int r){
    for(int y=cy-r;y<=cy+r;y++) for(int x=cx-r;x<=cx+r;x++){
        if(x<0||x>=WORLD_W||y<0||y>=WORLD_H) continue;
        float dx=(float)(x-cx)/r, dy=(float)(y-cy)/(r*0.7f);
        if(dx*dx+dy*dy<1 && g->tiles[y][x] && g->tiles[y][x]!=T_WATER){
            g->tiles[y][x]=T_AIR; g->walls[y][x]=W_NONE;
        }
    }
}
static const char* CLUE_TEXTS[15]={
    "纸条:核电站泄漏...","收音机:最后的广播","金属牌:冰川监测站2047",
    "日记:海平面上升70米","涂鸦:寻找方舟计划","字条:不要喝海水",
    "地图碎片:地下避难所","报告:变异样本失控","信:亲爱的...",
    "GPS:最近陆地3742km","笔记:反向渗透净化","船日志:第87天",
    "蓝图:矿井设计图","军方文件:Operation Ark","镜子:记住你是谁"
};
static void generateWorld(){
    memset(g->tiles,0,sizeof(g->tiles));
    memset(g->walls,0,sizeof(g->walls));
    for(int x=0;x<WORLD_W;x++){
        int gy=SURFACE_Y;
        float d=std::abs(x-(ISLAND_S+ISLAND_E)/2);
        float maxD=(ISLAND_E-ISLAND_S)/2.f;
        if(d>maxD*0.75f){ float t=(d-maxD*0.75f)/(maxD*0.25f); gy+=(int)(t*10); }
        gy += (int)(sinf(x*0.2f)*3 + sinf(x*0.5f)*1.5f);
        if(gy<SURFACE_Y-4) gy=SURFACE_Y-4;
        if(gy>SURFACE_Y+8) gy=SURFACE_Y+8;
        for(int y=gy;y<WORLD_H;y++){
            if(y<gy+3) g->tiles[y][x]=T_GRASS;
            else if(y<gy+12) g->tiles[y][x]=T_DIRT;
            else g->tiles[y][x]=T_STONE;
        }
    }
    for(int x=0;x<ISLAND_S;x++) for(int y=SURFACE_Y;y<WORLD_H;y++) g->tiles[y][x]=T_WATER;
    for(int x=ISLAND_E;x<WORLD_W;x++) for(int y=SURFACE_Y;y<WORLD_H;y++) g->tiles[y][x]=T_WATER;
    srand(42);
    for(int x=ISLAND_S+2;x<ISLAND_E-2;x++){
        int gy=findGroundY(x);
        if(gy>0&&gy<WORLD_H && rand()%100<22 && !g->walls[gy-1][x]) g->walls[gy-1][x]=W_TREE;
    }
    for(int x=ISLAND_S+3;x<ISLAND_E-3;x++){
        int gy=findGroundY(x);
        if(gy>0&&gy<WORLD_H && rand()%100<7 && !g->walls[gy-1][x]) g->walls[gy-1][x]=W_BERRY;
    }
    for(int i=0;i<60;i++){
        int x=ISLAND_S+rand()%(ISLAND_E-ISLAND_S);
        int y=SURFACE_Y+8+rand()%35;
        if(y<WORLD_H && g->tiles[y][x]==T_STONE){
            int r=rand()%100;
            if(r<35) g->tiles[y][x]=T_IRON;
            else if(r<55) g->tiles[y][x]=T_COAL;
            else if(r<60) g->tiles[y][x]=T_GOLD;
        }
    }
    for(int i=0;i<12;i++){
        int cx=ISLAND_S+10+rand()%(ISLAND_E-ISLAND_S-20);
        int cy=SURFACE_Y+5+rand()%25;
        carveCave(cx,cy,2+rand()%4);
    }
    g->clueCount=0;
    int cxs[15]={40,55,70,85,100,115,45,65,90,110,60,85,100,120,50};
    int cys[15]={36,36,36,36,36,36,50,55,48,52,60,62,58,50,45};
    for(int i=0;i<15;i++){
        g->clues[i]={i,cxs[i],cys[i],false,CLUE_TEXTS[i]};
        g->clueCount++;
        if(cys[i]<WORLD_H && cxs[i]<WORLD_W) g->walls[cys[i]][cxs[i]]=W_CLUE;
    }
}
static void initPlayer(){
    int sx=(ISLAND_S+ISLAND_E)/2;
    int sy=findGroundY(sx)-5;
    g->p.x=sx*TILE+TILE/2; g->p.y=sy*TILE;
    g->p.vx=0; g->p.vy=0; g->p.hp=100; g->p.hunger=100; g->p.thirst=100; g->p.rad=0;
    g->p.facing=1; g->p.animFrame=0; g->p.hasRadSuit=false;
}
void initGame(){
    generateWorld();
    initPlayer();
    memset(g->inv,0,sizeof(g->inv));
    g->camX=g->p.x; g->camY=g->p.y; g->gameTime=0; g->dayNight=0;
    g->foundClues=0; g->parts.clear();
    g->mineTX=-1; g->mineProg=0; g->mineHold=0;
    g->initialized=true;
}

// ==================== 库存 ====================
static void addItem(int id,int n){ if(id>0&&id<I_COUNT) g->inv[id]+=n; }
static int getCount(int id){ return (id>0&&id<I_COUNT)?g->inv[id]:0; }
static bool removeItem(int id,int n){ if(id<0||id>=I_COUNT||g->inv[id]<n) return false; g->inv[id]-=n; return true; }

// ==================== 物理 ====================
static bool isSolid(float x,float y){
    int tx=(int)(x/TILE), ty=(int)(y/TILE);
    if(tx<0||tx>=WORLD_W||ty<0||ty>=WORLD_H) return false;
    uint8_t t=g->tiles[ty][tx];
    if(t && t!=T_WATER) return true;
    uint8_t w=g->walls[ty][tx];
    if(w==W_TREE) return false;
    if(w) return true;
    return false;
}
int getTileAt(int tx,int ty){
    if(tx<0||tx>=WORLD_W||ty<0||ty>=WORLD_H) return 0;
    if(g->tiles[ty][tx]) return g->tiles[ty][tx];
    if(g->walls[ty][tx]) return g->walls[ty][tx];
    return 0;
}
static void spawnParts(float x,float y,int n,uint8_t r,uint8_t gg,uint8_t b){
    for(int i=0;i<n;i++) g->parts.push_back({x,y,(float)(rand()%100-50),-(float)(rand()%120+40),0.4f+(rand()%40)/100.f,r,gg,b});
}

// ==================== 挖掘 ====================
float mineTime(int t){
    bool hasPick=getCount(I_PICK)>0||getCount(I_IPICK)>0;
    bool hasIron=getCount(I_IPICK)>0;
    switch(t){
        case T_GRASS: case T_DIRT: return 0.3f;
        case T_STONE: return hasIron?0.4f:hasPick?0.7f:1.5f;
        case T_IRON: case T_COAL: return hasIron?0.5f:hasPick?0.9f:2.0f;
        case T_GOLD: return hasIron?0.7f:hasPick?1.3f:999.f;
        case W_TREE: return 0.5f;
        case W_BERRY: return 0.2f;
        case W_CLUE: return 0.1f;
        default: return 1.0f;
    }
}
static void completeMine(int tx,int ty,int t){
    float px=tx*TILE+TILE/2.f, py=ty*TILE+TILE/2.f;
    if(g->tiles[ty][tx]==t) g->tiles[ty][tx]=T_AIR;
    else if(g->walls[ty][tx]==t) g->walls[ty][tx]=W_NONE;
    switch(t){
        case T_GRASS: addItem(I_DIRT,1); spawnParts(px,py,5,100,80,30); break;
        case T_DIRT: addItem(I_DIRT,1); spawnParts(px,py,5,139,105,20); break;
        case T_STONE: addItem(I_STONE,1+rand()%2); spawnParts(px,py,6,170,170,170); break;
        case T_IRON: addItem(I_IRON,1+rand()%2); spawnParts(px,py,6,200,144,100); break;
        case T_COAL: addItem(I_COAL,1+rand()%2); spawnParts(px,py,6,50,50,50); break;
        case T_GOLD: addItem(I_GOLD,1+rand()%2); spawnParts(px,py,10,255,215,0); break;
        case W_TREE: addItem(I_WOOD,2+rand()%4); spawnParts(px,py,8,90,60,30); break;
        case W_BERRY: addItem(I_BERRY,2+rand()%3); spawnParts(px,py,5,233,30,99); break;
        case W_CLUE: {
            for(int i=0;i<g->clueCount;i++){
                if(g->clues[i].x==tx && g->clues[i].y==ty && !g->clues[i].found){
                    g->clues[i].found=true; g->foundClues++;
                    showToast(g->clues[i].text);
                }
            }
            break;
        }
    }
}

// ==================== UI ====================
void showToast(const char* msg){ g->toast=2.5f; strncpy(g->toastText,msg,159); g->toastText[159]=0; }
void tryInteract(){
    Player& p=g->p;
    int cx=(int)((p.x+p.facing*TILE*1.5f)/TILE);
    int cy=(int)((p.y-p.h/2)/TILE);
    int t=getTileAt(cx,cy);
    if(t==0){ cy=(int)(p.y/TILE); t=getTileAt(cx,cy); }
    if(t==0){ showToast("面前没东西"); return; }
    float dx=(cx*TILE+TILE/2.f)-p.x, dy=(cy*TILE+TILE/2.f)-(p.y-p.h/2);
    if(std::sqrt(dx*dx+dy*dy)>TILE*3.f){ showToast("太远了"); return; }
    if(t==W_BERRY){ addItem(I_BERRY,2+rand()%3); spawnParts(cx*TILE+16,cy*TILE+16,5,233,30,99); showToast("采集浆果"); if(rand()%100<30) g->walls[cy][cx]=W_NONE; }
    else if(t==T_WATER){ showToast("海水有辐射!"); }
    else if(t==W_CLUE){ showToast("挖掘查看线索"); }
    else { showToast("按住[挖]挖掘"); }
}
void tryUseItem(int id){
    Player& p=g->p; bool used=false; const char* msg="不能直接使用";
    switch(id){
        case I_BERRY: if(removeItem(id,1)){p.hunger=std::min(p.maxHunger,p.hunger+15);msg="浆果+15饥饿";used=true;} break;
        case I_CBERRY: if(removeItem(id,1)){p.hunger=std::min(p.maxHunger,p.hunger+35);msg="熟浆果+35饥饿";used=true;} break;
        case I_FWATER: if(removeItem(id,1)){p.thirst=std::min(p.maxThirst,p.thirst+20);msg="淡水+20口渴";used=true;} break;
        case I_PWATER: if(removeItem(id,1)){p.thirst=std::min(p.maxThirst,p.thirst+50);msg="纯净水+50口渴";used=true;} break;
        case I_BANDAGE: if(removeItem(id,1)){p.hp=std::min(p.maxHp,p.hp+25);msg="绷带+25生命";used=true;} break;
    }
    if(used) showToast(msg);
    else if(getCount(id)>0) showToast(msg);
}
void tryCraft(int idx){
    if(idx<0||idx>=RECIPE_COUNT) return;
    Recipe& r=RECIPES[idx];
    for(int i=0;i<4;i++){ if(r.req[i][0]<0) break; if(getCount(r.req[i][0])<r.req[i][1]){ showToast("材料不足"); return; } }
    for(int i=0;i<4;i++){ if(r.req[i][0]<0) break; removeItem(r.req[i][0], r.req[i][1]); }
    addItem(r.give[0], r.give[1]);
    if(r.give[0]==I_RSUIT) g->p.hasRadSuit=true;
    char buf[80]; snprintf(buf,80,"制作: %s",r.name); showToast(buf);
}

// ==================== 更新 ====================
void update(float dt){
    // 开屏动画
    if(g->state==STATE_SPLASH){
        g->splashTimer+=dt;
        if(g->splashTimer>2.5f) g->state=STATE_MAIN;
        g->gameTime+=dt;
        return;
    }
    if(g->state!=STATE_PLAYING) { g->gameTime+=dt; return; }

    Player& p=g->p;
    float tvx=0;
    if(g->touchLeft) tvx=-MOVE_SPD;
    if(g->touchRight) tvx=MOVE_SPD;
    if(tvx!=0) p.facing = tvx>0?1:-1;
    p.vx += (tvx-p.vx)*std::min(dt*12.f,1.f);
    if(g->touchJump && p.onGround){ p.vy=JUMP_V; p.onGround=false; }
    p.vy += GRAVITY*dt;
    if(p.vy>700) p.vy=700;
    int ptx=(int)(p.x/TILE), pty=(int)((p.y-p.h/2)/TILE);
    p.inWater = (getTileAt(ptx,pty)==T_WATER);
    float nx=p.x+p.vx*dt;
    float left=nx-p.w/2, right=nx+p.w/2;
    float top=p.y-p.h+2, bot=p.y-2;
    if(p.vx<0 && (isSolid(left,top)||isSolid(left,bot))){ nx=((int)(left/TILE)+1)*TILE+p.w/2; p.vx=0; }
    if(p.vx>0 && (isSolid(right,top)||isSolid(right,bot))){ nx=(int)(right/TILE)*TILE-p.w/2; p.vx=0; }
    p.x=nx;
    float ny=p.y+p.vy*dt;
    left=p.x-p.w/2+2; right=p.x+p.w/2-2;
    top=ny-p.h; bot=ny;
    p.onGround=false;
    if(p.vy<0 && (isSolid(left,top)||isSolid(right,top))){ ny=((int)(top/TILE)+1)*TILE+p.h; p.vy=0; }
    if(p.vy>0 && (isSolid(left,bot)||isSolid(right,bot))){ ny=(int)(bot/TILE)*TILE; p.vy=0; p.onGround=true; }
    p.y=ny;
    if(p.inWater){ p.vx*=0.8f; if(p.vy>50)p.vy*=0.6f; p.onGround=false; }
    if(p.inWater) p.rad=std::min(p.maxRad,p.rad+(p.hasRadSuit?2.f:8.f)*dt);
    else p.rad=std::max(0.f,p.rad-2.f*dt);
    if(p.rad>=p.maxRad) p.hp-=5.f*dt;
    p.hunger=std::max(0.f,p.hunger-1.2f*dt);
    p.thirst=std::max(0.f,p.thirst-1.8f*dt);
    if(p.hunger<=0) p.hp-=3.f*dt;
    if(p.thirst<=0) p.hp-=4.f*dt;
    if(p.invinceTimer>0) p.invinceTimer-=dt;
    g->dayNight=(sinf(g->gameTime*0.08f)+1.f)/2.f;
    p.animTimer+=dt;
    if(std::abs(p.vx)>20 && p.onGround){ if(p.animTimer>0.12f){ p.animTimer=0; p.animFrame=(p.animFrame+1)%4; } }
    else if(!p.onGround) p.animFrame=5;
    else p.animFrame=0;
    for(int i=0;i<g->clueCount;i++){
        if(g->clues[i].found) continue;
        float dx=p.x-(g->clues[i].x*TILE+TILE/2.f);
        float dy=(p.y-p.h/2)-(g->clues[i].y*TILE+TILE/2.f);
        if(std::sqrt(dx*dx+dy*dy)<TILE*2.5f){ g->clues[i].found=true; g->foundClues++; showToast(g->clues[i].text); }
    }
    // 挖掘(按钮)
    if(g->mineHold==1){
        int cx=(int)((p.x+p.facing*TILE*1.5f)/TILE);
        int cy=(int)((p.y-p.h/2)/TILE);
        int t=getTileAt(cx,cy);
        if(t==0){ cy=(int)(p.y/TILE); t=getTileAt(cx,cy); }
        if(t!=0){
            float dx=(cx*TILE+TILE/2.f)-p.x, dy=(cy*TILE+TILE/2.f)-(p.y-p.h/2);
            if(std::sqrt(dx*dx+dy*dy)<TILE*3.f){
                if(g->mineTX!=cx||g->mineTY!=cy){ g->mineTX=cx; g->mineTY=cy; g->mineProg=0; }
                g->mineProg+=dt;
                if(g->mineProg>=mineTime(t)){ completeMine(cx,cy,t); g->mineTX=-1; g->mineProg=0; }
            } else { g->mineTX=-1; g->mineProg=0; }
        } else { g->mineTX=-1; g->mineProg=0; }
    } else if(g->mineHold==2){
        int cx=(int)(g->canvasMineX/TILE), cy=(int)(g->canvasMineY/TILE);
        int t=getTileAt(cx,cy);
        if(t!=0){
            float dx=(cx*TILE+TILE/2.f)-p.x, dy=(cy*TILE+TILE/2.f)-(p.y-p.h/2);
            if(std::sqrt(dx*dx+dy*dy)<TILE*3.5f){
                if(g->mineTX!=cx||g->mineTY!=cy){ g->mineTX=cx; g->mineTY=cy; g->mineProg=0; }
                g->mineProg+=dt;
                if(g->mineProg>=mineTime(t)){ completeMine(cx,cy,t); g->mineTX=-1; g->mineProg=0; }
            } else { g->mineTX=-1; g->mineProg=0; }
        } else { g->mineTX=-1; g->mineProg=0; }
    } else { g->mineTX=-1; g->mineProg=0; }
    for(int i=(int)g->parts.size()-1;i>=0;i--){
        Particle& pa=g->parts[i];
        pa.x+=pa.vx*dt; pa.y+=pa.vy*dt; pa.vy+=300.f*dt; pa.life-=dt;
        if(pa.life<=0) g->parts.erase(g->parts.begin()+i);
    }
    // 摄像机用虚拟分辨率
    float tcx=p.x-VW/2.f, tcy=p.y-VH/2.f-30.f;
    g->camX += (tcx-g->camX)*0.1f;
    g->camY += (tcy-g->camY)*0.1f;
    if(g->camX<0) g->camX=0;
    if(g->camX>WORLD_W*TILE-VW) g->camX=WORLD_W*TILE-VW;
    if(g->camY<0) g->camY=0;
    if(g->camY>WORLD_H*TILE-VH) g->camY=WORLD_H*TILE-VH;
    if(p.hp<=0){ showToast("你死了...重新开始"); initGame(); }
    if(g->toast>0) g->toast-=dt;
}

// ==================== EGL ====================
static void initEGL(ANativeWindow* win){
    g->eglDisplay=eglGetDisplay(EGL_DEFAULT_DISPLAY);
    eglInitialize(g->eglDisplay,0,0);
    const EGLint cfgAttr[]={EGL_SURFACE_TYPE,EGL_WINDOW_BIT,EGL_BLUE_SIZE,8,EGL_GREEN_SIZE,8,EGL_RED_SIZE,8,EGL_NONE};
    EGLConfig cfg; EGLint num;
    eglChooseConfig(g->eglDisplay,cfgAttr,&cfg,1,&num);
    EGLint format; eglGetConfigAttrib(g->eglDisplay,cfg,EGL_NATIVE_VISUAL_ID,&format);
    ANativeWindow_setBuffersGeometry(win,0,0,format);
    g->eglSurface=eglCreateWindowSurface(g->eglDisplay,cfg,win,nullptr);
    const EGLint ctxAttr[]={EGL_CONTEXT_CLIENT_VERSION,2,EGL_NONE};
    g->eglContext=eglCreateContext(g->eglDisplay,cfg,nullptr,ctxAttr);
    eglMakeCurrent(g->eglDisplay,g->eglSurface,g->eglSurface,g->eglContext);
    eglQuerySurface(g->eglDisplay,g->eglSurface,EGL_WIDTH,&g->scrW);
    eglQuerySurface(g->eglDisplay,g->eglSurface,EGL_HEIGHT,&g->scrH);
    setupGL();
    loadTextures();
    loadFont();
    // 计算 letterbox（保持16:9宽高比）
    float targetAspect=(float)VW/VH;
    float screenAspect=(float)g->scrW/g->scrH;
    if(screenAspect>targetAspect){
        g->viewH=g->scrH; g->viewW=(int)(g->scrH*targetAspect);
        g->viewX=(g->scrW-g->viewW)/2; g->viewY=0;
    } else {
        g->viewW=g->scrW; g->viewH=(int)(g->scrW/targetAspect);
        g->viewX=0; g->viewY=(g->scrH-g->viewH)/2;
    }
    g->initialized=true;
    LOGI("EGL init %dx%d view %dx%d+%d+%d",g->scrW,g->scrH,g->viewW,g->viewH,g->viewX,g->viewY);
}
static void termEGL(){
    if(g->eglDisplay!=EGL_NO_DISPLAY){
        eglMakeCurrent(g->eglDisplay,EGL_NO_SURFACE,EGL_NO_SURFACE,EGL_NO_CONTEXT);
        if(g->eglContext) eglDestroyContext(g->eglDisplay,g->eglContext);
        if(g->eglSurface) eglDestroySurface(g->eglDisplay,g->eglSurface);
        eglTerminate(g->eglDisplay);
    }
    g->eglDisplay=EGL_NO_DISPLAY; g->eglContext=EGL_NO_CONTEXT; g->eglSurface=EGL_NO_SURFACE;
}

// ==================== 输入 ====================
// 将屏幕坐标转换为虚拟分辨率坐标
static void screenToVirtual(float& x, float& y){
    x = (x - g->viewX) * VW / g->viewW;
    y = (y - g->viewY) * VH / g->viewH;
}

static int hitBtnIdx(float x,float y){
    for(int i=0;i<8;i++) if(btnHit(BTNS[i],x,y)) return i;
    return -1;
}

static void handleSettingsTap(float x,float y){
    // 设置菜单点击处理
    float ry=y-80;
    if(ry<0) return;
    int row=(int)(ry/30);
    // 行0:音效 行1:音乐 行2:震动 行3:操作模式 行4:像素放大 行5:按键位置
    switch(row){
        case 0: g->settings.soundOn=!g->settings.soundOn; break;
        case 1: g->settings.musicOn=!g->settings.musicOn; break;
        case 2: g->settings.vibrationOn=!g->settings.vibrationOn; break;
        case 3: g->settings.touchControl=!g->settings.touchControl; break;
        case 4: g->settings.pixelScale = g->settings.pixelScale>=4?1:g->settings.pixelScale+1; break;
        case 5:
            g->showSettings=false;
            g->customizeControls=true;
            g->state=STATE_PLAYING;
            showToast("拖拽按钮调整位置, 点击空白处完成");
            break;
    }
    // 返回按钮
    if(x>VW-80 && y>VH-30){ g->showSettings=false; g->state=STATE_PAUSED; }
}

static void handlePanelTap(float x,float y){
    if(g->showSettings){ handleSettingsTap(x,y); return; }
    if(g->showMenu){
        if(y<VH*0.45f){ g->showMenu=false; g->state=STATE_PLAYING; }
        else if(y<VH*0.6f){ g->showMenu=false; g->state=STATE_PLAYING; initGame(); }
        else if(y<VH*0.75f){ g->showSettings=true; }
        else { g->showMenu=false; g->state=STATE_MAIN; }
        return;
    }
    if(x<VW*0.05f || x>VW*0.95f || y<VH*0.12f || y>VH*0.88f){ g->showInv=false; g->showCraft=false; g->state=STATE_PLAYING; return; }
    float ry=y-VH*0.2f;
    if(ry<0) return;
    int row=(int)(ry/22);
    if(row<0) row=0;
    if(g->showInv){
        if(row<g->invDisplayCount) tryUseItem(g->invDisplay[row]);
    } else if(g->showCraft){
        if(row<RECIPE_COUNT) tryCraft(row);
    }
}

static int onInput(struct android_app* app, AInputEvent* ev){
    if(AInputEvent_getType(ev)!=AINPUT_EVENT_TYPE_MOTION) return 0;
    int32_t a=AMotionEvent_getAction(ev);
    int action=a & AMOTION_EVENT_ACTION_MASK;
    int idx=(a & AMOTION_EVENT_ACTION_POINTER_INDEX_MASK)>>AMOTION_EVENT_ACTION_POINTER_INDEX_SHIFT;

    if(g->state==STATE_SPLASH){
        if(action==AMOTION_EVENT_ACTION_DOWN){ g->splashTimer=3.0f; }
        return 1;
    }
    if(g->state==STATE_MAIN){
        if(action==AMOTION_EVENT_ACTION_DOWN){
            g->state=STATE_PLAYING;
            initGame();
        }
        return 1;
    }
    if(action==AMOTION_EVENT_ACTION_DOWN||action==AMOTION_EVENT_ACTION_POINTER_DOWN){
        int pid=AMotionEvent_getPointerId(ev,idx);
        float x=AMotionEvent_getX(ev,idx), y=AMotionEvent_getY(ev,idx);
        screenToVirtual(x,y);
        // 按键自定义模式
        if(g->customizeControls){
            // 检查重置按钮
            if(x>=g->resetBtnX && x<=g->resetBtnX+g->resetBtnW && y>=g->resetBtnY && y<=g->resetBtnY+g->resetBtnH){
                // 重置为默认位置
                Btn def[8] = {
                    {10,  220, 60, 45, "←"},
                    {78,  220, 60, 45, "→"},
                    {146, 220, 60, 45, "跳"},
                    {354, 220, 60, 45, "挖"},
                    {420, 220, 50, 45, "用"},
                    {180, 4,   60, 16, "背包"},
                    {244, 4,   60, 16, "制作"},
                    {308, 4,   60, 16, "菜单"},
                };
                for(int i=0;i<8;i++){ BTNS[i].x=def[i].x; BTNS[i].y=def[i].y; g->settings.btnX[i]=0; g->settings.btnY[i]=0; }
                showToast("已重置为默认位置");
                return 1;
            }
            int b=hitBtnIdx(x,y);
            if(b>=0){
                g->dragBtnIdx=b;
                g->dragOffX=x-BTNS[b].x;
                g->dragOffY=y-BTNS[b].y;
            } else {
                // 点击空白处退出自定义模式
                g->customizeControls=false;
                showToast("按键位置已保存");
            }
            return 1;
        }
        if(g->showInv||g->showCraft||g->showMenu||g->showSettings){ handlePanelTap(x,y); return 1; }
        int b=hitBtnIdx(x,y);
        if(b>=0){
            switch(b){
                case 0: g->touchLeft=true; g->touchIdLeft=pid; break;
                case 1: g->touchRight=true; g->touchIdRight=pid; break;
                case 2: g->touchJump=true; g->touchIdJump=pid; break;
                case 3: g->touchMine=true; g->touchIdMine=pid; g->mineHold=1; break;
                case 4: tryInteract(); break;
                case 5: g->showInv=!g->showInv; g->showCraft=false; g->showMenu=false; g->state=g->showInv?STATE_PAUSED:STATE_PLAYING; break;
                case 6: g->showCraft=!g->showCraft; g->showInv=false; g->showMenu=false; g->state=g->showCraft?STATE_PAUSED:STATE_PLAYING; break;
                case 7: g->showMenu=!g->showMenu; g->state=g->showMenu?STATE_PAUSED:STATE_PLAYING; g->showInv=false; g->showCraft=false; break;
            }
        } else {
            g->mineHold=2; g->touchIdCanvas=pid;
            g->canvasMineX=x+g->camX; g->canvasMineY=y+g->camY;
        }
    } else if(action==AMOTION_EVENT_ACTION_UP||action==AMOTION_EVENT_ACTION_POINTER_UP){
        if(g->customizeControls){ g->dragBtnIdx=-1; return 1; }
        int pid=AMotionEvent_getPointerId(ev,idx);
        if(g->touchIdLeft==pid){ g->touchLeft=false; g->touchIdLeft=-1; }
        if(g->touchIdRight==pid){ g->touchRight=false; g->touchIdRight=-1; }
        if(g->touchIdJump==pid){ g->touchJump=false; g->touchIdJump=-1; }
        if(g->touchIdMine==pid){ g->touchMine=false; g->touchIdMine=-1; if(g->mineHold==1)g->mineHold=0; }
        if(g->touchIdCanvas==pid){ g->touchIdCanvas=-1; if(g->mineHold==2)g->mineHold=0; g->mineTX=-1; }
    } else if(action==AMOTION_EVENT_ACTION_MOVE){
        // 按键自定义模式拖拽
        if(g->customizeControls && g->dragBtnIdx>=0){
            float mx=AMotionEvent_getX(ev,idx), my=AMotionEvent_getY(ev,idx);
            screenToVirtual(mx,my);
            BTNS[g->dragBtnIdx].x=mx-g->dragOffX;
            BTNS[g->dragBtnIdx].y=my-g->dragOffY;
            // 保存自定义位置
            g->settings.btnX[g->dragBtnIdx]=BTNS[g->dragBtnIdx].x;
            g->settings.btnY[g->dragBtnIdx]=BTNS[g->dragBtnIdx].y;
            return 1;
        }
        int n=AMotionEvent_getPointerCount(ev);
        for(int i=0;i<n;i++){
            int pid=AMotionEvent_getPointerId(ev,i);
            if(g->touchIdCanvas==pid){
                float mx=AMotionEvent_getX(ev,i), my=AMotionEvent_getY(ev,i);
                screenToVirtual(mx,my);
                g->canvasMineX=mx+g->camX; g->canvasMineY=my+g->camY;
            }
        }
    }
    return 1;
}

// ==================== 命令 ====================
static void onCmd(struct android_app* app, int32_t cmd){
    switch(cmd){
        case APP_CMD_INIT_WINDOW:
            if(app->window){ initEGL(app->window); }
            break;
        case APP_CMD_TERM_WINDOW: termEGL(); break;
        case APP_CMD_GAINED_FOCUS: if(g->state==STATE_PAUSED) g->state=STATE_PLAYING; break;
        case APP_CMD_LOST_FOCUS: if(g->state==STATE_PLAYING) g->state=STATE_PAUSED; break;
        case APP_CMD_SAVE_STATE: break;
    }
}

// ==================== 入口 ====================
extern "C" void android_main(struct android_app* app){
    g=new Game();
    app->onAppCmd=onCmd;
    app->onInputEvent=onInput;
    struct timespec ts; clock_gettime(CLOCK_MONOTONIC,&ts);
    double last=ts.tv_sec+ts.tv_nsec/1e9;
    while(!app->destroyRequested){
        int events; struct android_poll_source* src;
        while(true){
            int r=ALooper_pollOnce(0,nullptr,&events,(void**)&src);
            if(r==ALOOPER_POLL_TIMEOUT || r==ALOOPER_POLL_ERROR) break;
            if(src) src->process(app,src);
            if(app->destroyRequested) break;
        }
        if(g->eglDisplay!=EGL_NO_DISPLAY && g->initialized){
            if(g->state==STATE_PLAYING){
                clock_gettime(CLOCK_MONOTONIC,&ts);
                double now=ts.tv_sec+ts.tv_nsec/1e9;
                float dt=(float)(now-last); last=now;
                if(dt>0.05f) dt=0.05f; if(dt<0)dt=0.016f;
                update(dt);
            } else {
                clock_gettime(CLOCK_MONOTONIC,&ts);
                double now=ts.tv_sec+ts.tv_nsec/1e9;
                float dt=(float)(now-last); last=now;
                if(dt>0.05f) dt=0.05f; if(dt<0)dt=0.016f;
                update(dt); // 开屏/主菜单也需要更新时间
            }
            render();
            eglSwapBuffers(g->eglDisplay,g->eglSurface);
        }
    }
    termEGL();
    delete g; g=nullptr;
}
