import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../config/api.config';
import {
  Categoria,
  ConfiguracionCatalogo,
  Marca,
  PermisosCatalogo,
  PostCategoria,
  PostMarca,
  PostProducto,
  PostSolucion,
  PostUsuario,
  Producto,
  ProductoArchivo,
  PutConfiguracionCatalogo,
  PutCategoria,
  PutMarca,
  PutProducto,
  PutSolucion,
  PutUsuario,
  Solucion,
  Usuario
} from '../models/api.models';

export interface ProductosQuery {
  idProducto?: number;
  nombre?: string;
  categoriaId?: number;
  marcaId?: number;
  orderAsc?: boolean;
  includeHidden?: boolean;
}

export interface CatalogoQuery {
  id?: number;
  nombre?: string;
  orderAsc?: boolean;
  includeHidden?: boolean;
}

export interface SolucionesQuery {
  id?: number;
  titulo?: string;
  orderAsc?: boolean;
}

@Injectable({ providedIn: 'root' })
export class BackendApiService {
  private readonly http = inject(HttpClient);

  getCategorias(query: CatalogoQuery = {}): Observable<Categoria[]> {
    let params = new HttpParams();

    if (query.id && query.id > 0) {
      params = params.set('idCategoria', String(query.id));
    }
    if (query.nombre) {
      params = params.set('nombre', query.nombre);
    }
    params = params.set('orderAsc', String(query.orderAsc ?? true));
    if (query.includeHidden !== undefined) {
      params = params.set('includeHidden', String(query.includeHidden));
    }

    return this.http.get<Categoria[]>(`${API_BASE_URL}/categorias`, { params });
  }

  getCategoriaById(id: number, includeHidden = false): Observable<Categoria> {
    const params = new HttpParams().set('includeHidden', String(includeHidden));
    return this.http.get<Categoria>(`${API_BASE_URL}/categorias/${id}`, { params });
  }

  getConfiguracionCatalogo(): Observable<ConfiguracionCatalogo> {
    return this.http.get<ConfiguracionCatalogo>(`${API_BASE_URL}/configuracion/catalogo`);
  }

  updateConfiguracionCatalogo(payload: PutConfiguracionCatalogo): Observable<ConfiguracionCatalogo> {
    return this.http.put<ConfiguracionCatalogo>(`${API_BASE_URL}/configuracion/catalogo`, payload);
  }

  getPermisosCatalogoActuales(): Observable<PermisosCatalogo> {
    return this.http.get<PermisosCatalogo>(`${API_BASE_URL}/permisos/catalogo/actuales`);
  }

  getPermisosCatalogoUser(): Observable<PermisosCatalogo> {
    return this.http.get<PermisosCatalogo>(`${API_BASE_URL}/permisos/catalogo/user`);
  }

  updatePermisosCatalogoUser(payload: PermisosCatalogo): Observable<PermisosCatalogo> {
    return this.http.put<PermisosCatalogo>(`${API_BASE_URL}/permisos/catalogo/user`, payload);
  }

  createCategoria(payload: PostCategoria): Observable<Categoria> {
    return this.http.post<Categoria>(`${API_BASE_URL}/categorias`, payload);
  }

  updateCategoria(id: number, payload: PutCategoria): Observable<void> {
    return this.http.put<void>(`${API_BASE_URL}/categorias/${id}`, payload);
  }

  deleteCategoria(id: number): Observable<void> {
    return this.http.delete<void>(`${API_BASE_URL}/categorias/${id}`);
  }

  getMarcas(query: CatalogoQuery = {}): Observable<Marca[]> {
    let params = new HttpParams();

    if (query.id && query.id > 0) {
      params = params.set('idMarca', String(query.id));
    }
    if (query.nombre) {
      params = params.set('nombre', query.nombre);
    }
    params = params.set('orderAsc', String(query.orderAsc ?? true));
    if (query.includeHidden !== undefined) {
      params = params.set('includeHidden', String(query.includeHidden));
    }

    return this.http.get<Marca[]>(`${API_BASE_URL}/marcas`, { params });
  }

