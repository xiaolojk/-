// render.cpp - GLES2 渲染（精美版）
#include "game.h"
#include "sprites.h"
#include <GLES2/gl2.h>
#include <cmath>
#include <cstring>

// ==================== 5x7 字体 ====================
static const uint8_t FONT[128][7] = {
    [' ']={0,0,0,0,0,0,0},
    ['!']={0,0,0x1F,0,0,0x1F,0},
    ['.']={0,0,0,0,0,0x0C,0x0C},
    [':']={0,0x0C,0x0C,0,0x0C,0x0C,0},
    ['-']={0,0,0,0x1F,0,0,0},
    ['/']={0x01,0x02,0x04,0x08,0x10,0,0},
    ['+']={0,0x04,0x04,0x1F,0x04,0x04,0},
    ['0']={0x0E,0x11,0x13,0x15,0x19,0x11,0x0E},
    ['1']={0x04,0x0C,0x04,0x04,0x04,0x04,0x0E},
    ['2']={0x0E,0x11,0x01,0x06,0x08,0x10,0x1F},
    ['3']={0x1F,0x01,0x02,0x06,0x01,0x11,0x0E},
    ['4']={0x02,0x06,0x0A,0x12,0x1F,0x02,0x02},
    ['5']={0x1F,0x10,0x1E,0x01,0x01,0x11,0x0E},
    ['6']={0x06,0x08,0x10,0x1E,0x11,0x11,0x0E},
    ['7']={0x1F,0x01,0x02,0x04,0x08,0x08,0x08},
    ['8']={0x0E,0x11,0x11,0x0E,0x11,0x11,0x0E},
    ['9']={0x0E,0x11,0x11,0x0F,0x01,0x02,0x0C},
    ['A']={0x0E,0x11,0x11,0x1F,0x11,0x11,0x11},
    ['B']={0x1E,0x11,0x11,0x1E,0x11,0x11,0x1E},
    ['C']={0x0E,0x11,0x10,0x10,0x10,0x11,0x0E},
    ['D']={0x1E,0x11,0x11,0x11,0x11,0x11,0x1E},
    ['E']={0x1F,0x10,0x10,0x1E,0x10,0x10,0x1F},
    ['F']={0x1F,0x10,0x10,0x1E,0x10,0x10,0x10},
    ['G']={0x0E,0x11,0x10,0x17,0x11,0x11,0x0F},
    ['H']={0x11,0x11,0x11,0x1F,0x11,0x11,0x11},
    ['I']={0x0E,0x04,0x04,0x04,0x04,0x04,0x0E},
    ['J']={0x01,0x01,0x01,0x01,0x11,0x11,0x0E},
    ['K']={0x11,0x12,0x14,0x18,0x14,0x12,0x11},
    ['L']={0x10,0x10,0x10,0x10,0x10,0x10,0x1F},
    ['M']={0x11,0x1B,0x15,0x15,0x11,0x11,0x11},
    ['N']={0x11,0x11,0x19,0x15,0x13,0x11,0x11},
    ['O']={0x0E,0x11,0x11,0x11,0x11,0x11,0x0E},
    ['P']={0x1E,0x11,0x11,0x1E,0x10,0x10,0x10},
    ['Q']={0x0E,0x11,0x11,0x11,0x15,0x12,0x0D},
    ['R']={0x1E,0x11,0x11,0x1E,0x14,0x12,0x11},
    ['S']={0x0F,0x10,0x10,0x0E,0x01,0x11,0x1E},
    ['T']={0x1F,0x04,0x04,0x04,0x04,0x04,0x04},
    ['U']={0x11,0x11,0x11,0x11,0x11,0x11,0x0E},
    ['V']={0x11,0x11,0x11,0x11,0x11,0x0A,0x04},
    ['W']={0x11,0x11,0x11,0x15,0x15,0x15,0x0A},
    ['X']={0x11,0x11,0x0A,0x04,0x0A,0x11,0x11},
    ['Y']={0x11,0x11,0x0A,0x04,0x04,0x04,0x04},
    ['Z']={0x1F,0x01,0x02,0x04,0x08,0x10,0x1F},
};

