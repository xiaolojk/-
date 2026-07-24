// render.cpp - GLES2 渲染 (v9.0 系统字体版)
#include "game.h"
#include "sprites.h"
#include <GLES2/gl2.h>
#include <cmath>
#include <cstring>
#include <jni.h>
#include <android/log.h>
#include <android_native_app_glue.h>

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
    glEnableVertexAttribArray(g->aPos); glEnableVertexAttribArray(g->aUV); glEnableVertexAttribArray(g->aColor);
    glEnable(GL_BLEND); glBlendFunc(GL_SRC_ALPHA,GL_ONE_MINUS_SRC_ALPHA);
    glDisable(GL_DEPTH_TEST);
    uint8_t w[4]={255,255,255,255};
    glGenTextures(1,&g->texWhite); glBindTexture(GL_TEXTURE_2D,g->texWhite);
    glTexImage2D(GL_TEXTURE_2D,0,GL_RGBA,1,1,0,GL_RGBA,GL_UNSIGNED_BYTE,w);
    glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MIN_FILTER,GL_NEAREST);
    glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MAG_FILTER,GL_NEAREST);

    // 创建FBO (480x270虚拟分辨率)
    glGenTextures(1,&g->fboTex); glBindTexture(GL_TEXTURE_2D,g->fboTex);
    glTexImage2D(GL_TEXTURE_2D,0,GL_RGBA,VW,VH,0,GL_RGBA,GL_UNSIGNED_BYTE,nullptr);
    glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MIN_FILTER,GL_NEAREST);
    glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MAG_FILTER,GL_NEAREST);
    glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_WRAP_S,GL_CLAMP_TO_EDGE);
    glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_WRAP_T,GL_CLAMP_TO_EDGE);
    glGenFramebuffers(1,&g->fbo);
    glBindFramebuffer(GL_FRAMEBUFFER,g->fbo);
    glFramebufferTexture2D(GL_FRAMEBUFFER,GL_COLOR_ATTACHMENT0,GL_TEXTURE_2D,g->fboTex,0);
    glBindFramebuffer(GL_FRAMEBUFFER,0);
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
    for(int i=0;i<6;i++) g->texPlayer[i]=uploadCanvas(makePlayer(i));
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

// ==================== 字体系统 (Android系统字体) ====================
// 通过JNI从Java端获取Android系统字体渲染的图集
static const int MAX_FONT_CHARS = 512;
static uint16_t g_fontUnicodes[MAX_FONT_CHARS];
static int g_fontCount = 0;
static int g_fontCharSize = 32;
static int g_fontCols = 16;
static int g_fontAtlasW = 512;
static int g_fontAtlasH = 1024;

static int fontLookup(uint16_t unicode){
    for(int i=0;i<g_fontCount;i++){
        if(g_fontUnicodes[i]==unicode) return i;
    }
    return -1;
}

