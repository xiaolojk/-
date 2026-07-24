// sprites.h - 像素精灵生成（多帧动画 + 高质量方块）
#pragma once
#include <cstdint>
#include <cstring>
#include <cmath>
#include <cstdlib>

struct Canvas {
    int w, h;
    uint8_t* px;
    Canvas(int W, int H) : w(W), h(H) { px = new uint8_t[W*H*4](); }
    ~Canvas(){ delete[] px; }
    void set(int x,int y,uint8_t r,uint8_t g,uint8_t b,uint8_t a=255){
        if(x<0||y<0||x>=w||y>=h) return;
        uint8_t* p = px + (y*w+x)*4;
        // alpha blend
        if(a==255){ p[0]=r;p[1]=g;p[2]=b;p[3]=255; }
        else if(a>0){
            float af=a/255.0f, nf=1-af;
            p[0]=(uint8_t)(r*af+p[0]*nf);
            p[1]=(uint8_t)(g*af+p[1]*nf);
            p[2]=(uint8_t)(b*af+p[2]*nf);
            p[3]=(uint8_t)(255-(255-a)*(255-p[3])/255);
        }
    }
    void rect(int x,int y,int rw,int rh,uint8_t r,uint8_t g,uint8_t b,uint8_t a=255){
        for(int j=0;j<rh;j++) for(int i=0;i<rw;i++) set(x+i,y+j,r,g,b,a);
    }
    void circle(int cx,int cy,int rad,uint8_t r,uint8_t g,uint8_t b,uint8_t a=255){
        for(int y=-rad;y<=rad;y++) for(int x=-rad;x<=rad;x++)
            if(x*x+y*y<=rad*rad) set(cx+x,cy+y,r,g,b,a);
    }
    void clear(uint8_t r=0,uint8_t g=0,uint8_t b=0,uint8_t a=0){
        for(int i=0;i<w*h*4;i+=4){ px[i]=r;px[i+1]=g;px[i+2]=b;px[i+3]=a; }
    }
};

// ---- 玩家精灵 32x48, frame: 0=idle,1-4=walk,5=jump ----
// legSwing: 0=站立, 正值左腿前, 负值右腿前
inline Canvas* makePlayer(int frame){
    Canvas* c = new Canvas(32,48);
    int legA=0, legB=0, armA=0, armB=0; // 腿臂偏移
    if(frame>=1&&frame<=4){
        // 走路4帧: 腿臂摆动
        const int la[4]={2,0,-2,0}, lb[4]={-2,0,2,0};
        const int aa[4]={-2,0,2,0}, ab[4]={2,0,-2,0};
        legA=la[frame-1]; legB=lb[frame-1]; armA=aa[frame-1]; armB=ab[frame-1];
    } else if(frame==5){
        legA=-3; legB=3; armA=3; armB=-3; // 跳跃姿势
    }
    // 头发
    c->rect(10,2,12,5,45,24,16);
    c->rect(8,4,4,3,45,24,16); c->rect(20,4,4,3,45,24,16);
    c->rect(10,6,12,2,60,32,22);
    // 脸
    c->rect(10,7,12,10,244,194,162);
    // 头发侧边
    c->rect(9,7,2,6,45,24,16); c->rect(21,7,2,6,45,24,16);
    // 眼睛
    c->rect(13,11,2,2,26,26,46); c->rect(17,11,2,2,26,26,46);
    c->rect(14,11,1,1,80,80,120); c->rect(18,11,1,1,80,80,120);
    // 嘴
    c->rect(14,14,4,1,192,57,43);
    // 脖子
    c->rect(13,17,6,2,232,184,145);
    // 衬衫(蓝)
    c->rect(8,19,16,14,45,106,159);
    c->rect(8,19,16,2,58,122,184);   // 领口高光
    c->rect(8,31,16,2,26,74,127);    // 底部阴影
    c->rect(14,21,4,10,30,90,140);   // 中线
    // 手臂(随动画摆动)
    c->rect(6,19+armA,3,3,45,106,159);  // 左袖
    c->rect(6,22+armA,3,8,244,194,162); // 左手
    c->rect(23,19+armB,3,3,45,106,159); // 右袖
    c->rect(23,22+armB,3,8,244,194,162);// 右手
    // 腰带
    c->rect(8,33,16,2,90,58,26);
    c->rect(14,33,4,2,139,105,20);   // 扣子
    // 裤子(随动画摆腿)
    c->rect(9,35,6,7,58,58,74);
    c->rect(17,35,6,7,58,58,74);
    // 腿偏移
    if(legA>0) c->rect(9+legA,42,6,2,58,58,74);
    if(legB>0) c->rect(17+legB,42,6,2,58,58,74);
    // 靴子
    c->rect(8,44+ (legA>0?1:0),7,4,26,26,26);
    c->rect(17,44+ (legB>0?1:0),7,4,26,26,26);
    c->rect(8,44,7,1,60,60,60); // 靴子高光
    c->rect(17,44,7,1,60,60,60);
    return c;
}

inline Canvas* makeGrass(){
    Canvas* c=new Canvas(32,32);
    c->rect(0,0,32,32,90,138,58);
    c->rect(0,0,32,6,109,170,74);
    c->rect(0,0,32,2,140,200,102);
    // 草叶
    for(int i=0;i<10;i++) c->rect(i*3+1,5,1,2+(i%3),143,204,102);
    // 泥土
    c->rect(0,8,32,24,106,90,42);
    // 泥土纹理
    uint8_t spots[12][2]={{2,12},{8,10},{14,14},{20,11},{26,13},{5,18},{12,20},{18,18},{24,22},{3,26},{15,26},{27,28}};
    for(auto&s:spots) c->rect(s[0],s[1],2,2,90,74,30);
    for(int i=0;i<6;i++) c->rect(rand()%28+2,rand()%18+10,1,1,122,106,58);
    c->rect(0,28,32,4,0,0,0,50); // 底部阴影
    return c;
}