// ==================== 着色器 ====================
void setupGL(){
    const char* vs=
        "attribute vec2 aPos;attribute vec2 aUV;attribute vec4 aColor;"
        "varying vec2 vUV;varying vec4 vCol;uniform float uW,uH;"
        "void main(){vUV=aUV;vCol=aColor;"
        "gl_Position=vec4((aPos.x/uW-0.5)*2.0,(0.5-aPos.y/uH)*2.0,0.0,1.0);}";
    const char* fs=
        "precision mediump float;varying vec2 vUV;varying vec4 vCol;"
        "uniform sampler2D sTex;void main(){gl_FragColor=texture2D(sTex,vUV)*vCol;}";
    auto mk=[&](int t,const char* s){ GLuint sh=glCreateShader(t); glShaderSource(sh,1,&s,0); glCompileShader(sh); return sh; };
    GLuint v=mk(GL_VERTEX_SHADER,vs), f=mk(GL_FRAGMENT_SHADER,fs);
    g->prog=glCreateProgram(); glAttachShader(g->prog,v); glAttachShader(g->prog,f); glLinkProgram(g->prog);
    glUseProgram(g->prog);
    g->aPos=glGetAttribLocation(g->prog,"aPos");
    g->aUV=glGetAttribLocation(g->prog,"aUV");
    g->aColor=glGetAttribLocation(g->prog,"aColor");
    g->uTex=glGetUniformLocation(g->prog,"sTex");
    g->uW=glGetUniformLocation(g->prog,"uW"); g->uH=glGetUniformLocation(g->prog,"uH");
    glUniform1i(g->uTex,0);
    glUniform1f(g->uW,(float)g->scrW); glUniform1f(g->uH,(float)g->scrH);
    glEnableVertexAttribArray(g->aPos); glEnableVertexAttribArray(g->aUV); glEnableVertexAttribArray(g->aColor);
    glEnable(GL_BLEND); glBlendFunc(GL_SRC_ALPHA,GL_ONE_MINUS_SRC_ALPHA);
    glDisable(GL_DEPTH_TEST);
    uint8_t w[4]={255,255,255,255};
    glGenTextures(1,&g->texWhite); glBindTexture(GL_TEXTURE_2D,g->texWhite);
    glTexImage2D(GL_TEXTURE_2D,0,GL_RGBA,1,1,0,GL_RGBA,GL_UNSIGNED_BYTE,w);
    glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MIN_FILTER,GL_NEAREST);
    glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MAG_FILTER,GL_NEAREST);
}

static GLuint uploadCanvas(Canvas* c){
    GLuint t; glGenTextures(1,&t); glBindTexture(GL_TEXTURE_2D,t);
    glTexImage2D(GL_TEXTURE_2D,0,GL_RGBA,c->w,c->h,0,GL_RGBA,GL_UNSIGNED_BYTE,c->px);
    glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MIN_FILTER,GL_NEAREST);
    glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MAG_FILTER,GL_NEAREST);
    glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_WRAP_S,GL_CLAMP_TO_EDGE);
    glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_WRAP_T,GL_CLAMP_TO_EDGE);
    delete c; return t;
}

void loadTextures(){
    g->texPlayer[0]=uploadCanvas(makePlayer(0));
    g->texPlayer[1]=uploadCanvas(makePlayer(1));
    g->texPlayer[2]=uploadCanvas(makePlayer(2));
    g->texPlayer[3]=uploadCanvas(makePlayer(3));
    g->texPlayer[4]=uploadCanvas(makePlayer(4));
    g->texPlayer[5]=uploadCanvas(makePlayer(5));
    g->texGrass=uploadCanvas(makeGrass());
    g->texDirt=uploadCanvas(makeDirt());
    g->texStone=uploadCanvas(makeStone());
    g->texIron=uploadCanvas(makeOre(0));
    g->texCoal=uploadCanvas(makeOre(1));
    g->texGold=uploadCanvas(makeOre(2));
    g->texWater=uploadCanvas(makeWater());
    g->texTree=uploadCanvas(makeTree());
    g->texBerry=uploadCanvas(makeBerry());
    g->texClue=uploadCanvas(makeClue());
}