void loadFont(){
    // 通过JNI调用Java静态方法获取系统字体数据
    JNIEnv* env = nullptr;

    extern struct android_app* g_app;
    if(!g_app || !g_app->activity->vm){
        __android_log_print(ANDROID_LOG_ERROR, "Game", "No JavaVM available for font");
        return;
    }
    JavaVM* vm = g_app->activity->vm;
    bool attached = false;
    if(vm->GetEnv((void**)&env, JNI_VERSION_1_4) != JNI_OK){
        if(vm->AttachCurrentThread(&env, nullptr) == JNI_OK){
            attached = true;
        } else {
            __android_log_print(ANDROID_LOG_ERROR, "Game", "Failed to attach thread for font");
            return;
        }
    }

    jclass cls = env->FindClass("com/orgc/lostbluesea/GameActivity");
    if(!cls){
        __android_log_print(ANDROID_LOG_ERROR, "Game", "GameActivity class not found");
        if(attached) vm->DetachCurrentThread();
        return;
    }

    // 获取字符列表
    jmethodID getUnicodes = env->GetStaticMethodID(cls, "getFontUnicodes", "()[C");
    jmethodID getRgba = env->GetStaticMethodID(cls, "getFontRgba", "()[B");
    jmethodID getW = env->GetStaticMethodID(cls, "getFontAtlasW", "()I");
    jmethodID getH = env->GetStaticMethodID(cls, "getFontAtlasH", "()I");
    jmethodID getSize = env->GetStaticMethodID(cls, "getFontSize", "()I");
    jmethodID getCols = env->GetStaticMethodID(cls, "getFontCols", "()I");

    if(!getUnicodes || !getRgba || !getW || !getH || !getSize || !getCols){
        __android_log_print(ANDROID_LOG_ERROR, "Game", "Font methods not found");
        if(attached) vm->DetachCurrentThread();
        return;
    }

    jcharArray ju = (jcharArray)env->CallStaticObjectMethod(cls, getUnicodes);
    jbyteArray jr = (jbyteArray)env->CallStaticObjectMethod(cls, getRgba);
    int aw = env->CallStaticIntMethod(cls, getW);
    int ah = env->CallStaticIntMethod(cls, getH);
    int cs = env->CallStaticIntMethod(cls, getSize);
    int cols = env->CallStaticIntMethod(cls, getCols);

    if(!ju || !jr || aw<=0 || ah<=0){
        __android_log_print(ANDROID_LOG_ERROR, "Game", "Font data not available");
        if(attached) vm->DetachCurrentThread();
        return;
    }

    // 提取unicode码点
    jsize count = env->GetArrayLength(ju);
    if(count > MAX_FONT_CHARS) count = MAX_FONT_CHARS;
    jchar* uc = env->GetCharArrayElements(ju, nullptr);
    for(int i=0;i<count;i++) g_fontUnicodes[i] = uc[i];
    env->ReleaseCharArrayElements(ju, uc, JNI_ABORT);

    // 提取RGBA数据
    jbyte* rgba = env->GetByteArrayElements(jr, nullptr);

    g_fontCount = count;
    g_fontCharSize = cs;
    g_fontCols = cols;
    g_fontAtlasW = aw;
    g_fontAtlasH = ah;

    // 生成字体纹理
    if(g->texFont==0) glGenTextures(1,&g->texFont);
    glBindTexture(GL_TEXTURE_2D, g->texFont);
    glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, aw, ah, 0, GL_RGBA, GL_UNSIGNED_BYTE, rgba);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_EDGE);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_EDGE);

    env->ReleaseByteArrayElements(jr, rgba, JNI_ABORT);
    env->DeleteLocalRef(cls);
    if(ju) env->DeleteLocalRef(ju);
    if(jr) env->DeleteLocalRef(jr);

    if(attached) vm->DetachCurrentThread();

    __android_log_print(ANDROID_LOG_INFO, "Game", "System font loaded: %d chars, %dx%d atlas", count, aw, ah);
}

// ==================== 绘制原语 (虚拟分辨率坐标) ====================
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
static void rect(float x,float y,float w,float h,float r,float gg,float b,float a){
    drawQuad(g->texWhite,x,y,w,h,false,r,gg,b,a);
}
static void sprite(GLuint t,float x,float y,float w,float h,bool flip=false){
    drawQuad(t,x,y,w,h,flip,1,1,1,1);
}

