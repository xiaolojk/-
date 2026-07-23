const express = require("express");
const fs = require("fs");
const path = require("path");
const crypto = require("crypto");

const app = express();
app.use(express.json());

const DATA_FILE = path.join(__dirname, "users.json");
const SECRET = "card-game-secret-key-2024";

// 读取用户数据
function loadUsers() {
  try {
    if (fs.existsSync(DATA_FILE)) {
      return JSON.parse(fs.readFileSync(DATA_FILE, "utf-8"));
    }
  } catch (e) {}
  return [];
}

// 保存用户数据
function saveUsers(users) {
  fs.writeFileSync(DATA_FILE, JSON.stringify(users, null, 2), "utf-8");
}

// 生成简单 token
function generateToken(userId) {
  const data = userId + "|" + Date.now() + "|" + SECRET;
  return crypto.createHash("sha256").update(data).digest("hex");
}

// ========== 注册接口 ==========
app.post("/user/register", (req, res) => {
  const { username, password, email } = req.body;

  // 参数校验
  if (!username || username.length < 3) {
    return res.json({ code: 400, success: false, message: "用户名至少需要3个字符" });
  }
  if (!password || password.length < 6) {
    return res.json({ code: 400, success: false, message: "密码至少需要6个字符" });
  }
  if (!email || !email.includes("@")) {
    return res.json({ code: 400, success: false, message: "请输入有效的邮箱地址" });
  }

  const users = loadUsers();

  // 检查用户名是否已存在
  if (users.find((u) => u.username === username)) {
    return res.json({ code: 400, success: false, message: "用户名已存在" });
  }

  // 检查邮箱是否已注册
  if (users.find((u) => u.email === email)) {
    return res.json({ code: 400, success: false, message: "该邮箱已被注册" });
  }

  // 创建新用户
  const userId = "u" + Date.now() + "_" + Math.random().toString(36).slice(2, 8);
  const newUser = {
    userId,
    username,
    password, // 实际项目中应该加密存储
    email,
    createdAt: new Date().toISOString(),
  };

  users.push(newUser);
  saveUsers(users);

  console.log(`[注册成功] 用户: ${username}, ID: ${userId}`);

  return res.json({
    code: 200,
    success: true,
    message: "注册成功",
    userId,
    username,
  });
});

// ========== 登录接口 ==========
app.post("/user/login", (req, res) => {
  const { username, password } = req.body;

  if (!username || !password) {
    return res.json({ code: 400, success: false, message: "请输入用户名和密码" });
  }

  const users = loadUsers();
  const user = users.find((u) => u.username === username && u.password === password);

  if (!user) {
    return res.json({ code: 401, success: false, message: "用户名或密码错误" });
  }

  const token = generateToken(user.userId);

  console.log(`[登录成功] 用户: ${username}, ID: ${user.userId}`);

  return res.json({
    code: 200,
    success: true,
    message: "登录成功",
    userId: user.userId,
    username: user.username,
    token,
  });
});

// ========== 验证 Token 接口 ==========
app.post("/user/check-token", (req, res) => {
  const { token, userId } = req.body;

  if (!token || !userId) {
    return res.json({ code: 400, success: false, message: "参数不完整" });
  }

  // 简单验证：重新生成 token 对比
  const expectedToken = generateToken(userId);
  if (token === expectedToken) {
    return res.json({ code: 200, success: true, message: "Token 有效" });
  }

  return res.json({ code: 401, success: false, message: "Token 无效" });
});

// ========== 健康检查 ==========
app.get("/", (req, res) => {
  res.json({ status: "running", name: "卡牌游戏服务器", version: "1.0.0" });
});

// 启动服务器
const PORT = process.env.PORT || 3000;
app.listen(PORT, "0.0.0.0", () => {
  console.log("========================================");
  console.log("  卡牌游戏服务器已启动");
  console.log("  地址: http://0.0.0.0:" + PORT);
  console.log("  注册: POST /user/register");
  console.log("  登录: POST /user/login");
  console.log("========================================");
});