// ==================== 绘制原语 ====================
static void drawQuad(GLuint tex,float dx,float dy,float dw,float dh,bool flip,
                     float r,float gg,float b,float a){
    float u0=flip?1.f:0.f, u1=flip?0.f:1.f;
    float v[]={ dx,dy,u0,0,r,gg,b,a, dx+dw,dy,u1,0,r,gg,b,a,
                dx,dy+dh,u0,1,r,gg,b,a, dx+dw,dy+dh,u1,1,r,gg,b,a };
    glBindTexture(GL_TEXTURE_2D,tex);
    glVertexAttribPointer(g->aPos,2,GL_FLOAT,GL_FALSE,32,v);
    glVertexAttribPointer(g->aUV,2,GL_FLOAT,GL_FALSE,32,v+2);
    glVertexAttribPointer(g->aColor,4,GL_FLOAT,GL_FALSE,32,v+4);
    glDrawArrays(GL_TRIANGLE_STRIP,0,4);
}
static void drawQuadG(float dx,float dy,float dw,float dh,
  float r0,float g0,float b0,float a0,float r1,float g1,float b1,float a1){
    float v[]={ dx,dy,0,0,r0,g0,b0,a0, dx+dw,dy,1,0,r0,g0,b0,a0,
                dx,dy+dh,0,1,r1,g1,b1,a1, dx+dw,dy+dh,1,1,r1,g1,b1,a1 };
    glBindTexture(GL_TEXTURE_2D,g->texWhite);
    glVertexAttribPointer(g->aPos,2,GL_FLOAT,GL_FALSE,32,v);
    glVertexAttribPointer(g->aUV,2,GL_FLOAT,GL_FALSE,32,v+2);
    glVertexAttribPointer(g->aColor,4,GL_FLOAT,GL_FALSE,32,v+4);
    glDrawArrays(GL_TRIANGLE_STRIP,0,4);
}
static void rect(float x,float y,float w,float h,float r,float gg,float b,float a){
    drawQuad(g->texWhite,x,y,w,h,false,r,gg,b,a);
}
static void sprite(GLuint t,float x,float y,float w,float h,bool flip=false){
    drawQuad(t,x,y,w,h,flip,1,1,1,1);
}
static void text(float x,float y,const char* s,float sc,float r,float gg,float b){
    float cx=x;
    for(const unsigned char* p=(const unsigned char*)s; *p; p++){
        unsigned char c=*p;
        if(c>=128){ cx+=5*sc; continue; }
        if(c==' '){ cx+=5*sc; continue; }
        const uint8_t* gl=FONT[c];
        if(gl[0]||gl[1]||gl[2]||gl[3]||gl[4]||gl[5]||gl[6]){
            for(int row=0;row<7;row++){
                uint8_t bits=gl[row];
                for(int col=0;col<5;col++) if(bits&(1<<(4-col)))
                    rect(cx+col*sc,y+row*sc,sc,sc,r,gg,b,1);
            }
        }
        cx+=6*sc;
    }
}
static void num(float x,float y,int v,float sc,float r,float gg,float b){
    char buf[16]; snprintf(buf,16,"%d",v); text(x,y,buf,sc,r,gg,b);
}

