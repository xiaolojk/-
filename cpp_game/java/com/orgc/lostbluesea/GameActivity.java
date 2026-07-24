package com.orgc.lostbluesea;

import android.app.NativeActivity;
import android.graphics.Bitmap;
import android.graphics.Canvas;
import android.graphics.Paint;
import android.graphics.Typeface;
import android.os.Bundle;
import android.view.WindowManager;
import android.view.View;

public class GameActivity extends NativeActivity {

    private static final int CHAR_SIZE = 32;
    private static final int ATLAS_COLS = 16;

    private static final String CHARS =
        " ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789" +
        ":%/!.,+-?()[]" +
        "蓝色迷海点击开始生存冒险精美版" +
        "暂停继续重新开始退出设置按键位置" +
        "生命饥饿口渴辐射线索挖掘中" +
        "水跳挖用背包制作菜单" +
        "泥土石头木头木板木棍镐子火把篝火" +
        "绷带浆果熟淡水纯铁金煤锭筏防服净器" +
        "材料不足海有太远了面前没东西" +
        "查看线索按住采集你死不能直接使用空" +
        "纸条核电站泄漏收音机最后的广播金属牌冰川监测站" +
        "日记平面上升米涂鸦寻找方舟计划字不要喝" +
        "地图碎片地下避难报告变异样本失控信亲爱的" +
        "最近陆地笔记反向渗透净化船日志第天蓝图矿井设计图" +
        "军方文件操作镜子记住你是谁" +
        "音效音乐画质震动高低开关" +
        "虚拟按键点击返回量亮度" +
        "出品已收集共个夜获得消耗耐久距离" +
        "拖拽按钮到想要的位置重置默认";

    @Override
    protected void onCreate(Bundle b) {
        super.onCreate(b);
        getWindow().addFlags(WindowManager.LayoutParams.FLAG_FULLSCREEN
            | WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON);
        getWindow().getDecorView().setSystemUiVisibility(
            View.SYSTEM_UI_FLAG_HIDE_NAVIGATION
            | View.SYSTEM_UI_FLAG_FULLSCREEN
            | View.SYSTEM_UI_FLAG_IMMERSIVE_STICKY);

        renderFontAtlas();
    }

    private void renderFontAtlas() {
        try {
            int count = CHARS.length();
            int rows = (count + ATLAS_COLS - 1) / ATLAS_COLS;
            int atlasW = ATLAS_COLS * CHAR_SIZE;
            int potH = 1;
            while (potH < rows * CHAR_SIZE) potH *= 2;
            int atlasH = potH;

            Bitmap bmp = Bitmap.createBitmap(atlasW, atlasH, Bitmap.Config.ARGB_8888);
            Canvas canvas = new Canvas(bmp);
            canvas.drawColor(0x00000000);

            Paint paint = new Paint();
            paint.setAntiAlias(true);
            paint.setTextAlign(Paint.Align.CENTER);
            paint.setTypeface(Typeface.DEFAULT);
            paint.setTextSize(CHAR_SIZE - 4);
            paint.setColor(0xFFFFFFFF);
            paint.setSubpixelText(true);

            float cx = CHAR_SIZE / 2f;
            float cy = CHAR_SIZE - 6f;

            for (int i = 0; i < count; i++) {
                int col = i % ATLAS_COLS;
                int row = i / ATLAS_COLS;
                float x = col * CHAR_SIZE + cx;
                float y = row * CHAR_SIZE + cy;
                canvas.drawText(CHARS, i, i + 1, x, y, paint);
            }

            int[] pixels = new int[atlasW * atlasH];
            bmp.getPixels(pixels, 0, atlasW, 0, 0, atlasW, atlasH);
            bmp.recycle();

            byte[] rgba = new byte[atlasW * atlasH * 4];
            for (int i = 0; i < pixels.length; i++) {
                int p = pixels[i];
                rgba[i * 4]     = (byte) ((p >> 16) & 0xFF);
                rgba[i * 4 + 1] = (byte) ((p >> 8) & 0xFF);
                rgba[i * 4 + 2] = (byte) (p & 0xFF);
                rgba[i * 4 + 3] = (byte) ((p >> 24) & 0xFF);
            }

            char[] unicodes = new char[count];
            for (int i = 0; i < count; i++) {
                unicodes[i] = CHARS.charAt(i);
            }

            // 调用native方法传入字体数据（不做GL操作）
            nativeSetFontData(unicodes, rgba, atlasW, atlasH, CHAR_SIZE, ATLAS_COLS);
        } catch (Exception e) {
            android.util.Log.e("Game", "Font rendering failed", e);
        }
    }

    private native void nativeSetFontData(char[] unicodes, byte[] rgba,
                                          int atlasW, int atlasH,
                                          int charSize, int cols);
}