// UTF-8中文文本绘制 (Android系统字体)
static void text(float x,float y,const char* s,float sz,float r,float gg,float b,float a=1.f){
    float cx=x;
    for(const unsigned char* p=(const unsigned char*)s; *p; ){
        uint16_t unicode;
        if(*p<0x80){ unicode=*p; p++; }
        else if((*p&0xE0)==0xC0){ unicode=((*p&0x1F)<<6)|(*(p+1)&0x3F); p+=2; }
        else if((*p&0xF0)==0xE0){ unicode=((*p&0x0F)<<12)|((*(p+1)&0x3F)<<6)|(*(p+2)&0x3F); p+=3; }
        else { p++; continue; }
        if(unicode==' '){ cx+=sz*0.6f; continue; }
        int idx=fontLookup(unicode);
        if(idx<0){ cx+=sz+1; continue; }
        int col=idx%g_fontCols, row=idx/g_fontCols;
        float u0=(float)(col*g_fontCharSize)/(float)g_fontAtlasW;
        float v0=(float)(row*g_fontCharSize)/(float)g_fontAtlasH;
        float u1=u0+(float)g_fontCharSize/(float)g_fontAtlasW;
        float v1=v0+(float)g_fontCharSize/(float)g_fontAtlasH;
        float v[]={ cx,y,u0,v0,r,gg,b,a, cx+sz,y,u1,v0,r,gg,b,a,
                    cx,y+sz,u0,v1,r,gg,b,a, cx+sz,y+sz,u1,v1,r,gg,b,a };
        glBindTexture(GL_TEXTURE_2D,g->texFont);
        glVertexAttribPointer(g->aPos,2,GL_FLOAT,GL_FALSE,32,v);
        glVertexAttribPointer(g->aUV,2,GL_FLOAT,GL_FALSE,32,v+2);
        glVertexAttribPointer(g->aColor,4,GL_FLOAT,GL_FALSE,32,v+4);
        glDrawArrays(GL_TRIANGLE_STRIP,0,4);
        cx+=sz+1;
    }
}
static void num(float x,float y,int v,float sz,float r,float gg,float b,float a=1.f){
    char buf[16]; snprintf(buf,16,"%d",v); text(x,y,buf,sz,r,gg,b,a);
}
// 计算文本宽度
static float textW(const char* s,float sz){
    float w=0;
    for(const unsigned char* p=(const unsigned char*)s; *p; ){
        uint16_t unicode;
        if(*p<0x80){ unicode=*p; p++; }
        else if((*p&0xE0)==0xC0){ unicode=((*p&0x1F)<<6)|(*(p+1)&0x3F); p+=2; }
        else if((*p&0xF0)==0xE0){ unicode=((*p&0x0F)<<12)|((*(p+1)&0x3F)<<6)|(*(p+2)&0x3F); p+=3; }
        else { p++; continue; }
        if(unicode==' ') w+=sz*0.6f;
        else w+=sz+1;
    }
    return w;
}

// ==================== 世界渲染 ====================
static void renderWorld(){
    int sx=std::max(0,(int)(g->camX/TILE)-1);
    int sy=std::max(0,(int)(g->camY/TILE)-1);
    int ex=std::min(WORLD_W,(int)((g->camX+VW)/TILE)+1);
    int ey=std::min(WORLD_H,(int)((g->camY+VH)/TILE)+1);
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
            float depth=(y-SURFACE_Y)/(float)(WORLD_H-SURFACE_Y);
            rect(px,py,TILE,TILE, 0.12f-depth*0.08f, 0.06f-depth*0.04f, 0.03f-depth*0.02f, 1);
        }
        if(w==W_TREE){ sprite(g->texTree,px,py-TILE,TILE,TILE*2); }
        else if(w==W_BERRY){ sprite(g->texBerry,px,py,TILE,TILE); }
        else if(w==W_CLUE){
            float pulse=0.7f+0.3f*sinf(g->gameTime*3);
            drawQuad(g->texClue,px,py,TILE,TILE,false,pulse,pulse,pulse,pulse);
        }
    }
    if(g->mineTX>=0){
        float mx=g->mineTX*TILE-g->camX, my=g->mineTY*TILE-g->camY;
        float glow=0.3f+0.1f*sinf(g->gameTime*8);
        rect(mx,my,TILE,TILE,1,0.7f,0.2f,glow);
        int t=getTileAt(g->mineTX,g->mineTY);
        float pct=g->mineProg/mineTime(t);
        rect(mx+4,my+4,4,1,0,0,0,pct*0.7f);
        rect(mx+12,my+8,6,1,0,0,0,pct*0.7f);
        rect(mx+6,my+14,5,1,0,0,0,pct*0.7f);
        rect(mx+16,my+20,4,1,0,0,0,pct*0.7f);
    }
}