  getMarcaById(id: number, includeHidden = false): Observable<Marca> {
    const params = new HttpParams().set('includeHidden', String(includeHidden));
    return this.http.get<Marca>(`${API_BASE_URL}/marcas/${id}`, { params });
  }

  createMarca(payload: PostMarca): Observable<Marca> {
    return this.http.post<Marca>(`${API_BASE_URL}/marcas`, payload);
  }

  updateMarca(id: number, payload: PutMarca): Observable<void> {
    return this.http.put<void>(`${API_BASE_URL}/marcas/${id}`, payload);
  }

  deleteMarca(id: number): Observable<void> {
    return this.http.delete<void>(`${API_BASE_URL}/marcas/${id}`);
  }

  getSoluciones(query: SolucionesQuery = {}): Observable<Solucion[]> {
    let params = new HttpParams();

    if (query.id && query.id > 0) {
      params = params.set('idSolucion', String(query.id));
    }
    if (query.titulo) {
      params = params.set('titulo', query.titulo);
    }
    params = params.set('orderAsc', String(query.orderAsc ?? true));

    return this.http.get<Solucion[]>(`${API_BASE_URL}/soluciones`, { params });
  }

  getSolucionById(id: number): Observable<Solucion> {
    return this.http.get<Solucion>(`${API_BASE_URL}/soluciones/${id}`);
  }

  createSolucion(payload: PostSolucion): Observable<Solucion> {
    return this.http.post<Solucion>(`${API_BASE_URL}/soluciones`, payload);
  }

  updateSolucion(id: number, payload: PutSolucion): Observable<void> {
    return this.http.put<void>(`${API_BASE_URL}/soluciones/${id}`, payload);
  }

  deleteSolucion(id: number): Observable<void> {
    return this.http.delete<void>(`${API_BASE_URL}/soluciones/${id}`);
  }

  getProductos(query: ProductosQuery = {}): Observable<Producto[]> {
    let params = new HttpParams();

    if (query.idProducto && query.idProducto > 0) {
      params = params.set('idProducto', String(query.idProducto));
    }
    if (query.nombre) {
      params = params.set('nombre', query.nombre);
    }
    if (query.categoriaId && query.categoriaId > 0) {
      params = params.set('categoriaId', String(query.categoriaId));
    }
    if (query.marcaId && query.marcaId > 0) {
      params = params.set('marcaId', String(query.marcaId));
    }
    params = params.set('orderAsc', String(query.orderAsc ?? true));
    if (query.includeHidden !== undefined) {
      params = params.set('includeHidden', String(query.includeHidden));
    }

    return this.http.get<Producto[]>(`${API_BASE_URL}/productos`, { params });
  }

  getMisProductos(orderAsc = true, includeHidden = true): Observable<Producto[]> {
    const params = new HttpParams()
      .set('orderAsc', String(orderAsc))
      .set('includeHidden', String(includeHidden));
    return this.http.get<Producto[]>(`${API_BASE_URL}/productos/mios`, { params });
  }

  getProductosPorUsuario(usuarioId: number, orderAsc = true, includeHidden = true): Observable<Producto[]> {
    const params = new HttpParams()
      .set('orderAsc', String(orderAsc))
      .set('includeHidden', String(includeHidden));
    return this.http.get<Producto[]>(`${API_BASE_URL}/productos/usuario/${usuarioId}`, { params });
  }

  getProductoById(id: number, includeHidden = false): Observable<Producto> {
    const params = new HttpParams().set('includeHidden', String(includeHidden));
    return this.http.get<Producto>(`${API_BASE_URL}/productos/${id}`, { params });
  }

