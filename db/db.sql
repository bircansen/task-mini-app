-- DATABASE
CREATE DATABASE IF NOT EXISTS task_mini_app;
USE task_mini_app;

-- USERS TABLE
CREATE TABLE users (
  id INT AUTO_INCREMENT PRIMARY KEY,
  full_name VARCHAR(100) NOT NULL,
  email VARCHAR(100) NOT NULL UNIQUE,
  created_at DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- TASKS TABLE
CREATE TABLE tasks (
  id INT AUTO_INCREMENT PRIMARY KEY,
  title VARCHAR(255) NOT NULL,
  description TEXT,
  status ENUM('todo', 'doing', 'done') NOT NULL DEFAULT 'todo',
  assignee_user_id INT NULL,
  due_date DATE NULL,
  created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
  FOREIGN KEY (assignee_user_id) REFERENCES users(id)
);

-- TASK LOGS TABLE
CREATE TABLE task_logs (
  id INT AUTO_INCREMENT PRIMARY KEY,
  task_id INT NOT NULL,
  action ENUM('created', 'updated', 'status_changed') NOT NULL,
  old_value TEXT NULL,
  new_value TEXT NULL,
  created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
  FOREIGN KEY (task_id) REFERENCES tasks(id)
);

-- SEED USERS (>= 3)
INSERT INTO users (full_name, email) VALUES
('Ali Yılmaz', 'ali@example.com'),
('Ayşe Demir', 'ayse@example.com'),
('Mehmet Kaya', 'mehmet@example.com');

-- SEED TASKS (>= 10, farklı status dağılımı)
INSERT INTO tasks (title, description, status, assignee_user_id, due_date) VALUES
('Login ekranı', 'Kullanıcı giriş ekranı', 'todo', 1, '2025-03-10'),
('Register ekranı', 'Kayıt olma ekranı', 'doing', 2, '2025-03-11'),
('Task API', 'Görev API yazılacak', 'done', 1, '2025-03-05'),
('UI tasarım', 'Basit UI', 'todo', 3, NULL),
('JWT Auth', 'Token sistemi', 'doing', 1, NULL),
('Filtreleme', 'Task filtreleme', 'todo', 2, NULL),
('Pagination', 'Sayfalama', 'todo', NULL, NULL),
('Unit Test', 'Test yazımı', 'done', 3, NULL),
('Deploy', 'Sunucuya atma', 'todo', NULL, NULL),
('Log sistemi', 'Task loglama', 'doing', 2, NULL);

-- SORGU 1: Kullanıcı bazlı açık görev sayısı (todo + doing)
SELECT 
  u.full_name,
  COUNT(t.id) AS open_task_count
FROM users u
LEFT JOIN tasks t 
  ON u.id = t.assignee_user_id
  AND t.status IN ('todo', 'doing')
GROUP BY u.id;

-- SORGU 2: Son 7 günde değişen görevler
SELECT 
  t.title,
  tl.action,
  tl.created_at
FROM task_logs tl
JOIN tasks t ON t.id = tl.task_id
WHERE tl.created_at >= NOW() - INTERVAL 7 DAY;