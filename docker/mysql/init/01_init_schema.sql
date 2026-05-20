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
