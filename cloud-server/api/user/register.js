const crypto = require("crypto");
const fs = require("fs");
const path = require("path");

const DATA_FILE = path.join(__dirname, "..", "..", "users.json");
const SECRET = "card-game-secret-key-2024";

function loadUsers() {
  try {
    if (fs.existsSync(DATA_FILE)) {
      return JSON.parse(fs.readFileSync(DATA_FILE, "utf-8"));
    }
  } catch (e) {}
  return [];
}

function saveUsers(users) {
  fs.writeFileSync(DATA_FILE, JSON.stringify(users, null, 2), "utf-8");
}

function generateToken(userId) {
  const data = userId + "|" + Date.now() + "|" + SECRET;
  return crypto.createHash("sha256").update(data).digest("hex");
}

module.exports = async (req, res) => {
  if (req.method !== "POST") {
    return res.status(405).json({ code: 405, success: false, message: "仅支持 POST 请求" });
  }

  const { username, password, email } = req.body;

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
  if (users.find((u) => u.username === username)) {
    return res.json({ code: 400, success: false, message: "用户名已存在" });
  }
  if (users.find((u) => u.email === email)) {
    return res.json({ code: 400, success: false, message: "该邮箱已被注册" });
  }

  const userId = "u" + Date.now() + "_" + Math.random().toString(36).slice(2, 8);
  users.push({
    userId, username, password, email,
    createdAt: new Date().toISOString(),
  });
  saveUsers(users);

  return res.json({ code: 200, success: true, message: "注册成功", userId, username });
};