// ==================== 世界渲染 ====================
static void renderWorld(){
    int sx=std::max(0,(int)(g->camX/TILE)-1);
    int sy=std::max(0,(int)(g->camY/TILE)-1);
    int ex=std::min(WORLD_W,(int)((g->camX+g->scrW)/TILE)+1);
    int ey=std::min(WORLD_H,(int)((g->camY+g->scrH)/TILE)+1);
    for(int y=sy;y<ey;y++) for(int x=sx;x<ex;x++){
        int t=g->tiles[y][x];
        int w=g->walls[y][x];
        float px=x*TILE-g->camX, py=y*TILE-g->camY;
        if(t){
            switch(t){
                case T_GRASS: sprite(g->texGrass,px,py,TILE,TILE); break;
                case T_DIRT: sprite(g->texDirt,px,py,TILE,TILE); break;
                case T_STONE: sprite(g->texStone,px,py,TILE,TILE); break;
                case T_IRON: sprite(g->texIron,px,py,TILE,TILE); break;
                case T_COAL: sprite(g->texCoal,px,py,TILE,TILE); break;
                case T_GOLD: sprite(g->texGold,px,py,TILE,TILE); break;
                case T_WATER:
                    drawQuad(g->texWater,px,py,TILE,TILE,false,
                        1,1,1,0.7f+0.2f*sinf(g->gameTime*2+x*0.5f+y*0.3f));
                    break;
            }
        } else if(y>SURFACE_Y){
            // 地下渐变背景
            float depth = (y - SURFACE_Y) / (float)(WORLD_H - SURFACE_Y);
            float br = 0.12f - depth * 0.08f;
            float bg = 0.06f - depth * 0.04f;
            float bb = 0.03f - depth * 0.02f;
            rect(px,py,TILE,TILE, br, bg, bb, 1);
        }
        if(w==W_TREE){ sprite(g->texTree,px,py-TILE,TILE,TILE*2); }
        else if(w==W_BERRY){ sprite(g->texBerry,px,py,TILE,TILE); }
        else if(w==W_CLUE){
            float pulse=0.7f+0.3f*sinf(g->gameTime*3);
            drawQuad(g->texClue,px,py,TILE,TILE,false,pulse,pulse,pulse,pulse);
            text(px+10,py+8,"?",2.2f,0.3f,0.2f,0.1f);
        }
    }
    // 挖掘高亮
    if(g->mineTX>=0){
        float mx=g->mineTX*TILE-g->camX, my=g->mineTY*TILE-g->camY;
        float glow = 0.3f + 0.1f*sinf(g->gameTime*8);
        rect(mx,my,TILE,TILE,1,0.7f,0.2f,glow);
        int t=getTileAt(g->mineTX,g->mineTY);
        float pct=g->mineProg/mineTime(t);
        float dk=pct*0.7f;
        rect(mx+6,my+6,4,1,0,0,0,dk);
        rect(mx+14,my+12,6,1,0,0,0,dk);
        rect(mx+8,my+18,5,1,0,0,0,dk);
        rect(mx+18,my+22,4,1,0,0,0,dk);
    }
}

static void renderPlayer(){
    Player& p=g->p;
    int ti;
    if(!p.onGround) ti=5;
    else if(std::abs(p.vx)>20) ti=1+(p.animFrame%4);
    else ti=0;
    float px=p.x-16-g->camX, py=p.y-48-g->camY;
    bool flip = p.facing<0;
    if(p.invinceTimer>0 && ((int)(p.invinceTimer*10))%2==0) return;
    sprite(g->texPlayer[ti],px,py,32,48,flip);
    if(p.hasRadSuit){
        rect(px+4,py,24,48,1,0.85f,0.25f,0.25f);
    }
}

static void renderParticles(){
    for(auto& pa:g->parts){
        float a=std::min(1.f,pa.life/0.4f);
        float sz = 3.0f + pa.life * 2.0f;
        rect(pa.x-sz-g->camX, pa.y-sz-g->camY, sz*2, sz*2, pa.r/255.f, pa.g/255.f, pa.b/255.f, a);
    }
}

// ==================== UI ====================
static void drawBtn(const Btn& b,bool active){
    float x=b.x*g->scrW, y=b.y*g->scrH, w=b.w*g->scrW, h=b.h*g->scrH;
    rect(x,y,w,h,0,0,0,0.55f);
    float cr = active?0.35f:0.18f, cg = active?0.55f:0.18f, cb = active?0.75f:0.22f;
    rect(x+2,y+2,w-4,h-4, cr, cg, cb, 0.85f);
    // 顶部高光
    rect(x+2,y+2,w-4,3, cr+0.15f, cg+0.15f, cb+0.15f, 0.3f);
    // 边框
    rect(x,y,w,2,0.4f,0.7f,0.95f,0.7f);
    rect(x,y+h-2,w,2,0.4f,0.7f,0.95f,0.7f);
    rect(x,y,2,h,0.4f,0.7f,0.95f,0.7f);
    rect(x+w-2,y,2,h,0.4f,0.7f,0.95f,0.7f);
    float ts = h*0.35f;
    float tw = strlen(b.label)*6*ts;
    text(x+(w-tw)/2, y+(h-7*ts)/2, b.label, ts, 1,1,1);
}

