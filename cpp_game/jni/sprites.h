// sprites.h - 精美像素精灵（基于预生成嵌入数据）
#pragma once
#include <cstdint>
#include <cstring>
#include <cmath>
#include <cstdlib>
#include "sprites_data.h"

struct Canvas {
    int w, h;
    uint8_t* px;
    Canvas(int W, int H) : w(W), h(H) { px = new uint8_t[W*H*4](); }
    ~Canvas(){ delete[] px; }
    void set(int x,int y,uint8_t r,uint8_t g,uint8_t b,uint8_t a=255){
        if(x<0||y<0||x>=w||y>=h) return;
        uint8_t* p = px + (y*w+x)*4;
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
    void clear(uint8_t r=0,uint8_t g=0,uint8_t b=0,uint8_t a=0){
        for(int i=0;i<w*h*4;i+=4){ px[i]=r;px[i+1]=g;px[i+2]=b;px[i+3]=a; }
    }
};

// Create a Canvas from embedded sprite data
inline Canvas* canvasFromData(const SpriteData* sd){
    if(!sd || !sd->data) return nullptr;
    Canvas* c = new Canvas(sd->w, sd->h);
    memcpy(c->px, sd->data, sd->w * sd->h * 4);
    return c;
}

// 玩家精灵 32x48 (6帧: 0站立/1-4走路/5跳跃)
inline Canvas* makePlayer(int frame){
    char name[16];
    snprintf(name, 16, "player_%d", frame);
    return canvasFromData(getSprite(name));
}

// 草地 32x32
inline Canvas* makeGrass(){ return canvasFromData(getSprite("grass")); }

// 泥土 32x32
inline Canvas* makeDirt(){ return canvasFromData(getSprite("dirt")); }

// 石头 32x32
inline Canvas* makeStone(){ return canvasFromData(getSprite("stone")); }

// 矿石 (0=铁 1=煤 2=金) 32x32
inline Canvas* makeOre(int type){
    const char* names[] = {"iron", "coal", "gold"};
    return canvasFromData(getSprite(names[type]));
}

// 水 32x32
inline Canvas* makeWater(){ return canvasFromData(getSprite("water")); }

// 树 32x64
inline Canvas* makeTree(){ return canvasFromData(getSprite("tree")); }

// 浆果丛 32x32
inline Canvas* makeBerry(){ return canvasFromData(getSprite("berry")); }

// 线索标记 32x32
inline Canvas* makeClue(){ return canvasFromData(getSprite("clue")); }