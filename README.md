# 像素生存游戏

一个像素风格的荒野求生游戏，基于裕语言(iyu)开发，运行于 Android 8.0+。

## 项目结构

```
├── config.iyu          # 全局配置（API地址、颜色主题）
├── api.myu             # 网络请求模块（注册、登录）
├── login.iyu           # 登录界面逻辑
├── login.xml           # 登录界面布局
├── register.iyu        # 注册界面逻辑
├── register.xml        # 注册界面布局
├── main.iyu            # 主界面逻辑
├── main.xml            # 主界面布局
├── game.iyu            # 游戏画面逻辑
├── game.xml            # 游戏画面布局
├── server/             # 后端服务器
│   ├── package.json
│   └── server.js
└── README.md
```

## 导入到 iApp

- 6个代码文件(.iyu/.myu) → 放项目根目录
- 4个布局文件(.xml) → 放 layout/ 文件夹

## 部署后端

```bash
cd server
npm install
node server.js
# 运行在 http://localhost:3000
```

修改 config.iyu 第一行指向你的服务器地址。

## 接口

| 接口 | 方法 | 说明 |
|------|------|------|
| /user/register | POST | 注册 |
| /user/login | POST | 登录 |
| /user/check-token | POST | 验证Token |
