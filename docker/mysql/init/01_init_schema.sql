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
  descripcion VARCHAR(255) NULL,
  pais_origen VARCHAR(100) NULL,
  anio_fundacion INT NULL,
  es_visible BOOLEAN NOT NULL DEFAULT TRUE,
  fecha_creacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  fecha_actualizacion DATETIME NULL DEFAULT NULL,
  UNIQUE KEY uq_marcas_nombre (nombre)
);

CREATE TABLE IF NOT EXISTS categorias (
  id INT AUTO_INCREMENT PRIMARY KEY,
  nombre VARCHAR(100) NOT NULL,
  descripcion VARCHAR(255) NULL,
  orden_visual INT NOT NULL DEFAULT 0,
  es_visible BOOLEAN NOT NULL DEFAULT TRUE,
  fecha_creacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  fecha_actualizacion DATETIME NULL DEFAULT NULL,
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
  es_visible BOOLEAN NOT NULL DEFAULT TRUE,
  fecha_creacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  fecha_actualizacion DATETIME NULL DEFAULT NULL,
  FOREIGN KEY (categoria_id) REFERENCES categorias(id),
  FOREIGN KEY (marca_id) REFERENCES marcas(id)
);

SET @schema_name = DATABASE();

SET @sql = IF (
  EXISTS (
    SELECT 1
    FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = @schema_name
      AND TABLE_NAME = 'marcas'
      AND COLUMN_NAME = 'descripcion'
  ),
  'SELECT 1',
  'ALTER TABLE marcas ADD COLUMN descripcion VARCHAR(255) NULL'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @sql = IF (
  EXISTS (
    SELECT 1
    FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = @schema_name
      AND TABLE_NAME = 'marcas'
      AND COLUMN_NAME = 'pais_origen'
  ),
  'SELECT 1',
  'ALTER TABLE marcas ADD COLUMN pais_origen VARCHAR(100) NULL'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @sql = IF (
  EXISTS (
    SELECT 1
    FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = @schema_name
      AND TABLE_NAME = 'marcas'
      AND COLUMN_NAME = 'anio_fundacion'
  ),
  'SELECT 1',
  'ALTER TABLE marcas ADD COLUMN anio_fundacion INT NULL'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @sql = IF (
  EXISTS (
    SELECT 1
    FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = @schema_name
      AND TABLE_NAME = 'marcas'
      AND COLUMN_NAME = 'es_visible'
  ),
  'SELECT 1',
  'ALTER TABLE marcas ADD COLUMN es_visible BOOLEAN NOT NULL DEFAULT TRUE'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @sql = IF (
  EXISTS (
    SELECT 1
    FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = @schema_name
      AND TABLE_NAME = 'marcas'
      AND COLUMN_NAME = 'fecha_creacion'
  ),
  'SELECT 1',
  'ALTER TABLE marcas ADD COLUMN fecha_creacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @sql = IF (
  EXISTS (
    SELECT 1
    FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = @schema_name
      AND TABLE_NAME = 'marcas'
      AND COLUMN_NAME = 'fecha_actualizacion'
  ),
  'SELECT 1',
  'ALTER TABLE marcas ADD COLUMN fecha_actualizacion DATETIME NULL DEFAULT NULL'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @sql = IF (
  EXISTS (
    SELECT 1
    FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = @schema_name
      AND TABLE_NAME = 'categorias'
      AND COLUMN_NAME = 'descripcion'
  ),
  'SELECT 1',
  'ALTER TABLE categorias ADD COLUMN descripcion VARCHAR(255) NULL'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @sql = IF (
  EXISTS (
    SELECT 1
    FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = @schema_name
      AND TABLE_NAME = 'categorias'
      AND COLUMN_NAME = 'orden_visual'
  ),
  'SELECT 1',
  'ALTER TABLE categorias ADD COLUMN orden_visual INT NOT NULL DEFAULT 0'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @sql = IF (
  EXISTS (
    SELECT 1
    FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = @schema_name
      AND TABLE_NAME = 'categorias'
      AND COLUMN_NAME = 'es_visible'
  ),
  'SELECT 1',
  'ALTER TABLE categorias ADD COLUMN es_visible BOOLEAN NOT NULL DEFAULT TRUE'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @sql = IF (
  EXISTS (
    SELECT 1
    FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = @schema_name
      AND TABLE_NAME = 'categorias'
      AND COLUMN_NAME = 'fecha_creacion'
  ),
  'SELECT 1',
  'ALTER TABLE categorias ADD COLUMN fecha_creacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @sql = IF (
  EXISTS (
    SELECT 1
    FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = @schema_name
      AND TABLE_NAME = 'categorias'
      AND COLUMN_NAME = 'fecha_actualizacion'
  ),
  'SELECT 1',
  'ALTER TABLE categorias ADD COLUMN fecha_actualizacion DATETIME NULL DEFAULT NULL'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @sql = IF (
  EXISTS (
    SELECT 1
    FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = @schema_name
      AND TABLE_NAME = 'productos'
      AND COLUMN_NAME = 'es_visible'
  ),
  'SELECT 1',
  'ALTER TABLE productos ADD COLUMN es_visible BOOLEAN NOT NULL DEFAULT TRUE'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @sql = IF (
  EXISTS (
    SELECT 1
    FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = @schema_name
      AND TABLE_NAME = 'productos'
      AND COLUMN_NAME = 'fecha_creacion'
  ),
  'SELECT 1',
  'ALTER TABLE productos ADD COLUMN fecha_creacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @sql = IF (
  EXISTS (
    SELECT 1
    FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = @schema_name
      AND TABLE_NAME = 'productos'
      AND COLUMN_NAME = 'fecha_actualizacion'
  ),
  'SELECT 1',
  'ALTER TABLE productos ADD COLUMN fecha_actualizacion DATETIME NULL DEFAULT NULL'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

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
