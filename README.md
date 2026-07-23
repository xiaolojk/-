# 像素生存游戏

一个像素风格的荒野求生游戏。

## 项目结构

```
/workspace/
├── config.iyu          # 全局配置（API地址、颜色主题）
├── api.myu             # 网络请求模块（注册、登录）
├── login.iyu           # 登录界面逻辑
├── login.xml           # 登录界面布局
├── register.iyu        # 注册界面逻辑
├── register.xml        # 注册界面布局
├── main.iyu            # 主界面逻辑
├── main.xml            # 主界面布局
├── game.iyu            # 游戏界面逻辑
├── game.xml            # 游戏界面布局
├── server/             # 后端服务器
│   ├── package.json
│   └── server.js
└── README.md
```

## 技术栈

- **开发语言**: 裕语言 (iyu)
- **运行平台**: Android 8.0+
- **后端**: Node.js + Express
- **UI设计**: 像素荒野风格，深绿主题

## 界面

### 登录界面
- 用户名/密码输入
- 进入游戏按钮
- 跳转注册

### 注册界面
- 角色名/邮箱/密码/确认密码
- 创建角色按钮

### 主界面
- 幸存者欢迎语
- 开始探索按钮
- 背包/制造入口

### 游戏界面
- 状态栏（HP、饥饿、口渴）
- 资源栏（木材、石料、食物）
- 像素世界地图
- 方向键移动
- 采集/吃/喝按钮
- 生存倒计时（饥饿口渴自动减少）

## 部署

### 后端部署

```bash
cd server
npm install
node server.js
# 运行在 http://localhost:3000
```

### 修改连接地址

编辑 `config.iyu` 第一行：
```iyu
qj s BASE_URL = "http://你的服务器IP:3000"
```

### 导入 iApp

将所有 .iyu .myu .xml 文件导入 iApp，构建 APK 即可。