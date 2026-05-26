CREATE TABLE IF NOT EXISTS usuarios (
  id INT AUTO_INCREMENT PRIMARY KEY,
  username VARCHAR(100) NULL,
  password_hash VARCHAR(512) NULL,
  role VARCHAR(30) NULL,
  nombre VARCHAR(100) NULL,
  apellido VARCHAR(100) NULL,
  email VARCHAR(255) NOT NULL,
  fecha_registro DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  esta_activo BOOLEAN NOT NULL DEFAULT TRUE,
  UNIQUE KEY uq_usuarios_username (username),
  UNIQUE KEY uq_usuarios_email (email)
);

CREATE TABLE IF NOT EXISTS marcas (
  id INT AUTO_INCREMENT PRIMARY KEY,
  nombre VARCHAR(100) NOT NULL,
  UNIQUE KEY uq_marcas_nombre (nombre)
);

CREATE TABLE IF NOT EXISTS categorias (
  id INT AUTO_INCREMENT PRIMARY KEY,
  nombre VARCHAR(100) NOT NULL,
  UNIQUE KEY uq_categorias_nombre (nombre)
);

CREATE TABLE IF NOT EXISTS productos (
  id INT AUTO_INCREMENT PRIMARY KEY,
  nombre VARCHAR(150) NOT NULL,
  descripcion VARCHAR(500) NULL,
  precio DECIMAL(10,2) NOT NULL,
  stock INT NOT NULL DEFAULT 0,
  categoria_id INT NOT NULL,
  marca_id INT NOT NULL,
  FOREIGN KEY (categoria_id) REFERENCES categorias(id),
  FOREIGN KEY (marca_id) REFERENCES marcas(id)
);

INSERT INTO usuarios (username, password_hash, role, nombre, apellido, email, esta_activo)
SELECT
  'admin',
  'PBKDF2-SHA256$100000$ZfVIHyHIJ7Yx3T805k66Rw==$8bBBikXRbnSNk3ph8SypTC+iGdGwtGYiL9woNXKYzY0=',
  'SuperAdmin',
  'Administrador',
  'Sistema',
  'admin@sillas3cantos.local',
  TRUE
WHERE NOT EXISTS (
  SELECT 1
  FROM usuarios
  WHERE username = 'admin' OR email = 'admin@sillas3cantos.local'
);

UPDATE usuarios
SET role = 'SuperAdmin'
WHERE LOWER(username) = 'admin'
  AND role IS NOT NULL
  AND LOWER(role) = 'admin';

INSERT INTO usuarios (username, password_hash, role, nombre, apellido, email, esta_activo)
SELECT
  'user',
  'PBKDF2-SHA256$100000$17odRLWuHKkMzlVz8oSMZA==$r6m4fHtZMyZ4N/WPaAYyBys+e1Zo8OXJxEJbY2HBMB0=',
  'User',
  'Usuario',
  'Sistema',
  'user@sillas3cantos.local',
  TRUE
WHERE NOT EXISTS (
  SELECT 1
  FROM usuarios
  WHERE username = 'user' OR email = 'user@sillas3cantos.local'
);