static void renderPlayer(){
    Player& p=g->p;
    int ti;
    if(!p.onGround) ti=5;
    else if(std::abs(p.vx)>20) ti=1+(p.animFrame%4);
    else ti=0;
    float px=p.x-16-g->camX, py=p.y-48-g->camY;
    bool flip=p.facing<0;
    if(p.invinceTimer>0 && ((int)(p.invinceTimer*10))%2==0) return;
    sprite(g->texPlayer[ti],px,py,32,48,flip);
    if(p.hasRadSuit) rect(px+4,py,24,48,1,0.85f,0.25f,0.25f);
}

static void renderParticles(){
    for(auto& pa:g->parts){
        float a=std::min(1.f,pa.life/0.4f);
        float sz=2.0f+pa.life;
        rect(pa.x-sz-g->camX,pa.y-sz-g->camY,sz*2,sz*2,pa.r/255.f,pa.g/255.f,pa.b/255.f,a);
    }
}

// ==================== UI ====================
static void drawBtn(const Btn& b,bool active){
    float x=b.x, y=b.y, w=b.w, h=b.h;
    rect(x,y,w,h,0,0,0,0.55f);
    float cr=active?0.35f:0.18f, cg=active?0.55f:0.18f, cb=active?0.75f:0.22f;
    rect(x+1,y+1,w-2,h-2,cr,cg,cb,0.85f);
    rect(x+1,y+1,w-2,2,cr+0.15f,cg+0.15f,cb+0.15f,0.3f);
    rect(x,y,w,1,0.4f,0.7f,0.95f,0.7f);
    rect(x,y+h-1,w,1,0.4f,0.7f,0.95f,0.7f);
    rect(x,y,1,h,0.4f,0.7f,0.95f,0.7f);
    rect(x+w-1,y,1,h,0.4f,0.7f,0.95f,0.7f);
    float ts=h*0.4f;
    float tw=textW(b.label,ts);
    text(x+(w-tw)/2, y+(h-ts)/2, b.label, ts, 1,1,1);
}

static void renderHUD(){
    float barW=90, barH=8, pad=4;
    auto bar=[&](const char* lbl,float v,float m,float r,float gg,float b,float y){
        rect(pad, y, barW, barH, 0,0,0,0.65f);
        float pct=v/m;
        if(pct>0) rect(pad+1,y+1,(barW-2)*pct,barH-2,r,gg,b,1);
        text(pad+2, y+barH+1, lbl, 6, 1,1,1);
        num(pad+barW-20, y+barH+1, (int)v, 6, 1,1,1);
    };
    bar("生命",g->p.hp,g->p.maxHp,0.95f,0.2f,0.2f,pad);
    bar("饥饿",g->p.hunger,g->p.maxHunger,1,0.6f,0,pad+20);
    bar("口渴",g->p.thirst,g->p.maxThirst,0.15f,0.65f,1,pad+40);
    bar("辐射",g->p.rad,g->p.maxRad,1,0.85f,0.1f,pad+60);

    if(g->p.inWater){
        float wx=VW/2-60;
        rect(wx-3,VH-55,126,16,0,0,0,0.7f);
        rect(wx,VH-53,120,12,0.9f,0.15f,0.05f,0.8f);
        text(wx+6,VH-51,"辐射水!",7,1,0.9f,0.7f);
    }
    // 线索计数
    float cx=VW-80;
    rect(cx,pad,76,12,0,0,0,0.65f);
    text(cx+2,pad+2,"线索:",6,1,0.85f,0.3f);
    num(cx+30,pad+2,g->foundClues,6,1,0.85f,0.3f);
    text(cx+42,pad+2,"/",6,0.7f,0.7f,0.7f);
    num(cx+48,pad+2,g->clueCount,6,0.7f,0.7f,0.7f);
}

