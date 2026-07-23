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

function generateToken(userId) {
  const data = userId + "|" + Date.now() + "|" + SECRET;
  return crypto.createHash("sha256").update(data).digest("hex");
}

module.exports = async (req, res) => {
  if (req.method !== "POST") {
    return res.status(405).json({ code: 405, success: false, message: "仅支持 POST 请求" });
  }

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

  return res.json({
    code: 200, success: true, message: "登录成功",
    userId: user.userId, username: user.username, token,
  });
};