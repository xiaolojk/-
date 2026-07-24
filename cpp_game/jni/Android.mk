LOCAL_PATH := $(call my-dir)

include $(CLEAR_VARS)
LOCAL_MODULE    := game
LOCAL_SRC_FILES := main.cpp render.cpp
LOCAL_LDLIBS    := -lEGL -lGLESv2 -llog -landroid
LOCAL_STATIC_LIBRARIES := android_native_app_glue
LOCAL_CFLAGS    := -DANDROID -O2 -Wall -Wno-c99-designator
include $(BUILD_SHARED_LIBRARY)

$(call import-module,android/native_app_glue)