static const char* ITEM_NAME(int id){
    switch(id){
        case I_DIRT:return"泥土";case I_STONE:return"石头";case I_WOOD:return"木头";
        case I_PLANK:return"木板";case I_STICK:return"木棍";case I_PICK:return"镐子";
        case I_TORCH:return"火把";case I_CAMP:return"篝火";case I_BANDAGE:return"绷带";
        case I_BERRY:return"浆果";case I_CBERRY:return"熟浆果";case I_FWATER:return"淡水";
        case I_PWATER:return"纯净水";case I_IRON:return"铁矿";case I_GOLD:return"金矿";
        case I_COAL:return"煤矿";case I_INGOT:return"铁锭";case I_IPICK:return"铁镐";
        case I_RAFT:return"木筏";case I_RSUIT:return"防辐射服";case I_FILTER:return"净水器";
    }
    return"?";
}

static void renderPanel(){
    float px=VW*0.06f, py=VH*0.12f, pw=VW*0.88f, ph=VH*0.76f;
    rect(px,py,pw,ph,0.02f,0.06f,0.14f,0.94f);
    rect(px,py,pw,2,0.25f,0.7f,0.95f,1);
    rect(px,py,2,ph,0.25f,0.7f,0.95f,1);
    float ry=VH*0.2f, lh=18;

    if(g->showSettings){
        text(VW*0.1f, VH*0.14f, "设置", 12, 0.25f,0.7f,0.95f);
        const char* labels[]={"音效","音乐","震动","操作模式","像素放大","按键位置"};
        for(int i=0;i<6;i++){
            float yy=ry+i*30;
            if(i%2) rect(VW*0.08f,yy,VW*0.84f,28,1,1,1,0.04f);
            text(VW*0.1f, yy+8, labels[i], 8, 0.8f,0.85f,1);
            const char* val="";
            switch(i){
                case 0: val=g->settings.soundOn?"开":"关"; break;
                case 1: val=g->settings.musicOn?"开":"关"; break;
                case 2: val=g->settings.vibrationOn?"开":"关"; break;
                case 3: val=g->settings.touchControl?"虚拟按键":"点击"; break;
                case 4: val=g->settings.pixelScale==1?"高":g->settings.pixelScale==2?"中":g->settings.pixelScale==3?"低":"超低"; break;
                case 5: val="自定义>"; break;
            }
            text(VW*0.6f, yy+8, val, 8, 0.3f,1,0.3f);
        }
        // 返回按钮
        rect(VW-80,VH-30,70,20,0.15f,0.5f,0.25f,0.85f);
        text(VW-65,VH-26,"返回",8,1,1,1);
        return;
    }

    if(g->showInv){
        text(VW*0.1f,VH*0.14f,"背包",12,0.25f,0.7f,0.95f);
        g->invDisplayCount=0; int row=0;
        for(int id=1;id<I_COUNT;id++){
            if(g->inv[id]>0){
                float yy=ry+row*lh;
                if(row%2) rect(VW*0.08f,yy,VW*0.84f,lh,1,1,1,0.04f);
                text(VW*0.1f,yy+4,ITEM_NAME(id),8,0.8f,0.85f,1);
                num(VW*0.5f,yy+4,g->inv[id],8,1,1,1);
                bool useable=(id==I_BERRY||id==I_CBERRY||id==I_FWATER||id==I_PWATER||id==I_BANDAGE);
                if(useable) text(VW*0.65f,yy+4,"点击使用",7,0.3f,1,0.3f);
                if(g->invDisplayCount<24) g->invDisplay[g->invDisplayCount++]=id;
                row++; if(row>10) break;
            }
        }
        if(row==0) text(VW*0.35f,VH*0.4f,"空",12,0.5f,0.5f,0.5f);
    } else if(g->showCraft){
        text(VW*0.1f,VH*0.14f,"制作",12,0.25f,0.7f,0.95f);
        for(int i=0;i<RECIPE_COUNT && i<11;i++){
            Recipe& r=RECIPES[i];
            float yy=ry+i*lh;
            bool can=true;
            for(int k=0;k<4;k++){ if(r.req[k][0]<0)break; if(g->inv[r.req[k][0]]<r.req[k][1]) can=false; }
            if(i%2) rect(VW*0.08f,yy,VW*0.84f,lh,1,1,1,0.04f);
            float col=can?1:0.4f;
            text(VW*0.1f,yy+4,r.name,8,col,col*0.85f,col*0.4f);
            char buf[64]; buf[0]=0;
            for(int k=0;k<4;k++){ if(r.req[k][0]<0)break;
                char b2[32]; snprintf(b2,32,"%s%d ",ITEM_NAME(r.req[k][0]),r.req[k][1]); strcat(buf,b2);
            }
            text(VW*0.35f,yy+4,buf,6,0.7f,0.7f,0.7f);
            if(can) text(VW*0.82f,yy+4,"制作",7,0.3f,1,0.3f);
        }
    } else if(g->showMenu){
        text(VW*0.35f,VH*0.22f,"暂停",16,0.25f,0.7f,0.95f);
        rect(VW*0.2f,VH*0.38f,VW*0.6f,VH*0.06f,0.15f,0.5f,0.25f,0.85f);
        text(VW*0.4f,VH*0.4f,"继续",10,1,1,1);
        rect(VW*0.2f,VH*0.48f,VW*0.6f,VH*0.06f,0.5f,0.35f,0.15f,0.85f);
        text(VW*0.38f,VH*0.5f,"重新开始",10,1,1,1);
        rect(VW*0.2f,VH*0.58f,VW*0.6f,VH*0.06f,0.15f,0.35f,0.5f,0.85f);
        text(VW*0.42f,VH*0.6f,"设置",10,1,1,1);
        rect(VW*0.2f,VH*0.68f,VW*0.6f,VH*0.06f,0.25f,0.25f,0.25f,0.85f);
        text(VW*0.42f,VH*0.7f,"退出",10,1,1,1);
    }
}

