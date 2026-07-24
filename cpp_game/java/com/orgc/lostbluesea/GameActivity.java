package com.orgc.lostbluesea;

import android.app.Activity;
import android.os.Bundle;
import android.view.WindowManager;
import android.view.View;

public class GameActivity extends Activity {
    static { System.loadLibrary("game"); }
    native void nativeOnCreate(Activity act);
    native void nativeOnDestroy();
    native void nativeOnPause();
    native void nativeOnResume();

    @Override
    protected void onCreate(Bundle b) {
        super.onCreate(b);
        getWindow().addFlags(WindowManager.LayoutParams.FLAG_FULLSCREEN
            | WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON);
        getWindow().getDecorView().setSystemUiVisibility(
            View.SYSTEM_UI_FLAG_HIDE_NAVIGATION
            | View.SYSTEM_UI_FLAG_FULLSCREEN
            | View.SYSTEM_UI_FLAG_IMMERSIVE_STICKY);
        nativeOnCreate(this);
    }

    @Override protected void onDestroy() { nativeOnDestroy(); super.onDestroy(); }
    @Override protected void onPause() { nativeOnPause(); super.onPause(); }
    @Override protected void onResume() { nativeOnResume(); super.onResume(); }
}