static void renderHUD(){
    float pad = g->scrW * 0.008f;
    float barW = g->scrW * 0.18f;
    float barX = pad;
    float barH = 16;
    auto bar=[&](const char* lbl,float v,float m,float r,float gg,float b,float y){
        rect(barX, y, barW, barH, 0, 0, 0, 0.65f);
        rect(barX+2, y+2, barW-4, 3, 0.1f, 0.1f, 0.1f, 1);
        float pct = v / m;
        if(pct > 0) rect(barX+2, y+2, (barW-4)*pct, 3, r, gg, b, 1);
        if(pct < 0.25f && sinf(g->gameTime*6)>0) rect(barX+2, y+2, (barW-4)*pct, 3, 1, 0.1f, 0.1f, 0.7f);
        text(barX+4, y+7, lbl, 1.2f, 1, 1, 1);
        num(barX + barW - 32, y+7, (int)v, 1.2f, 1, 1, 1);
    };
    bar("HP", g->p.hp, g->p.maxHp, 0.95f, 0.2f, 0.2f, pad);
    bar("HUNGER", g->p.hunger, g->p.maxHunger, 1, 0.6f, 0, pad + 20);
    bar("THIRST", g->p.thirst, g->p.maxThirst, 0.15f, 0.65f, 1, pad + 40);
    bar("RAD", g->p.rad, g->p.maxRad, 1, 0.85f, 0.1f, pad + 60);
    
    // 水警告
    if(g->p.inWater){
        float wx = g->scrW/2 - 100;
        float wy = g->scrH - 170;
        rect(wx-5, wy-5, 210, 28, 0, 0, 0, 0.7f);
        rect(wx, wy, 200, 18, 0.9f, 0.15f, 0.05f, 0.8f);
        text(wx + 10, wy + 4, "RADIATION WATER!", 1.6f, 1, 0.9f, 0.7f);
    }
    
    // 线索计数
    float cx = g->scrW - 130;
    rect(cx, pad, 122, 18, 0, 0, 0, 0.65f);
    text(cx + 4, pad + 4, "CLUES:", 1.3f, 1, 0.85f, 0.3f);
    num(cx + 60, pad + 4, g->foundClues, 1.3f, 1, 0.85f, 0.3f);
    text(cx + 78, pad + 4, "/", 1.3f, 0.7f, 0.7f, 0.7f);
    num(cx + 88, pad + 4, g->clueCount, 1.3f, 0.7f, 0.7f, 0.7f);
}

static const char* ITEM_NAME(int id){
    switch(id){
        case I_DIRT:return"DIRT";case I_STONE:return"STONE";case I_WOOD:return"WOOD";
        case I_PLANK:return"PLANK";case I_STICK:return"STICK";case I_PICK:return"PICK";
        case I_TORCH:return"TORCH";case I_CAMP:return"FIRE";case I_BANDAGE:return"BAND";
        case I_BERRY:return"BERRY";case I_CBERRY:return"C.BRY";case I_FWATER:return"WATER";
        case I_PWATER:return"P.WTR";case I_IRON:return"IRON";case I_GOLD:return"GOLD";
        case I_COAL:return"COAL";case I_INGOT:return"INGOT";case I_IPICK:return"I.PICK";
        case I_RAFT:return"RAFT";case I_RSUIT:return"SUIT";case I_FILTER:return"FILT";
    }
    return"?";
}