// ==================== 开屏动画 ====================
static void renderSplash(){
    float t=g->splashTimer;
    float alpha;
    if(t<0.5f) alpha=t/0.5f;           // 淡入
    else if(t<2.0f) alpha=1.0f;          // 停留
    else alpha=1.0f-(t-2.0f)/0.5f;       // 淡出
    if(alpha<0) alpha=0;
    // 背景
    rect(0,0,VW,VH,0.02f,0.04f,0.08f,1);
    // Logo闪烁效果
    float pulse=0.85f+0.15f*sinf(t*3);
    // 装饰线
    rect(VW/2-80,VH*0.35f,160,1,0.25f,0.7f,0.95f,alpha*0.6f);
    text(VW/2-textW("蓝色迷海",16)/2, VH*0.38f, "蓝色迷海", 16, 0.25f,0.7f,0.95f, alpha*pulse);
    rect(VW/2-80,VH*0.52f,160,1,0.25f,0.7f,0.95f,alpha*0.6f);
    text(VW/2-textW("Orgc Studio",8)/2, VH*0.56f, "Orgc Studio", 8, 0.4f,0.5f,0.55f, alpha);
    // 加载提示
    if(t>1.0f){
        float dotAlpha=0.3f+0.3f*sinf(t*4);
        text(VW/2-20, VH*0.75f, "加载中", 6, 0.5f,0.5f,0.5f, dotAlpha);
    }
}

