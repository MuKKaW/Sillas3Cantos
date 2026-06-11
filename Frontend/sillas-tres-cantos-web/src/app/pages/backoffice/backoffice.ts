import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { Component, ElementRef, OnInit, ViewChild, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';
import { BackendApiService } from '../../core/services/backend-api.service';
import {
  BusquedaProductosExternos,
  Categoria,
  Marca,
  PostCategoria,
  PostMarca,
  PostProducto,
  PostUsuario,
  Producto,
  ProductoArchivo,
  PutCategoria,
  PutMarca,
  PutProducto,
  PutUsuario,
  Usuario
} from '../../core/models/api.models';

type BackofficeTab = 'productos' | 'categorias' | 'marcas' | 'usuarios' | 'externos';

interface ProductosCategoriaGrupo {
  categoriaId: number;
  categoriaNombre: string;
  productos: Producto[];
  totalStock: number;
}

@Component({
  selector: 'app-backoffice',
  imports: [CommonModule, FormsModule, CurrencyPipe, DatePipe],
  templateUrl: './backoffice.html',
  styleUrl: './backoffice.scss',
})
export class Backoffice implements OnInit {
  @ViewChild('productoImagenInput') private productoImagenInput?: ElementRef<HTMLInputElement>;

  private readonly api = inject(BackendApiService);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly isSuperAdmin = this.authService.isSuperAdmin;
  readonly activeTab = signal<BackofficeTab>('productos');
  readonly loading = signal(false);
  readonly uploading = signal(false);
  readonly statusMessage = signal('');
  readonly errorMessage = signal('');
  readonly expandedCategoriaIds = signal<Set<number>>(new Set<number>());
  readonly canManageUsers = computed(() => this.isSuperAdmin());

  categorias: Categoria[] = [];
  marcas: Marca[] = [];
  productos: Producto[] = [];
  usuarios: Usuario[] = [];
  externosResultado: BusquedaProductosExternos | null = null;

  filtroProductosNombre = '';
  filtroProductosCategoriaId = 0;
  filtroProductosMarcaId = 0;
  filtroProductosOrderAsc = true;
  filtroProductosIncludeHidden = true;
  modoProductos: 'todos' | 'mios' | 'usuario' = 'todos';
  filtroProductosUsuarioId = 0;
  filtroUsuariosNombre = '';
  buscarUsuarioId = 0;
  usuarioDetalle: Usuario | null = null;

  newProducto: PostProducto = {
    nombre: '',
    descripcion: '',
    precio: 0,
    stock: 0,
    categoriaId: 0,
    marcaId: 0,
    esVisible: true
  };

  newCategoria: PostCategoria = {
    nombre: '',
    descripcion: '',
    ordenVisual: 0,
    esVisible: true
  };

  newMarca: PostMarca = {
    nombre: '',
    descripcion: '',
    paisOrigen: '',
    anioFundacion: null,
    esVisible: true
  };

  newUsuario: PostUsuario = {
    username: '',
    password: '',
    role: 'User',
    nombre: '',
    apellido: '',
    email: ''
  };

  editProductoId: number | null = null;
  editProducto: PutProducto = {};
  editCategoriaId: number | null = null;
  editCategoria: PutCategoria = {};
  editMarcaId: number | null = null;
  editMarca: PutMarca = {};
  editUsuarioId: number | null = null;
  editUsuario: PutUsuario = {};

  productoArchivosActual: ProductoArchivo[] = [];
  productoArchivosActualId = 0;
  selectedUploadFile: File | null = null;
  selectedProductoImagen: File | null = null;

  externosQuery = 'chair';
  externosLimit = 8;
  externosSkip = 0;

  async ngOnInit(): Promise<void> {
    await this.cargarDatosIniciales();
  }

  async cargarDatosIniciales(): Promise<void> {
    this.loading.set(true);
    this.errorMessage.set('');

    try {
      const [categorias, marcas] = await Promise.all([
        firstValueFrom(this.api.getCategorias({ includeHidden: true, orderAsc: true })),
        firstValueFrom(this.api.getMarcas({ includeHidden: true, orderAsc: true }))
      ]);
      this.categorias = categorias;
      this.marcas = marcas;
      await this.cargarProductos();
      if (this.canManageUsers()) {
        await this.cargarUsuarios();
      }
      this.statusMessage.set('Portal preparado.');
    } catch (error) {
      console.error(error);
      this.errorMessage.set('No se pudo cargar el backoffice. Revisa login/API.');
    } finally {
      this.loading.set(false);
    }
  }

  setTab(tab: BackofficeTab): void {
    if (tab === 'usuarios' && !this.canManageUsers()) {
      this.errorMessage.set('Solo SuperAdmin puede gestionar usuarios.');
      return;
    }
    this.activeTab.set(tab);
    this.statusMessage.set('');
    this.errorMessage.set('');
  }

  async logout(): Promise<void> {
    this.authService.logout();
    await this.router.navigateByUrl('/');
  }

  async goHome(): Promise<void> {
    await this.router.navigateByUrl('/');
  }

  clearError(): void {
    this.errorMessage.set('');
  }

  async cargarProductos(): Promise<void> {
    try {
      if (this.modoProductos === 'mios') {
        this.productos = await firstValueFrom(
          this.api.getMisProductos(this.filtroProductosOrderAsc, this.filtroProductosIncludeHidden)
        );
      } else if (this.modoProductos === 'usuario') {
        if (!this.canManageUsers()) {
          this.errorMessage.set('Solo SuperAdmin puede consultar productos por usuario.');
          return;
        }
        if (this.filtroProductosUsuarioId <= 0) {
          this.errorMessage.set('Indica un usuarioId valido para este modo.');
          return;
        }
        this.productos = await firstValueFrom(
          this.api.getProductosPorUsuario(
            this.filtroProductosUsuarioId,
            this.filtroProductosOrderAsc,
            this.filtroProductosIncludeHidden
          )
        );
      } else {
        this.productos = await firstValueFrom(
          this.api.getProductos({
            nombre: this.filtroProductosNombre.trim(),
            categoriaId: this.filtroProductosCategoriaId || undefined,
            marcaId: this.filtroProductosMarcaId || undefined,
            orderAsc: this.filtroProductosOrderAsc,
            includeHidden: this.filtroProductosIncludeHidden
          })
        );
      }
      this.abrirCategoriasConProductos();
      this.statusMessage.set(`Productos cargados (${this.productos.length}).`);
    } catch (error) {
      console.error(error);
      this.errorMessage.set('Error al cargar productos.');
    }
  }

  async crearProducto(): Promise<void> {
    if (!this.newProducto.categoriaId || !this.newProducto.marcaId) {
      this.errorMessage.set('Debes seleccionar categoría y marca antes de crear el producto.');
      return;
    }

    try {
      await firstValueFrom(this.api.createProducto(this.newProducto, this.selectedProductoImagen));
      this.resetNewProductoForm();
      await this.cargarProductos();
      this.statusMessage.set('Producto creado.');
    } catch (error) {
      console.error(error);
      this.errorMessage.set('No se pudo crear el producto.');
    }
  }

  productosPorCategoria(): ProductosCategoriaGrupo[] {
    const grupos = new Map<number, ProductosCategoriaGrupo>();

    for (const producto of this.productos) {
      const categoriaId = producto.categoriaId || 0;
      const grupo = grupos.get(categoriaId) ?? {
        categoriaId,
        categoriaNombre: this.categoriaNombre(categoriaId),
        productos: [],
        totalStock: 0
      };

      grupo.productos.push(producto);
      grupo.totalStock += producto.stock;
      grupos.set(categoriaId, grupo);
    }

    return Array.from(grupos.values()).sort((a, b) =>
      a.categoriaNombre.localeCompare(b.categoriaNombre, 'es')
    );
  }

  isCategoriaExpanded(categoriaId: number): boolean {
    return this.expandedCategoriaIds().has(categoriaId);
  }

  toggleCategoria(categoriaId: number): void {
    const expanded = new Set(this.expandedCategoriaIds());

    if (expanded.has(categoriaId)) {
      expanded.delete(categoriaId);
    } else {
      expanded.add(categoriaId);
    }

    this.expandedCategoriaIds.set(expanded);
  }

  empezarEdicionProducto(producto: Producto): void {
    this.editProductoId = producto.id;
    this.editProducto = {
      nombre: producto.nombre,
      descripcion: producto.descripcion,
      precio: producto.precio,
      stock: producto.stock,
      categoriaId: producto.categoriaId,
      marcaId: producto.marcaId,
      esVisible: producto.esVisible
    };
  }

  cancelarEdicionProducto(): void {
    this.editProductoId = null;
    this.editProducto = {};
  }

  async guardarEdicionProducto(productoId: number): Promise<void> {
    try {
      await firstValueFrom(this.api.updateProducto(productoId, this.editProducto));
      this.cancelarEdicionProducto();
      await this.cargarProductos();
      this.statusMessage.set('Producto actualizado.');
    } catch (error) {
      console.error(error);
      this.errorMessage.set('No se pudo actualizar el producto.');
    }
  }

  async eliminarProducto(productoId: number): Promise<void> {
    if (!confirm('¿Eliminar producto?')) {
      return;
    }

    try {
      await firstValueFrom(this.api.deleteProducto(productoId));
      await this.cargarProductos();
      if (this.productoArchivosActualId === productoId) {
        this.productoArchivosActual = [];
        this.productoArchivosActualId = 0;
      }
      this.statusMessage.set('Producto eliminado.');
    } catch (error) {
      console.error(error);
      this.errorMessage.set('No se pudo eliminar el producto.');
    }
  }

  async cargarCategorias(): Promise<void> {
    try {
      this.categorias = await firstValueFrom(
        this.api.getCategorias({ includeHidden: true, orderAsc: true })
      );
    } catch (error) {
      console.error(error);
      this.errorMessage.set('Error al cargar categorias.');
    }
  }

  async crearCategoria(): Promise<void> {
    try {
      await firstValueFrom(this.api.createCategoria(this.newCategoria));
      this.newCategoria = {
        nombre: '',
        descripcion: '',
        ordenVisual: 0,
        esVisible: true
      };
      await this.cargarCategorias();
      this.statusMessage.set('Categoria creada.');
    } catch (error) {
      console.error(error);
      this.errorMessage.set('No se pudo crear la categoria.');
    }
  }

  empezarEdicionCategoria(categoria: Categoria): void {
    this.editCategoriaId = categoria.id;
    this.editCategoria = {
      nombre: categoria.nombre,
      descripcion: categoria.descripcion,
      ordenVisual: categoria.ordenVisual,
      esVisible: categoria.esVisible
    };
  }

  cancelarEdicionCategoria(): void {
    this.editCategoriaId = null;
    this.editCategoria = {};
  }

  async guardarEdicionCategoria(categoriaId: number): Promise<void> {
    try {
      await firstValueFrom(this.api.updateCategoria(categoriaId, this.editCategoria));
      this.cancelarEdicionCategoria();
      await this.cargarCategorias();
      this.statusMessage.set('Categoria actualizada.');
    } catch (error) {
      console.error(error);
      this.errorMessage.set('No se pudo actualizar la categoria.');
    }
  }

  async eliminarCategoria(categoriaId: number): Promise<void> {
    if (!confirm('¿Eliminar categoria?')) {
      return;
    }
    try {
      await firstValueFrom(this.api.deleteCategoria(categoriaId));
      await this.cargarCategorias();
      this.statusMessage.set('Categoria eliminada.');
    } catch (error) {
      console.error(error);
      this.errorMessage.set('No se pudo eliminar la categoria.');
    }
  }

  async cargarMarcas(): Promise<void> {
    try {
      this.marcas = await firstValueFrom(
        this.api.getMarcas({ includeHidden: true, orderAsc: true })
      );
    } catch (error) {
      console.error(error);
      this.errorMessage.set('Error al cargar marcas.');
    }
  }

  async crearMarca(): Promise<void> {
    try {
      await firstValueFrom(this.api.createMarca(this.newMarca));
      this.newMarca = {
        nombre: '',
        descripcion: '',
        paisOrigen: '',
        anioFundacion: null,
        esVisible: true
      };
      await this.cargarMarcas();
      this.statusMessage.set('Marca creada.');
    } catch (error) {
      console.error(error);
      this.errorMessage.set('No se pudo crear la marca.');
    }
  }

  empezarEdicionMarca(marca: Marca): void {
    this.editMarcaId = marca.id;
    this.editMarca = {
      nombre: marca.nombre,
      descripcion: marca.descripcion,
      paisOrigen: marca.paisOrigen,
      anioFundacion: marca.anioFundacion,
      esVisible: marca.esVisible
    };
  }

  cancelarEdicionMarca(): void {
    this.editMarcaId = null;
    this.editMarca = {};
  }

  async guardarEdicionMarca(marcaId: number): Promise<void> {
    try {
      await firstValueFrom(this.api.updateMarca(marcaId, this.editMarca));
      this.cancelarEdicionMarca();
      await this.cargarMarcas();
      this.statusMessage.set('Marca actualizada.');
    } catch (error) {
      console.error(error);
      this.errorMessage.set('No se pudo actualizar la marca.');
    }
  }

  async eliminarMarca(marcaId: number): Promise<void> {
    if (!confirm('¿Eliminar marca?')) {
      return;
    }
    try {
      await firstValueFrom(this.api.deleteMarca(marcaId));
      await this.cargarMarcas();
      this.statusMessage.set('Marca eliminada.');
    } catch (error) {
      console.error(error);
      this.errorMessage.set('No se pudo eliminar la marca.');
    }
  }

  async cargarUsuarios(): Promise<void> {
    if (!this.canManageUsers()) {
      return;
    }
    try {
      this.usuarios = await firstValueFrom(
        this.api.getUsuarios(this.filtroUsuariosNombre.trim(), true)
      );
    } catch (error) {
      console.error(error);
      this.errorMessage.set('No se pudieron cargar usuarios.');
    }
  }

  async buscarUsuarioPorId(): Promise<void> {
    if (!this.canManageUsers()) {
      return;
    }
    if (this.buscarUsuarioId <= 0) {
      this.errorMessage.set('Indica un ID de usuario valido.');
      this.usuarioDetalle = null;
      return;
    }
    try {
      this.usuarioDetalle = await firstValueFrom(this.api.getUsuarioById(this.buscarUsuarioId));
      this.statusMessage.set(`Usuario ${this.usuarioDetalle.id} cargado.`);
    } catch (error) {
      console.error(error);
      this.usuarioDetalle = null;
      this.errorMessage.set('No se pudo obtener el usuario por ID.');
    }
  }

  async crearUsuario(): Promise<void> {
    if (!this.canManageUsers()) {
      this.errorMessage.set('Solo SuperAdmin puede crear usuarios.');
      return;
    }
    try {
      await firstValueFrom(this.api.createUsuario(this.newUsuario));
      this.newUsuario = {
        username: '',
        password: '',
        role: 'User',
        nombre: '',
        apellido: '',
        email: ''
      };
      await this.cargarUsuarios();
      this.statusMessage.set('Usuario creado.');
    } catch (error) {
      console.error(error);
      this.errorMessage.set('No se pudo crear el usuario.');
    }
  }

  empezarEdicionUsuario(usuario: Usuario): void {
    this.editUsuarioId = usuario.id;
    this.editUsuario = {
      username: usuario.username ?? undefined,
      role: usuario.role ?? undefined,
      nombre: usuario.nombre ?? undefined,
      apellido: usuario.apellido ?? undefined,
      email: usuario.email,
      estaActivo: usuario.estaActivo ?? true
    };
  }

  cancelarEdicionUsuario(): void {
    this.editUsuarioId = null;
    this.editUsuario = {};
  }

  async guardarEdicionUsuario(usuarioId: number): Promise<void> {
    if (!this.canManageUsers()) {
      return;
    }
    try {
      await firstValueFrom(this.api.updateUsuario(usuarioId, this.editUsuario));
      this.cancelarEdicionUsuario();
      await this.cargarUsuarios();
      this.statusMessage.set('Usuario actualizado.');
    } catch (error) {
      console.error(error);
      this.errorMessage.set('No se pudo actualizar el usuario.');
    }
  }

  async eliminarUsuario(usuarioId: number): Promise<void> {
    if (!this.canManageUsers()) {
      return;
    }
    if (!confirm('¿Eliminar usuario?')) {
      return;
    }

    try {
      await firstValueFrom(this.api.deleteUsuario(usuarioId));
      await this.cargarUsuarios();
      this.statusMessage.set('Usuario eliminado.');
    } catch (error) {
      console.error(error);
      this.errorMessage.set('No se pudo eliminar el usuario.');
    }
  }

  async cargarArchivosProducto(productoId: number): Promise<void> {
    this.productoArchivosActualId = productoId;
    this.selectedUploadFile = null;
    this.uploading.set(true);

    try {
      this.productoArchivosActual = await firstValueFrom(
        this.api.getProductoArchivos(productoId, true)
      );
      this.statusMessage.set(`Archivos cargados (${this.productoArchivosActual.length}).`);
    } catch (error) {
      console.error(error);
      this.errorMessage.set('No se pudieron cargar archivos del producto.');
      this.productoArchivosActual = [];
    } finally {
      this.uploading.set(false);
    }
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0] ?? null;
    this.selectedUploadFile = file;
  }

  onProductoImagenSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.selectedProductoImagen = input.files?.[0] ?? null;
  }

  async subirArchivoProducto(): Promise<void> {
    if (!this.productoArchivosActualId || !this.selectedUploadFile) {
      this.errorMessage.set('Selecciona producto y archivo antes de subir.');
      return;
    }

    this.uploading.set(true);
    try {
      await firstValueFrom(
        this.api.uploadProductoArchivo(this.productoArchivosActualId, this.selectedUploadFile)
      );
      await this.cargarArchivosProducto(this.productoArchivosActualId);
      this.statusMessage.set('Archivo subido.');
    } catch (error) {
      console.error(error);
      this.errorMessage.set('No se pudo subir el archivo.');
    } finally {
      this.uploading.set(false);
    }
  }

  async descargarArchivoProducto(archivo: ProductoArchivo): Promise<void> {
    try {
      const blob = await firstValueFrom(
        this.api.downloadProductoArchivo(archivo.productoId, archivo.id, true)
      );
      const objectUrl = URL.createObjectURL(blob);
      const anchor = document.createElement('a');
      anchor.href = objectUrl;
      anchor.download = archivo.nombreOriginal;
      anchor.click();
      URL.revokeObjectURL(objectUrl);
    } catch (error) {
      console.error(error);
      this.errorMessage.set('No se pudo descargar el archivo.');
    }
  }

  async eliminarArchivoProducto(archivo: ProductoArchivo): Promise<void> {
    if (!confirm('¿Eliminar archivo?')) {
      return;
    }
    try {
      await firstValueFrom(this.api.deleteProductoArchivo(archivo.productoId, archivo.id));
      await this.cargarArchivosProducto(archivo.productoId);
      this.statusMessage.set('Archivo eliminado.');
    } catch (error) {
      console.error(error);
      this.errorMessage.set('No se pudo eliminar el archivo.');
    }
  }

  async buscarExternos(): Promise<void> {
    if (this.externosQuery.trim().length < 2) {
      this.errorMessage.set('La query externa requiere minimo 2 caracteres.');
      return;
    }
    try {
      this.externosResultado = await firstValueFrom(
        this.api.buscarProductosExternos(this.externosQuery.trim(), this.externosLimit, this.externosSkip)
      );
      this.statusMessage.set('Consulta externa completada.');
    } catch (error) {
      console.error(error);
      this.errorMessage.set('No se pudo consultar la API externa.');
    }
  }

  categoriaNombre(categoriaId: number): string {
    return this.categorias.find((categoria) => categoria.id === categoriaId)?.nombre ?? 'Sin categoria';
  }

  marcaNombre(marcaId: number): string {
    return this.marcas.find((marca) => marca.id === marcaId)?.nombre ?? 'Sin marca';
  }

  productoArchivosActualNombre(): string {
    return this.productos.find((producto) => producto.id === this.productoArchivosActualId)?.nombre ?? 'Producto seleccionado';
  }

  private abrirCategoriasConProductos(): void {
    this.expandedCategoriaIds.set(new Set(this.productos.map((producto) => producto.categoriaId || 0)));
  }

  private resetNewProductoForm(): void {
    this.newProducto = {
      nombre: '',
      descripcion: '',
      precio: 0,
      stock: 0,
      categoriaId: 0,
      marcaId: 0,
      esVisible: true
    };
    this.selectedProductoImagen = null;

    if (this.productoImagenInput) {
      this.productoImagenInput.nativeElement.value = '';
    }
  }
}
