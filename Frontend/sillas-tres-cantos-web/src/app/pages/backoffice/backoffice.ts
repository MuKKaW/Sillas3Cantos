import { CommonModule, CurrencyPipe, DOCUMENT, DatePipe } from '@angular/common';
import { Component, ElementRef, OnDestroy, OnInit, ViewChild, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';
import { BackendApiService } from '../../core/services/backend-api.service';
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
  PutCategoria,
  PutMarca,
  PutProducto,
  PutSolucion,
  PutUsuario,
  Solucion,
  Usuario
} from '../../core/models/api.models';

type BackofficeTab = 'productos' | 'categorias' | 'marcas' | 'soluciones' | 'administracion' | 'usuarios';
type SortableList = 'categorias' | 'marcas' | 'soluciones';

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
export class Backoffice implements OnInit, OnDestroy {
  @ViewChild('productoImagenInput') private productoImagenInput?: ElementRef<HTMLInputElement>;
  @ViewChild('editProductoImagenInput') private editProductoImagenInput?: ElementRef<HTMLInputElement>;
  @ViewChild('productoArchivoInput') private productoArchivoInput?: ElementRef<HTMLInputElement>;

  private readonly api = inject(BackendApiService);
  private readonly authService = inject(AuthService);
  private readonly document = inject(DOCUMENT);
  private readonly router = inject(Router);

  readonly isSuperAdmin = this.authService.isSuperAdmin;
  readonly activeTab = signal<BackofficeTab>('productos');
  readonly loading = signal(false);
  readonly uploading = signal(false);
  readonly savingProducto = signal(false);
  readonly guardandoConfiguracion = signal(false);
  readonly guardandoPermisos = signal(false);
  readonly statusMessage = signal('');
  readonly errorMessage = signal('');
  readonly expandedCategoriaIds = signal<Set<number>>(new Set<number>());
  readonly settlingList = signal<SortableList | null>(null);
  readonly canManageUsers = computed(() => this.isSuperAdmin());