// ==================== 主菜单 ====================
static void renderMain(){
    float dn=g->dayNight;
    // 渐变天空背景
    if(dn>0.5f){
        for(int y=0;y<VH;y+=4){
            float t=y/(float)VH;
            rect(0,y,VW,4, 0.28f+t*0.1f, 0.65f-t*0.1f, 0.92f-t*0.2f, 1);
        }
    } else {
        for(int y=0;y<VH;y+=4){
            float t=y/(float)VH;
            rect(0,y,VW,4, 0.03f+t*0.05f, 0.07f+t*0.1f, 0.14f+t*0.1f, 1);
        }
    }
    // 星星
    if(dn<0.5f){
        for(int i=0;i<30;i++){
            int sx=(i*7919)%VW, sy=(i*6271)%(VH/2);
            float tw=0.4f+0.6f*sinf(g->gameTime*1.5f+i*0.7f);
            rect(sx,sy,1,1,1,1,1,tw*(0.5f-dn));
        }
    }
    // 标题
    float cx=VW/2;
    rect(cx-90,VH*0.25f,180,1,0.25f,0.7f,0.95f,0.6f);
    text(cx-textW("蓝色迷海",20)/2, VH*0.28f, "蓝色迷海", 20, 0.25f,0.7f,0.95f);
    rect(cx-90,VH*0.42f,180,1,0.25f,0.7f,0.95f,0.6f);
    text(cx-textW("生存冒险",8)/2, VH*0.46f, "生存冒险", 8, 0.4f,0.5f,0.5f);
    // 开始按钮
    float bw=140, bh=30;
    float bx=cx-bw/2, by=VH*0.58f;
    float pulse=0.9f+0.1f*sinf(g->gameTime*3);
    rect(bx-1,by-1,bw+2,bh+2,0.25f,0.7f,0.95f,pulse*0.5f);
    rect(bx,by,bw,bh,0.15f,0.5f,0.78f,0.9f);
    rect(bx,by,bw,2,0.3f,0.75f,0.95f,0.4f);
    text(cx-textW("点击开始",10)/2, by+8, "点击开始", 10, 1,1,1);
    text(cx-textW("v9.0",6)/2, VH*0.9f, "v9.0", 6, 0.35f,0.35f,0.35f);
}

// ==================== 主渲染 ====================
static void renderGameContent(){
    glUniform1f(g->uW,(float)VW); glUniform1f(g->uH,(float)VH);

    // 天空渐变
    float dn=g->dayNight;
    if(dn>0.5f){
        for(int y=0;y<VH*0.7f;y+=4){
            float t=y/(float)(VH*0.7f);
            rect(0,y,VW,4, 0.28f+t*0.1f, 0.65f-t*0.1f, 0.92f-t*0.2f, 1);
        }
        // 云
        for(int i=0;i<4;i++){
            float cx=fmodf(VW*0.2f*i+g->gameTime*10, VW+80)-40;
            float cy=20+i*20;
            float ca=0.2f+0.1f*sinf(g->gameTime*0.5f+i);
            rect(cx-10,cy,30,4,1,1,1,ca);
            rect(cx-5,cy-3,20,3,1,1,1,ca*0.8f);
        }
    } else {
        for(int y=0;y<VH*0.7f;y+=4){
            float t=y/(float)(VH*0.7f);
            rect(0,y,VW,4, 0.03f+t*0.05f, 0.07f+t*0.1f, 0.14f+t*0.1f, 1);
        }
        for(int i=0;i<30;i++){
            int sx=(i*7919)%VW, sy=(i*6271)%(int)(VH*0.5f);
            float tw=0.4f+0.6f*sinf(g->gameTime*1.5f+i*0.7f);
            rect(sx,sy,1,1,1,1,1,tw*(0.5f-dn));
        }
    }
    // 太阳/月亮
    float celX=VW-50;
    if(dn>0.6f){
        rect(celX-8,20,16,16,1,0.85f,0.25f,0.5f);
        rect(celX-6,22,12,12,1,0.92f,0.4f,0.4f);
    } else if(dn<0.4f){
        rect(celX-6,22,12,12,0.85f,0.85f,0.9f,0.6f);
    }

    renderWorld();
    renderPlayer();
    renderParticles();
    if(dn<0.35f) rect(0,0,VW,VH,0,0,0.08f,0.4f-dn);

    renderHUD();
    // 按键自定义模式
    if(g->customizeControls){
        // 半透明遮罩
        rect(0,0,VW,VH,0,0,0,0.5f);
        text(VW/2-textW("拖拽按钮调整位置",10)/2, VH-20, "拖拽按钮调整位置", 10, 1,0.85f,0.3f);
        text(VW/2-textW("点击空白处完成",8)/2, VH-8, "点击空白处完成", 8, 0.7f,0.7f,0.7f);
        // 重置按钮
        rect(VW/2-30, VH-50, 60, 22, 0.8f,0.3f,0.3f,0.85f);
        text(VW/2-textW("重置默认",7)/2, VH-46, "重置默认", 7, 1,1,1);
        float resetBtnX=VW/2-30, resetBtnY=VH-50, resetBtnW=60, resetBtnH=22;
        // 高亮所有按钮
        for(int i=0;i<8;i++){
            Btn& b=BTNS[i];
            rect(b.x-2,b.y-2,b.w+4,b.h+4, 1,1,0,0.5f);
        }
        // 检查重置按钮点击 (在handleInput中无法直接检测, 这里把重置逻辑放在onInput)
        g->resetBtnX=resetBtnX; g->resetBtnY=resetBtnY;
        g->resetBtnW=resetBtnW; g->resetBtnH=resetBtnH;
    }
    drawBtn(BTNS[0],g->touchLeft);
    drawBtn(BTNS[1],g->touchRight);
    drawBtn(BTNS[2],g->touchJump);
    drawBtn(BTNS[3],g->touchMine);
    drawBtn(BTNS[4],false);
    drawBtn(BTNS[5],g->showInv);
    drawBtn(BTNS[6],g->showCraft);
    drawBtn(BTNS[7],g->showMenu);

    if(g->mineTX>=0){
        int t=getTileAt(g->mineTX,g->mineTY);
        float pct=std::min(1.f,g->mineProg/mineTime(t));
        rect(VW/2-40,VH-60,80,6,0,0,0,0.7f);
        rect(VW/2-39,VH-59,78*pct,4,1,0.55f,0,1);
        text(VW/2-textW("挖掘中",6)/2,VH-50,"挖掘中",6,1,0.7f,0.3f);
    }
    if(g->toast>0){
        float tw=textW(g->toastText,8);
        float ty=VH-30;
        rect(VW/2-tw/2-6,ty-2,tw+12,14,0,0,0,0.85f);
        text(VW/2-tw/2,ty,g->toastText,8,0.25f,0.7f,0.95f);
    }
    if(g->showInv||g->showCraft||g->showMenu||g->showSettings) renderPanel();
}