inline Canvas* makeDirt(){
    Canvas* c=new Canvas(32,32);
    c->rect(0,0,32,32,139,105,20);
    c->rect(2,2,28,28,155,121,36);
    for(int i=0;i<10;i++) c->rect(rand()%28+2,rand()%28+2,3,3,123,89,20);
    for(int i=0;i<8;i++) c->rect(rand()%28+2,rand()%28+2,2,2,171,137,52);
    c->rect(0,28,32,4,0,0,0,55);
    c->rect(28,0,4,32,0,0,0,40);
    return c;
}

inline Canvas* makeStone(){
    Canvas* c=new Canvas(32,32);
    c->rect(0,0,32,32,122,122,122);
    c->rect(2,2,28,28,138,138,138);
    c->rect(2,2,28,3,160,160,160);
    uint8_t sp[6][3][2]={{{6,4},{4,3}},{{18,8},{5,4}},{{10,16},{6,3}},{{22,20},{4,4}},{{3,22},{4,3}},{{14,24},{5,3}}};
    for(auto&s:sp){ c->rect(s[0][0],s[0][1],s[1][0],s[1][1],106,106,106); }
    c->rect(8,5,2,1,180,180,180); c->rect(20,9,3,1,180,180,180);
    c->rect(0,28,32,4,0,0,0,55);
    return c;
}

inline Canvas* makeOre(int type){ // 0=iron 1=coal 2=gold
    Canvas* c=new Canvas(32,32);
    c->rect(0,0,32,32,122,122,122);
    c->rect(2,2,28,28,138,138,138);
    uint8_t mr=0,mg=0,mb=0,hr=0,hg=0,hb=0,dr=0,dg=0,db=0;
    if(type==0){mr=200;mg=144;mb=100;hr=224;hg=168;hb=124;dr=184;dg=120;db=80;}
    if(type==1){mr=26;mg=26;mb=26;hr=42;hg=42;hb=42;dr=10;dg=10;db=10;}
    if(type==2){mr=255;mg=215;mb=0;hr=255;hg=236;hb=128;dr=230;dg=194;db=0;}
    uint8_t pos[3][2]={{5,6},{16,13},{7,21}};
    uint8_t sz[3][2]={{8,6},{10,8},{7,7}};
    for(int i=0;i<3;i++){ c->rect(pos[i][0],pos[i][1],sz[i][0],sz[i][1],mr,mg,mb); }
    for(int i=0;i<3;i++){ c->rect(pos[i][0]+1,pos[i][1]+1,sz[i][0]-3,sz[i][1]-2,hr,hg,hb); }
    for(int i=0;i<3;i++){ c->rect(pos[i][0]+sz[i][0]-2,pos[i][1]+sz[i][1]-2,2,2,dr,dg,db); }
    if(type==2){ c->set(8,8,255,255,255); c->set(20,16,255,255,255); c->set(10,24,255,255,255); }
    c->rect(0,28,32,4,0,0,0,55);
    return c;
}

inline Canvas* makeWater(){
    Canvas* c=new Canvas(32,32);
    c->rect(0,0,32,32,30,100,180);
    c->rect(0,0,32,16,42,127,212);
    // 波纹
    c->rect(2,4,8,1,255,255,255,80);
    c->rect(14,6,10,1,255,255,255,80);
    c->rect(4,10,6,1,255,255,255,80);
    c->rect(18,14,8,1,255,255,255,80);
    c->rect(6,20,7,1,255,255,255,60);
    c->rect(16,24,9,1,255,255,255,60);
    // 辐射绿调
    c->rect(0,0,32,32,100,255,50,35);
    return c;
}

inline Canvas* makeTree(){
    Canvas* c=new Canvas(32,64);
    // 树干
    c->rect(13,32,6,32,90,58,26);
    c->rect(13,32,2,32,106,74,42);
    c->rect(17,32,2,32,58,38,16);
    // 树冠层
    c->circle(16,24,14,45,106,30);
    c->circle(16,20,12,58,138,46);
    c->circle(16,16,9,74,160,62);
    c->circle(14,12,5,90,176,78);
    // 暗部
    c->rect(8,26,3,2,29,74,14);
    c->rect(22,24,3,2,29,74,14);
    c->rect(14,30,4,2,29,74,14);
    return c;
}

inline Canvas* makeBerry(){
    Canvas* c=new Canvas(32,32);
    c->circle(16,18,12,46,125,50);
    c->circle(16,16,10,58,154,62);
    c->circle(14,14,6,74,171,78);
    // 浆果
    c->rect(10,14,3,3,194,24,91);
    c->rect(20,16,3,3,194,24,91);
    c->rect(14,22,3,3,194,24,91);
    c->set(11,15,233,30,99); c->set(21,17,233,30,99); c->set(15,23,233,30,99);
    return c;
}

inline Canvas* makeClue(){
    Canvas* c=new Canvas(32,32);
    c->clear(0,0,0,0);
    c->circle(16,16,8,255,215,0,180);
    c->circle(16,16,5,255,235,100,200);
    return c;
}