static void renderPanel(){
    float sw=g->scrW, sh=g->scrH;
    float px = sw*0.06f, py = sh*0.12f, pw = sw*0.88f, ph = sh*0.76f;
    rect(px, py, pw, ph, 0.02f, 0.06f, 0.14f, 0.94f);
    // 顶部装饰线
    rect(px, py, pw, 3, 0.25f, 0.7f, 0.95f, 1);
    rect(px, py, 3, ph, 0.25f, 0.7f, 0.95f, 1);
    float ry=sh*0.2f, lh=sh*0.065f;
    if(g->showInv){
        text(sw*0.1f, sh*0.14f, "INVENTORY", 2.2f, 0.25f, 0.7f, 0.95f);
        g->invDisplayCount=0;
        int row=0;
        for(int id=1;id<I_COUNT;id++){
            if(g->inv[id]>0){
                float yy=ry+row*lh;
                if(row%2) rect(sw*0.08f, yy, sw*0.84f, lh, 1, 1, 1, 0.04f);
                text(sw*0.1f, yy+lh*0.2f, ITEM_NAME(id), 1.6f, 0.8f, 0.85f, 1);
                num(sw*0.58f, yy+lh*0.2f, g->inv[id], 1.6f, 1, 1, 1);
                bool useable=(id==I_BERRY||id==I_CBERRY||id==I_FWATER||id==I_PWATER||id==I_BANDAGE);
                if(useable) text(sw*0.7f, yy+lh*0.2f, "TAP USE", 1.4f, 0.3f, 1, 0.3f);
                if(g->invDisplayCount<24) g->invDisplay[g->invDisplayCount++]=id;
                row++;
                if(row>10) break;
            }
        }
        if(row==0) text(sw*0.3f, sh*0.4f, "EMPTY", 2.5f, 0.5f, 0.5f, 0.5f);
    } else if(g->showCraft){
        text(sw*0.1f, sh*0.14f, "CRAFTING", 2.2f, 0.25f, 0.7f, 0.95f);
        for(int i=0;i<RECIPE_COUNT && i<11;i++){
            Recipe& r=RECIPES[i];
            float yy=ry+i*lh;
            bool can=true;
            for(int k=0;k<4;k++){ if(r.req[k][0]<0)break; if(g->inv[r.req[k][0]]<r.req[k][1]) can=false; }
            if(i%2) rect(sw*0.08f, yy, sw*0.84f, lh, 1, 1, 1, 0.04f);
            float col = can?1:0.4f;
            text(sw*0.1f, yy+lh*0.2f, r.name, 1.5f, col, col*0.85f, col*0.4f);
            char buf[64]; buf[0]=0;
            for(int k=0;k<4;k++){ if(r.req[k][0]<0)break;
                char b2[16]; snprintf(b2,16,"%s%d ",ITEM_NAME(r.req[k][0]),r.req[k][1]); strcat(buf,b2);
            }
            text(sw*0.4f, yy+lh*0.2f, buf, 1.2f, 0.7f, 0.7f, 0.7f);
            if(can) text(sw*0.82f, yy+lh*0.2f, "TAP", 1.4f, 0.3f, 1, 0.3f);
        }
    } else if(g->showMenu){
        text(sw*0.32f, sh*0.22f, "PAUSED", 3.5f, 0.25f, 0.7f, 0.95f);
        rect(sw*0.2f, sh*0.38f, sw*0.6f, sh*0.06f, 0.15f, 0.5f, 0.25f, 0.85f);
        text(sw*0.4f, sh*0.4f, "RESUME", 2, 1, 1, 1);
        rect(sw*0.2f, sh*0.48f, sw*0.6f, sh*0.06f, 0.5f, 0.35f, 0.15f, 0.85f);
        text(sw*0.4f, sh*0.5f, "RESTART", 2, 1, 1, 1);
        rect(sw*0.2f, sh*0.58f, sw*0.6f, sh*0.06f, 0.25f, 0.25f, 0.25f, 0.85f);
        text(sw*0.42f, sh*0.6f, "QUIT", 2, 1, 1, 1);
    }
}