  createProducto(payload: PostProducto, imagen?: File | null): Observable<Producto> {
    const formData = new FormData();
    formData.append('Nombre', payload.nombre);
    formData.append('Descripcion', payload.descripcion ?? '');
    formData.append('Precio', String(payload.precio));
    formData.append('Stock', String(payload.stock));
    formData.append('CategoriaId', String(payload.categoriaId));
    formData.append('MarcaId', String(payload.marcaId));

    if (payload.esVisible !== undefined && payload.esVisible !== null) {
      formData.append('EsVisible', String(payload.esVisible));
    }

    if (imagen) {
      formData.append('Imagen', imagen, imagen.name);
    }

    return this.http.post<Producto>(`${API_BASE_URL}/productos`, formData);
  }

  updateProducto(id: number, payload: PutProducto, imagen?: File | null): Observable<void> {
    if (imagen) {
      const formData = new FormData();

      if (payload.nombre !== undefined && payload.nombre !== null) {
        formData.append('Nombre', payload.nombre);
      }
      if (payload.descripcion !== undefined && payload.descripcion !== null) {
        formData.append('Descripcion', payload.descripcion);
      }
      if (payload.precio !== undefined && payload.precio !== null) {
        formData.append('Precio', String(payload.precio));
      }
      if (payload.stock !== undefined && payload.stock !== null) {
        formData.append('Stock', String(payload.stock));
      }
      if (payload.categoriaId !== undefined && payload.categoriaId !== null) {
        formData.append('CategoriaId', String(payload.categoriaId));
      }
      if (payload.marcaId !== undefined && payload.marcaId !== null) {
        formData.append('MarcaId', String(payload.marcaId));
      }
      if (payload.esVisible !== undefined && payload.esVisible !== null) {
        formData.append('EsVisible', String(payload.esVisible));
      }
      formData.append('Imagen', imagen, imagen.name);

      return this.http.put<void>(`${API_BASE_URL}/productos/${id}`, formData);
    }

    return this.http.put<void>(`${API_BASE_URL}/productos/${id}`, payload);
  }

  deleteProducto(id: number): Observable<void> {
    return this.http.delete<void>(`${API_BASE_URL}/productos/${id}`);
  }

  getProductoArchivos(productoId: number, includeHidden = false): Observable<ProductoArchivo[]> {
    const params = new HttpParams().set('includeHidden', String(includeHidden));
    return this.http.get<ProductoArchivo[]>(`${API_BASE_URL}/productos/${productoId}/archivos`, { params });
  }

  uploadProductoArchivo(productoId: number, file: File): Observable<ProductoArchivo> {
    const formData = new FormData();
    formData.append('archivo', file, file.name);
    return this.http.post<ProductoArchivo>(`${API_BASE_URL}/productos/${productoId}/archivos`, formData);
  }

  deleteProductoArchivo(productoId: number, archivoId: number): Observable<void> {
    return this.http.delete<void>(`${API_BASE_URL}/productos/${productoId}/archivos/${archivoId}`);
  }

  downloadProductoArchivo(productoId: number, archivoId: number, includeHidden = false): Observable<Blob> {
    const params = new HttpParams().set('includeHidden', String(includeHidden));
    return this.http.get(`${API_BASE_URL}/productos/${productoId}/archivos/${archivoId}`, {
      params,
      responseType: 'blob'
    });
  }

  getUsuarios(nombre = '', orderAsc = true): Observable<Usuario[]> {
    let params = new HttpParams().set('orderAsc', String(orderAsc));
    if (nombre) {
      params = params.set('nombre', nombre);
    }
    return this.http.get<Usuario[]>(`${API_BASE_URL}/usuarios`, { params });
  }

  getUsuarioById(id: number): Observable<Usuario> {
    return this.http.get<Usuario>(`${API_BASE_URL}/usuarios/${id}`);
  }

  createUsuario(payload: PostUsuario): Observable<Usuario> {
    return this.http.post<Usuario>(`${API_BASE_URL}/usuarios`, payload);
  }

  updateUsuario(id: number, payload: PutUsuario): Observable<void> {
    return this.http.put<void>(`${API_BASE_URL}/usuarios/${id}`, payload);
  }

  deleteUsuario(id: number): Observable<void> {
    return this.http.delete<void>(`${API_BASE_URL}/usuarios/${id}`);
  }

}