  categorias: Categoria[] = [];
  marcas: Marca[] = [];
  soluciones: Solucion[] = [];
  productos: Producto[] = [];
  usuarios: Usuario[] = [];
  configuracionCatalogo: ConfiguracionCatalogo = {
    usarFiltroTabs: false,
    mostrarPrecios: false,
    mostrarStock: false,
    mostrarSeccionCatalogo: true,
    mostrarSeccionSoluciones: true,
    mostrarSeccionMapa: true,
    mostrarSeccionConocenos: true,
    fechaActualizacion: ''
  };
  permisosActuales: PermisosCatalogo = this.defaultPermisosCatalogo();
  permisosUser: PermisosCatalogo = this.defaultPermisosCatalogo();

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
    ordenVisual: 0,
    esVisible: true
  };

  newSolucion: PostSolucion = {
    titulo: '',
    texto: '',
    emoji: '',
    ordenVisual: 0
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
  editProductoActual: Producto | null = null;
  editProducto: PutProducto = {};
  editCategoriaId: number | null = null;
  editCategoriaActual: Categoria | null = null;
  editCategoria: PutCategoria = {};
  editMarcaId: number | null = null;
  editMarcaActual: Marca | null = null;
  editMarca: PutMarca = {};
  editSolucionId: number | null = null;
  editSolucionActual: Solucion | null = null;
  editSolucion: PutSolucion = {};
  editUsuarioId: number | null = null;
  editUsuario: PutUsuario = {};

  productoArchivosActual: ProductoArchivo[] = [];
  productoArchivosActualId = 0;
  selectedUploadFile: File | null = null;
  selectedProductoImagen: File | null = null;
  selectedEditProductoImagen: File | null = null;
  draggedList: SortableList | null = null;
  draggedIndex = -1;
  dropIndex = -1;

  async ngOnInit(): Promise<void> {
    await this.cargarDatosIniciales();
  }

  ngOnDestroy(): void {
    this.setBodyScrollLocked(false);
  }

  async cargarDatosIniciales(): Promise<void> {
    this.loading.set(true);
    this.errorMessage.set('');

    try {
      const configuracionPorDefecto: ConfiguracionCatalogo = {
        usarFiltroTabs: false,
        mostrarPrecios: false,
        mostrarStock: false,
        mostrarSeccionCatalogo: true,
        mostrarSeccionSoluciones: true,
        mostrarSeccionMapa: true,
        mostrarSeccionConocenos: true,
        fechaActualizacion: ''
      };
      const permisosPorDefecto = this.defaultPermisosCatalogo();
      const [categorias, marcas, soluciones, configuracionCatalogo, permisosActuales] = await Promise.all([
        firstValueFrom(this.api.getCategorias({ includeHidden: true, orderAsc: true })),
        firstValueFrom(this.api.getMarcas({ includeHidden: true, orderAsc: true })),
        firstValueFrom(this.api.getSoluciones({ orderAsc: true })),
        firstValueFrom(this.api.getConfiguracionCatalogo()).catch(() => configuracionPorDefecto),
        firstValueFrom(this.api.getPermisosCatalogoActuales()).catch(() => permisosPorDefecto)
      ]);
      this.categorias = categorias;
      this.marcas = marcas;
      this.soluciones = soluciones;
      this.configuracionCatalogo = { ...configuracionPorDefecto, ...configuracionCatalogo };
      this.permisosActuales = this.normalizePermisosCatalogo(permisosActuales);
      await this.cargarProductos();
      if (this.canManageUsers()) {
        const permisosUser = await firstValueFrom(this.api.getPermisosCatalogoUser()).catch(() => permisosPorDefecto);
        this.permisosUser = this.normalizePermisosCatalogo(permisosUser);
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

  async guardarConfiguracionCatalogo(): Promise<void> {
    this.guardandoConfiguracion.set(true);
    this.errorMessage.set('');

    try {
      const payload = {
        usarFiltroTabs: this.configuracionCatalogo.usarFiltroTabs,
        mostrarPrecios: this.configuracionCatalogo.mostrarPrecios,
        mostrarStock: this.configuracionCatalogo.mostrarStock,
        mostrarSeccionCatalogo: this.configuracionCatalogo.mostrarSeccionCatalogo,
        mostrarSeccionSoluciones: this.configuracionCatalogo.mostrarSeccionSoluciones,
        mostrarSeccionMapa: this.configuracionCatalogo.mostrarSeccionMapa,
        mostrarSeccionConocenos: this.configuracionCatalogo.mostrarSeccionConocenos
      };
      const configuracionActualizada = await firstValueFrom(this.api.updateConfiguracionCatalogo(payload));
      this.configuracionCatalogo = {
        ...this.configuracionCatalogo,
        ...payload,
        ...configuracionActualizada
      };
      this.statusMessage.set('Configuracion de la landing actualizada.');
    } catch (error) {
      console.error(error);
      this.errorMessage.set('No se pudo guardar la configuracion de la landing.');
    } finally {
      this.guardandoConfiguracion.set(false);
    }
  }

  async guardarPermisosUser(): Promise<void> {
    if (!this.canManageUsers()) {
      return;
    }

    this.guardandoPermisos.set(true);
    this.errorMessage.set('');

    try {
      this.permisosUser = this.normalizePermisosCatalogo(
        await firstValueFrom(this.api.updatePermisosCatalogoUser(this.permisosUser))
      );
      this.statusMessage.set('Permisos del rol User actualizados.');
    } catch (error) {
      console.error(error);
      this.errorMessage.set('No se pudieron guardar los permisos del rol User.');
    } finally {
      this.guardandoPermisos.set(false);
    }
  }

  modoFiltroCatalogo(): string {
    return this.configuracionCatalogo.usarFiltroTabs ? 'Filtro nuevo por tabs' : 'Filtro actual';
  }

  modoVisibilidadCatalogo(): string {
    const visibles = [
      this.configuracionCatalogo.mostrarPrecios ? 'precios' : '',
      this.configuracionCatalogo.mostrarStock ? 'stock' : ''
    ].filter(Boolean);

    return visibles.length > 0 ? `Muestra ${visibles.join(' y ')}` : 'Precio y stock ocultos';
  }

  productoObligatoriosCompletos(): boolean {
    return this.newProducto.nombre.trim().length > 0
      && this.hasNonNegativeNumber(this.newProducto.precio)
      && this.hasNonNegativeNumber(this.newProducto.stock)
      && this.newProducto.categoriaId > 0
      && this.newProducto.marcaId > 0;
  }

  categoriaObligatoriosCompletos(): boolean {
    return this.newCategoria.nombre.trim().length > 0;
  }

  marcaObligatoriosCompletos(): boolean {
    return this.newMarca.nombre.trim().length > 0;
  }

  solucionObligatoriosCompletos(): boolean {
    return this.newSolucion.titulo.trim().length > 0
      && this.newSolucion.texto.trim().length > 0
      && this.newSolucion.emoji.trim().length > 0;
  }

  canCrearProducto(): boolean {
    return this.permisosActuales.productos.crear && this.productoObligatoriosCompletos();
  }

  canCrearCategoria(): boolean {
    return this.permisosActuales.categorias.crear && this.categoriaObligatoriosCompletos();
  }

  canCrearMarca(): boolean {
    return this.permisosActuales.marcas.crear && this.marcaObligatoriosCompletos();
  }

  canCrearSolucion(): boolean {
    return this.permisosActuales.soluciones.crear && this.solucionObligatoriosCompletos();
  }

  canModificarCategorias(): boolean {
    return this.permisosActuales.categorias.modificar;
  }

  canEliminarCategorias(): boolean {
    return this.permisosActuales.categorias.eliminar;
  }

  canModificarMarcas(): boolean {
    return this.permisosActuales.marcas.modificar;
  }

  canEliminarMarcas(): boolean {
    return this.permisosActuales.marcas.eliminar;
  }

  canModificarProductos(): boolean {
    return this.permisosActuales.productos.modificar;
  }

  canEliminarProductos(): boolean {
    return this.permisosActuales.productos.eliminar;
  }

  canModificarSoluciones(): boolean {
    return this.permisosActuales.soluciones.modificar;
  }

  canEliminarSoluciones(): boolean {
    return this.permisosActuales.soluciones.eliminar;
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
    if (!this.permisosActuales.productos.crear) {
      this.errorMessage.set('No tienes permiso para crear productos.');
      return;
    }
    if (!this.productoObligatoriosCompletos()) {
      this.errorMessage.set('Completa los campos obligatorios antes de crear el producto.');
      return;
    }
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
      this.categoriaOrden(a.categoriaId) - this.categoriaOrden(b.categoriaId)
      || a.categoriaNombre.localeCompare(b.categoriaNombre, 'es')
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

  async empezarEdicionProducto(producto: Producto): Promise<void> {
    if (!this.canModificarProductos()) {
      this.errorMessage.set('No tienes permiso para modificar productos.');
      return;
    }

    this.editProductoId = producto.id;
    this.editProductoActual = producto;
    this.editProducto = {
      nombre: producto.nombre,
      descripcion: producto.descripcion,
      precio: producto.precio,
      stock: producto.stock,
      categoriaId: producto.categoriaId,
      marcaId: producto.marcaId,
      esVisible: producto.esVisible
    };
    this.selectedEditProductoImagen = null;
    this.setBodyScrollLocked(true);
    await this.cargarArchivosProducto(producto.id);
  }

  cancelarEdicionProducto(): void {
    this.editProductoId = null;
    this.editProductoActual = null;
    this.editProducto = {};
    this.selectedEditProductoImagen = null;
    this.selectedUploadFile = null;
    this.productoArchivosActual = [];
    this.productoArchivosActualId = 0;
    this.uploading.set(false);
    this.savingProducto.set(false);
    this.setBodyScrollLocked(false);

    if (this.editProductoImagenInput) {
      this.editProductoImagenInput.nativeElement.value = '';
    }

    if (this.productoArchivoInput) {
      this.productoArchivoInput.nativeElement.value = '';
    }
  }

  async guardarEdicionProducto(productoId: number): Promise<void> {
    this.savingProducto.set(true);
    try {
      await firstValueFrom(this.api.updateProducto(productoId, this.editProducto, this.selectedEditProductoImagen));
      await this.cargarProductos();
      this.cancelarEdicionProducto();
      this.statusMessage.set('Producto actualizado.');
    } catch (error) {
      console.error(error);
      this.errorMessage.set('No se pudo actualizar el producto.');
    } finally {
      this.savingProducto.set(false);
    }
  }

  async eliminarProducto(productoId: number): Promise<void> {
    if (!this.canEliminarProductos()) {
      this.errorMessage.set('No tienes permiso para eliminar productos.');
      return;
    }

    if (!confirm('¿Eliminar producto?')) {
      return;
    }

    try {
      await firstValueFrom(this.api.deleteProducto(productoId));
      await this.cargarProductos();
      if (this.productoArchivosActualId === productoId) {
        this.cancelarEdicionProducto();
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
    if (!this.permisosActuales.categorias.crear) {
      this.errorMessage.set('No tienes permiso para crear categorias.');
      return;
    }
    if (!this.categoriaObligatoriosCompletos()) {
      this.errorMessage.set('Completa los campos obligatorios antes de crear la categoria.');
      return;
    }

    try {
      await firstValueFrom(
        this.api.createCategoria({
          ...this.newCategoria,
          ordenVisual: this.nextOrdenVisual(this.categorias)
        })
      );
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
    if (!this.canModificarCategorias()) {
      this.errorMessage.set('No tienes permiso para modificar categorias.');
      return;
    }

    this.editCategoriaId = categoria.id;
    this.editCategoriaActual = categoria;
    this.editCategoria = {
      nombre: categoria.nombre,
      descripcion: categoria.descripcion,
      ordenVisual: categoria.ordenVisual,
      esVisible: categoria.esVisible
    };
    this.setBodyScrollLocked(true);
  }

  cancelarEdicionCategoria(): void {
    this.editCategoriaId = null;
    this.editCategoriaActual = null;
    this.editCategoria = {};
    this.setBodyScrollLocked(false);
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
    if (!this.canEliminarCategorias()) {
      this.errorMessage.set('No tienes permiso para eliminar categorias.');
      return;
    }

    if (!confirm('¿Eliminar categoria?')) {
      return;
    }
    try {
      await firstValueFrom(this.api.deleteCategoria(categoriaId));
      await this.cargarCategorias();
      if (this.editCategoriaId === categoriaId) {
        this.cancelarEdicionCategoria();
      }
      this.statusMessage.set('Categoria eliminada.');
    } catch (error) {
      console.error(error);
      this.errorMessage.set('No se pudo eliminar la categoria.');
    }
  }

  onReorderDragStart(kind: SortableList, index: number, event: DragEvent): void {
    if (this.isEditingSortableItem(kind, index)) {
      event.preventDefault();
      return;
    }

    this.draggedList = kind;
    this.draggedIndex = index;
    this.dropIndex = index;

    if (event.dataTransfer) {
      event.dataTransfer.effectAllowed = 'move';
      event.dataTransfer.setData('text/plain', `${kind}:${index}`);
    }
  }

  onReorderItemDragOver(kind: SortableList, index: number, event: DragEvent): void {
    if (!this.canDropOnList(kind)) {
      return;
    }

    event.preventDefault();
    event.stopPropagation();
    this.dropIndex = this.getItemDropIndex(index, event);

    if (event.dataTransfer) {
      event.dataTransfer.dropEffect = 'move';
    }
  }

  onReorderListDragOver(kind: SortableList, event: DragEvent): void {
    if (!this.canDropOnList(kind)) {
      return;
    }

    const target = event.target as HTMLElement | null;
    if (target?.closest('.sortable-item')) {
      return;
    }

    event.preventDefault();
    this.dropIndex = this.getSortableItems(kind).length;

    if (event.dataTransfer) {
      event.dataTransfer.dropEffect = 'move';
    }
  }

  async onReorderItemDrop(kind: SortableList, index: number, event: DragEvent): Promise<void> {
    if (!this.canDropOnList(kind)) {
      return;
    }

    event.preventDefault();
    event.stopPropagation();
    await this.commitReorder(kind, this.getItemDropIndex(index, event));
  }

  async onReorderListDrop(kind: SortableList, event: DragEvent): Promise<void> {
    if (!this.canDropOnList(kind)) {
      return;
    }

    const target = event.target as HTMLElement | null;
    if (target?.closest('.sortable-item')) {
      return;
    }

    event.preventDefault();
    await this.commitReorder(kind, this.getSortableItems(kind).length);
  }

  onReorderDragEnd(): void {
    this.clearDragState();
  }

  isDraggingItem(kind: SortableList, index: number): boolean {
    return this.draggedList === kind && this.draggedIndex === index;
  }

  isDropBefore(kind: SortableList, index: number): boolean {
    return this.draggedList === kind && this.dropIndex === index && this.draggedIndex !== index;
  }

  isDropAfter(kind: SortableList, index: number): boolean {
    const items = this.getSortableItems(kind);
    return this.draggedList === kind
      && this.dropIndex === items.length
      && index === items.length - 1
      && this.draggedIndex !== index;
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
    if (!this.permisosActuales.marcas.crear) {
      this.errorMessage.set('No tienes permiso para crear marcas.');
      return;
    }
    if (!this.marcaObligatoriosCompletos()) {
      this.errorMessage.set('Completa los campos obligatorios antes de crear la marca.');
      return;
    }

    try {
      await firstValueFrom(
        this.api.createMarca({
          ...this.newMarca,
          ordenVisual: this.nextOrdenVisual(this.marcas)
        })
      );
      this.newMarca = {
        nombre: '',
        descripcion: '',
        paisOrigen: '',
        anioFundacion: null,
        ordenVisual: 0,
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
    if (!this.canModificarMarcas()) {
      this.errorMessage.set('No tienes permiso para modificar marcas.');
      return;
    }

    this.editMarcaId = marca.id;
    this.editMarcaActual = marca;
    this.editMarca = {
      nombre: marca.nombre,
      descripcion: marca.descripcion,
      paisOrigen: marca.paisOrigen,
      anioFundacion: marca.anioFundacion,
      ordenVisual: marca.ordenVisual,
      esVisible: marca.esVisible
    };
    this.setBodyScrollLocked(true);
  }

  cancelarEdicionMarca(): void {
    this.editMarcaId = null;
    this.editMarcaActual = null;
    this.editMarca = {};
    this.setBodyScrollLocked(false);
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
    if (!this.canEliminarMarcas()) {
      this.errorMessage.set('No tienes permiso para eliminar marcas.');
      return;
    }

    if (!confirm('¿Eliminar marca?')) {
      return;
    }
    try {
      await firstValueFrom(this.api.deleteMarca(marcaId));
      await this.cargarMarcas();
      if (this.editMarcaId === marcaId) {
        this.cancelarEdicionMarca();
      }
      this.statusMessage.set('Marca eliminada.');
    } catch (error) {
      console.error(error);
      this.errorMessage.set('No se pudo eliminar la marca.');
    }
  }

  async cargarSoluciones(): Promise<void> {
    try {
      this.soluciones = await firstValueFrom(
        this.api.getSoluciones({ orderAsc: true })
      );
    } catch (error) {
      console.error(error);
      this.errorMessage.set('Error al cargar soluciones.');
    }
  }

  async crearSolucion(): Promise<void> {
    if (!this.permisosActuales.soluciones.crear) {
      this.errorMessage.set('No tienes permiso para crear soluciones.');
      return;
    }
    if (!this.solucionObligatoriosCompletos()) {
      this.errorMessage.set('Completa los campos obligatorios antes de crear la solucion.');
      return;
    }

    try {
      await firstValueFrom(
        this.api.createSolucion({
          ...this.newSolucion,
          ordenVisual: this.nextOrdenVisual(this.soluciones)
        })
      );
      this.newSolucion = {
        titulo: '',
        texto: '',
        emoji: '',
        ordenVisual: 0
      };
      await this.cargarSoluciones();
      this.statusMessage.set('Solucion creada.');
    } catch (error) {
      console.error(error);
      this.errorMessage.set('No se pudo crear la solucion.');
    }
  }

  empezarEdicionSolucion(solucion: Solucion): void {
    if (!this.canModificarSoluciones()) {
      this.errorMessage.set('No tienes permiso para modificar soluciones.');
      return;
    }

    this.editSolucionId = solucion.id;
    this.editSolucionActual = solucion;
    this.editSolucion = {
      titulo: solucion.titulo,
      texto: solucion.texto,
      emoji: solucion.emoji,
      ordenVisual: solucion.ordenVisual
    };
    this.setBodyScrollLocked(true);
  }

  cancelarEdicionSolucion(): void {
    this.editSolucionId = null;
    this.editSolucionActual = null;
    this.editSolucion = {};
    this.setBodyScrollLocked(false);
  }

  async guardarEdicionSolucion(solucionId: number): Promise<void> {
    try {
      await firstValueFrom(this.api.updateSolucion(solucionId, this.editSolucion));
      this.cancelarEdicionSolucion();
      await this.cargarSoluciones();
      this.statusMessage.set('Solucion actualizada.');
    } catch (error) {
      console.error(error);
      this.errorMessage.set('No se pudo actualizar la solucion.');
    }
  }

  async eliminarSolucion(solucionId: number): Promise<void> {
    if (!this.canEliminarSoluciones()) {
      this.errorMessage.set('No tienes permiso para eliminar soluciones.');
      return;
    }

    if (!confirm('¿Eliminar solucion?')) {
      return;
    }
    try {
      await firstValueFrom(this.api.deleteSolucion(solucionId));
      await this.cargarSoluciones();
      if (this.editSolucionId === solucionId) {
        this.cancelarEdicionSolucion();
      }
      this.statusMessage.set('Solucion eliminada.');
    } catch (error) {
      console.error(error);
      this.errorMessage.set('No se pudo eliminar la solucion.');
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
      const archivos = await firstValueFrom(
        this.api.getProductoArchivos(productoId, true)
      );

      if (this.productoArchivosActualId !== productoId) {
        return;
      }

      this.productoArchivosActual = archivos;
      this.statusMessage.set(`Archivos cargados (${this.productoArchivosActual.length}).`);
    } catch (error) {
      console.error(error);
      this.errorMessage.set('No se pudieron cargar archivos del producto.');

      if (this.productoArchivosActualId === productoId) {
        this.productoArchivosActual = [];
      }
    } finally {
      if (this.productoArchivosActualId === productoId) {
        this.uploading.set(false);
      }
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

  onEditProductoImagenSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.selectedEditProductoImagen = input.files?.[0] ?? null;
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
      this.selectedUploadFile = null;
      if (this.productoArchivoInput) {
        this.productoArchivoInput.nativeElement.value = '';
      }
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

  categoriaNombre(categoriaId: number): string {
    return this.categorias.find((categoria) => categoria.id === categoriaId)?.nombre ?? 'Sin categoria';
  }

  categoriaOrden(categoriaId: number): number {
    return this.categorias.find((categoria) => categoria.id === categoriaId)?.ordenVisual ?? Number.MAX_SAFE_INTEGER;
  }

  marcaNombre(marcaId: number): string {
    return this.marcas.find((marca) => marca.id === marcaId)?.nombre ?? 'Sin marca';
  }

  private abrirCategoriasConProductos(): void {
    this.expandedCategoriaIds.set(new Set(this.productos.map((producto) => producto.categoriaId || 0)));
  }

  private async commitReorder(kind: SortableList, insertIndex: number): Promise<void> {
    if (!this.canDropOnList(kind) || this.draggedIndex < 0) {
      this.clearDragState();
      return;
    }

    const fromIndex = this.draggedIndex;
    const currentItems = this.getSortableItems(kind);
    const reorderedItems = this.moveItem(currentItems, fromIndex, insertIndex);

    if (reorderedItems === currentItems) {
      this.clearDragState();
      return;
    }

    const normalizedItems = reorderedItems.map((item, index) => ({
      ...item,
      ordenVisual: index + 1
    }));
    const changedItems = normalizedItems.filter((item) => {
      const previous = currentItems.find((currentItem) => currentItem.id === item.id);
      return previous?.ordenVisual !== item.ordenVisual;
    });

    this.setSortableItems(kind, normalizedItems);
    this.clearDragState();
    this.playSettleAnimation(kind);

    try {
      if (kind === 'categorias') {
        await Promise.all(changedItems.map((categoria) =>
          firstValueFrom(this.api.updateCategoria(categoria.id, { ordenVisual: categoria.ordenVisual }))
        ));
        this.statusMessage.set('Orden de categorias actualizado.');
      } else if (kind === 'marcas') {
        await Promise.all(changedItems.map((marca) =>
          firstValueFrom(this.api.updateMarca(marca.id, { ordenVisual: marca.ordenVisual }))
        ));
        this.statusMessage.set('Orden de marcas actualizado.');
      } else {
        await Promise.all(changedItems.map((solucion) =>
          firstValueFrom(this.api.updateSolucion(solucion.id, { ordenVisual: solucion.ordenVisual }))
        ));
        this.statusMessage.set('Orden de soluciones actualizado.');
      }
    } catch (error) {
      console.error(error);
      const errorMessage = kind === 'categorias'
        ? 'No se pudo guardar el orden de categorias.'
        : kind === 'marcas'
          ? 'No se pudo guardar el orden de marcas.'
          : 'No se pudo guardar el orden de soluciones.';
      this.errorMessage.set(errorMessage);

      if (kind === 'categorias') {
        await this.cargarCategorias();
      } else if (kind === 'marcas') {
        await this.cargarMarcas();
      } else {
        await this.cargarSoluciones();
      }
    }
  }

  private moveItem<T extends { ordenVisual: number }>(items: T[], fromIndex: number, insertIndex: number): T[] {
    const boundedInsertIndex = Math.max(0, Math.min(insertIndex, items.length));
    const finalIndex = fromIndex < boundedInsertIndex ? boundedInsertIndex - 1 : boundedInsertIndex;

    if (finalIndex === fromIndex || fromIndex < 0 || fromIndex >= items.length) {
      return items;
    }

    const copy = [...items];
    const [movedItem] = copy.splice(fromIndex, 1);
    copy.splice(finalIndex, 0, movedItem);
    return copy;
  }

  private getItemDropIndex(index: number, event: DragEvent): number {
    const target = event.currentTarget as HTMLElement;
    const bounds = target.getBoundingClientRect();
    return event.clientY > bounds.top + bounds.height / 2 ? index + 1 : index;
  }

  private canDropOnList(kind: SortableList): boolean {
    const canModify = kind === 'categorias'
      ? this.canModificarCategorias()
      : kind === 'marcas'
        ? this.canModificarMarcas()
        : this.canModificarSoluciones();

    return canModify && this.draggedList === kind && this.draggedIndex >= 0;
  }

  private getSortableItems(kind: SortableList): Array<Categoria | Marca | Solucion> {
    if (kind === 'categorias') {
      return this.categorias;
    }

    return kind === 'marcas' ? this.marcas : this.soluciones;
  }

  private setSortableItems(kind: SortableList, items: Array<Categoria | Marca | Solucion>): void {
    if (kind === 'categorias') {
      this.categorias = items as Categoria[];
      return;
    }

    if (kind === 'marcas') {
      this.marcas = items as Marca[];
      return;
    }

    this.soluciones = items as Solucion[];
  }

  private isEditingSortableItem(kind: SortableList, index: number): boolean {
    const item = this.getSortableItems(kind)[index];
    if (!item) {
      return true;
    }

    if (kind === 'categorias') {
      return this.editCategoriaId === item.id;
    }

    return kind === 'marcas'
      ? this.editMarcaId === item.id
      : this.editSolucionId === item.id;
  }

  private clearDragState(): void {
    this.draggedList = null;
    this.draggedIndex = -1;
    this.dropIndex = -1;
  }

  private playSettleAnimation(kind: SortableList): void {
    this.settlingList.set(kind);
    window.setTimeout(() => {
      if (this.settlingList() === kind) {
        this.settlingList.set(null);
      }
    }, 260);
  }

  private nextOrdenVisual(items: Array<{ ordenVisual: number }>): number {
    return items.reduce((max, item) => Math.max(max, item.ordenVisual ?? 0), 0) + 1;
  }

  private hasNonNegativeNumber(value: unknown): boolean {
    if (value === null || value === undefined || String(value).trim() === '') {
      return false;
    }

    const parsed = Number(value);
    return Number.isFinite(parsed) && parsed >= 0;
  }

  private defaultPermisosCatalogo(): PermisosCatalogo {
    return {
      productos: {
        crear: true,
        modificar: true,
        eliminar: true
      },
      categorias: {
        crear: true,
        modificar: true,
        eliminar: true
      },
      marcas: {
        crear: true,
        modificar: true,
        eliminar: true
      },
      soluciones: {
        crear: true,
        modificar: true,
        eliminar: true
      },
      fechaActualizacion: ''
    };
  }

  private normalizePermisosCatalogo(permisos: PermisosCatalogo): PermisosCatalogo {
    const permisosPorDefecto = this.defaultPermisosCatalogo();

    return {
      productos: {
        ...permisosPorDefecto.productos,
        ...permisos.productos
      },
      categorias: {
        ...permisosPorDefecto.categorias,
        ...permisos.categorias
      },
      marcas: {
        ...permisosPorDefecto.marcas,
        ...permisos.marcas
      },
      soluciones: {
        ...permisosPorDefecto.soluciones,
        ...permisos.soluciones
      },
      fechaActualizacion: permisos.fechaActualizacion ?? permisosPorDefecto.fechaActualizacion
    };
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

  private setBodyScrollLocked(isLocked: boolean): void {
    this.document.body.classList.toggle('modal-open', isLocked);
  }
}
