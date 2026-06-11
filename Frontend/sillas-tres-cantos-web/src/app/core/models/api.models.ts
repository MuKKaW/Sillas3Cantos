export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  accessToken: string;
  expiresAtUtc: string;
  tokenType: string;
  role: string;
}

export interface Usuario {
  id: number;
  username?: string | null;
  role?: string | null;
  nombre?: string | null;
  apellido?: string | null;
  email: string;
  fechaRegistro?: string | null;
  estaActivo?: boolean | null;
}

export interface PostUsuario {
  username: string;
  password: string;
  role?: string | null;
  nombre?: string | null;
  apellido?: string | null;
  email: string;
}

export interface PutUsuario {
  id?: number;
  username?: string | null;
  password?: string | null;
  role?: string | null;
  nombre?: string | null;
  apellido?: string | null;
  email?: string | null;
  estaActivo?: boolean | null;
}

export interface Categoria {
  id: number;
  nombre: string;
  descripcion?: string | null;
  ordenVisual: number;
  esVisible: boolean;
  fechaCreacion: string;
  fechaActualizacion?: string | null;
}

export interface PostCategoria {
  nombre: string;
  descripcion?: string | null;
  ordenVisual?: number | null;
  esVisible?: boolean | null;
}

export interface PutCategoria {
  id?: number;
  nombre?: string | null;
  descripcion?: string | null;
  ordenVisual?: number | null;
  esVisible?: boolean | null;
}

export interface Marca {
  id: number;
  nombre: string;
  descripcion?: string | null;
  paisOrigen?: string | null;
  anioFundacion?: number | null;
  esVisible: boolean;
  fechaCreacion: string;
  fechaActualizacion?: string | null;
}

export interface PostMarca {
  nombre: string;
  descripcion?: string | null;
  paisOrigen?: string | null;
  anioFundacion?: number | null;
  esVisible?: boolean | null;
}

export interface PutMarca {
  id?: number;
  nombre?: string | null;
  descripcion?: string | null;
  paisOrigen?: string | null;
  anioFundacion?: number | null;
  esVisible?: boolean | null;
}

export interface Producto {
  id: number;
  nombre: string;
  descripcion?: string | null;
  precio: number;
  stock: number;
  categoriaId: number;
  marcaId: number;
  creadoPorUsuarioId?: number | null;
  esVisible: boolean;
  imagenUrl?: string | null;
  fechaCreacion: string;
  fechaActualizacion?: string | null;
}

export interface PostProducto {
  nombre: string;
  descripcion?: string | null;
  precio: number;
  stock: number;
  categoriaId: number;
  marcaId: number;
  esVisible?: boolean | null;
}

export interface PutProducto {
  id?: number;
  nombre?: string | null;
  descripcion?: string | null;
  precio?: number | null;
  stock?: number | null;
  categoriaId?: number | null;
  marcaId?: number | null;
  esVisible?: boolean | null;
}

export interface ProductoArchivo {
  id: number;
  productoId: number;
  subidoPorUsuarioId?: number | null;
  nombreOriginal: string;
  contentType: string;
  tamanoBytes: number;
  fechaSubida: string;
  urlDescarga: string;
}

export interface ProductoExterno {
  idExterno: number;
  nombre: string;
  descripcion?: string | null;
  categoria: string;
  marca?: string | null;
  precio: number;
  stock: number;
  rating: number;
  thumbnail?: string | null;
}

export interface BusquedaProductosExternos {
  fuente: string;
  query: string;
  total: number;
  skip: number;
  limit: number;
  productos: ProductoExterno[];
}