// ==================== 主渲染 ====================
void render(){
    glViewport(0,0,g->scrW,g->scrH);
    glClear(GL_COLOR_BUFFER_BIT);
    glUniform1f(g->uW,(float)g->scrW); glUniform1f(g->uH,(float)g->scrH);
    
    // 天空渐变
    float dn = g->dayNight;
    if(dn > 0.5f){
        // 白天天空
        drawQuadG(0,0,g->scrW,g->scrH*0.7f, 0.28f,0.65f,0.92f,1, 0.65f,0.88f,1,1);
        // 云朵
        for(int i=0;i<6;i++){
            float cx = fmodf(g->scrW*0.15f*i + g->gameTime*15, g->scrW+200) - 100;
            float cy = 40 + i*35;
            float ca = 0.3f + 0.15f*sinf(g->gameTime*0.5f+i);
            rect(cx-20, cy, 60, 12, 1, 1, 1, ca);
            rect(cx-10, cy-6, 50, 10, 1, 1, 1, ca*0.8f);
            rect(cx, cy-10, 30, 8, 1, 1, 1, ca*0.6f);
        }
    } else {
        // 夜晚天空
        drawQuadG(0,0,g->scrW,g->scrH*0.7f, 0.03f,0.07f,0.14f,1, 0.08f,0.18f,0.32f,1);
        // 星星
        for(int i=0;i<60;i++){
            int sx=(i*7919)%g->scrW, sy=(i*6271)%(int)(g->scrH*0.5f);
            float tw=0.4f+0.6f*sinf(g->gameTime*1.5f+i*0.7f);
            rect(sx, sy, 2, 2, 1, 1, 1, tw*(0.5f-dn));
        }
    }
    // 太阳/月亮
    float celX = g->scrW - 70;
    if(dn > 0.6f){
        // 太阳
        float sr = 22 + 3*sinf(g->gameTime*0.3f);
        rect(celX-sr, 35, sr*2, sr*2, 1, 0.85f, 0.25f, 0.6f);
        rect(celX-sr+4, 39, sr*2-8, sr*2-8, 1, 0.92f, 0.4f, 0.5f);
    } else if(dn < 0.4f){
        float mr = 15;
        rect(celX-mr, 38, mr*2, mr*2, 0.85f, 0.85f, 0.9f, 0.8f);
        rect(celX-mr+2, 40, mr*2-4, mr*2-4, 0.92f, 0.92f, 0.95f, 0.7f);
    }

    if(g->showMain){
        // 主菜单（精美版）
        drawQuadG(0,0,g->scrW,g->scrH, 0.03f,0.08f,0.16f,1, 0.06f,0.15f,0.28f,1);
        float cx=g->scrW/2;
        // 装饰线
        rect(cx-180, g->scrH*0.28f, 360, 2, 0.25f, 0.7f, 0.95f, 0.6f);
        text(cx-170, g->scrH*0.3f, "LOST BLUE SEA", 4.5f, 0.25f, 0.7f, 0.95f);
        rect(cx-180, g->scrH*0.38f, 360, 2, 0.25f, 0.7f, 0.95f, 0.6f);
        text(cx-65, g->scrH*0.42f, "ORGC", 2.2f, 0.5f, 0.5f, 0.55f);
        text(cx-155, g->scrH*0.48f, "SURVIVAL ADVENTURE", 1.6f, 0.4f, 0.5f, 0.5f);
        // 开始按钮
        float bw = 220, bh = 55;
        float bx = cx - bw/2, by = g->scrH*0.58f;
        float pulse = 0.9f + 0.1f*sinf(g->gameTime*3);
        rect(bx-2, by-2, bw+4, bh+4, 0.25f, 0.7f, 0.95f, pulse*0.5f);
        rect(bx, by, bw, bh, 0.15f, 0.5f, 0.78f, 0.9f);
        rect(bx, by, bw, 4, 0.3f, 0.75f, 0.95f, 0.4f);
        text(cx-55, by+14, "TAP TO START", 2.2f, 1, 1, 1);
        text(cx-130, g->scrH*0.88f, "V5.0  HD EDITION", 1.4f, 0.35f, 0.35f, 0.35f);
        return;
    }

    renderWorld();
    renderPlayer();
    renderParticles();

    // 夜晚遮罩
    if(dn < 0.35f) rect(0,0,g->scrW,g->scrH,0,0,0.08f,0.4f-dn);

    renderHUD();
    drawBtn(BTNS[0],g->touchLeft);
    drawBtn(BTNS[1],g->touchRight);
    drawBtn(BTNS[2],g->touchJump);
    drawBtn(BTNS[3],g->touchMine);
    drawBtn(BTNS[4],false);
    drawBtn(BTNS[5],g->showInv);
    drawBtn(BTNS[6],g->showCraft);
    drawBtn(BTNS[7],g->showMenu);
    
    // 挖掘进度条
    if(g->mineTX>=0){
        int t=getTileAt(g->mineTX,g->mineTY);
        float pct=std::min(1.f,g->mineProg/mineTime(t));
        float pw = 120;
        rect(g->scrW/2-pw/2, g->scrH-145, pw, 12, 0, 0, 0, 0.7f);
        rect(g->scrW/2-pw/2+2, g->scrH-143, (pw-4)*pct, 8, 1, 0.55f, 0, 1);
        text(g->scrW/2-20, g->scrH-130, "MINING", 1.3f, 1, 0.7f, 0.3f);
    }
    
    // toast
    if(g->toast>0){
        float tw=strnlen(g->toastText,40)*6*1.6f;
        float ty = g->scrH-200;
        rect(g->scrW/2-tw/2-12, ty-2, tw+24, 28, 0, 0, 0, 0.85f);
        rect(g->scrW/2-tw/2-12, ty-2, tw+24, 2, 0.25f, 0.7f, 0.95f, 0.8f);
        text(g->scrW/2-tw/2, ty+4, g->toastText, 1.6f, 0.25f, 0.7f, 0.95f);
    }
    
    if(g->showInv||g->showCraft||g->showMenu) renderPanel();
}