void render(){
    // 1. 渲染到FBO (480x270虚拟分辨率)
    glBindFramebuffer(GL_FRAMEBUFFER,g->fbo);
    glViewport(0,0,VW,VH);
    glClearColor(0,0,0,1);
    glClear(GL_COLOR_BUFFER_BIT);
    glUniform1f(g->uW,(float)VW); glUniform1f(g->uH,(float)VH);

    if(g->state==STATE_SPLASH){
        renderSplash();
    } else if(g->state==STATE_MAIN){
        renderMain();
    } else {
        renderGameContent();
    }

    // 2. 将FBO纹理绘制到屏幕（带letterbox黑边）
    glBindFramebuffer(GL_FRAMEBUFFER,0);
    glViewport(0,0,g->scrW,g->scrH);
    glClearColor(0,0,0,1);
    glClear(GL_COLOR_BUFFER_BIT);
    glUniform1f(g->uW,(float)g->scrW); glUniform1f(g->uH,(float)g->scrH);

    // 绘制FBO纹理到视口区域 (GL_NEAREST = 像素化)
    // 注意: OpenGL纹理V=0是底部, V=1是顶部; FBO渲染时游戏y=0在FBO顶部
    // 所以屏幕顶部对应V=1(纹理顶部), 屏幕底部对应V=0(纹理底部)
    float vx=(float)g->viewX, vy=(float)g->viewY, vw=(float)g->viewW, vh=(float)g->viewH;
    float v[]={ vx,vy,0,1,1,1,1,1, vx+vw,vy,1,1,1,1,1,1,
                vx,vy+vh,0,0,1,1,1,1, vx+vw,vy+vh,1,0,1,1,1,1 };
    glBindTexture(GL_TEXTURE_2D,g->fboTex);
    glVertexAttribPointer(g->aPos,2,GL_FLOAT,GL_FALSE,32,v);
    glVertexAttribPointer(g->aUV,2,GL_FLOAT,GL_FALSE,32,v+2);
    glVertexAttribPointer(g->aColor,4,GL_FLOAT,GL_FALSE,32,v+4);
    glDrawArrays(GL_TRIANGLE_STRIP,0,4);
}
