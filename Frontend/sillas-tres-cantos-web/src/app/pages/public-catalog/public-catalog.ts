import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';
import { BackendApiService } from '../../core/services/backend-api.service';
import {
  BusquedaProductosExternos,
  Categoria,
  Marca,
  Producto,
  ProductoArchivo
} from '../../core/models/api.models';

@Component({
  selector: 'app-public-catalog',
  imports: [CommonModule, FormsModule, CurrencyPipe, DatePipe],
  templateUrl: './public-catalog.html',
  styleUrl: './public-catalog.scss',
})
export class PublicCatalog implements OnInit {
  private readonly api = inject(BackendApiService);
  private readonly authService = inject(AuthService);

  readonly isAuthenticated = this.authService.isAuthenticated;
  readonly includeHiddenAllowed = computed(() => this.isAuthenticated());

  categorias: Categoria[] = [];
  marcas: Marca[] = [];
  productos: Producto[] = [];
  selectedProducto: Producto | null = null;
  selectedProductoArchivos: ProductoArchivo[] = [];

  filtroNombre = '';
  filtroCategoriaId = 0;
  filtroMarcaId = 0;
  includeHidden = false;
  orderAsc = true;

  externosQuery = 'chair';
  externosLimit = 6;
  externosSkip = 0;
  externosResultado: BusquedaProductosExternos | null = null;

  readonly loadingCatalogo = signal(false);
  readonly loadingArchivos = signal(false);
  readonly loadingExternos = signal(false);
  readonly statusMessage = signal('');
  readonly errorMessage = signal('');

  async ngOnInit(): Promise<void> {
    await this.cargarCatalogoInicial();
    await this.buscarExternos();
  }

  async cargarCatalogoInicial(): Promise<void> {
    this.loadingCatalogo.set(true);
    this.errorMessage.set('');

    try {
      const [categorias, marcas, productos] = await Promise.all([
        firstValueFrom(this.api.getCategorias({ includeHidden: this.includeHidden, orderAsc: true })),
        firstValueFrom(this.api.getMarcas({ includeHidden: this.includeHidden, orderAsc: true })),
        firstValueFrom(this.api.getProductos({ includeHidden: this.includeHidden, orderAsc: this.orderAsc }))
      ]);

      this.categorias = categorias;
      this.marcas = marcas;
      this.productos = productos;
      this.statusMessage.set(`Catalogo cargado (${productos.length} productos).`);
    } catch (error) {
      console.error(error);
      this.errorMessage.set('No se pudo cargar el catalogo. Revisa que la API este levantada.');
    } finally {
      this.loadingCatalogo.set(false);
    }
  }

  async aplicarFiltros(): Promise<void> {
    this.loadingCatalogo.set(true);
    this.errorMessage.set('');

    try {
      this.productos = await firstValueFrom(
        this.api.getProductos({
          nombre: this.filtroNombre.trim(),
          categoriaId: this.filtroCategoriaId || undefined,
          marcaId: this.filtroMarcaId || undefined,
          includeHidden: this.includeHidden,
          orderAsc: this.orderAsc
        })
      );
      this.statusMessage.set(`Se encontraron ${this.productos.length} productos.`);
      this.selectedProducto = null;
      this.selectedProductoArchivos = [];
    } catch (error) {
      console.error(error);
      this.errorMessage.set('No se pudieron aplicar los filtros.');
    } finally {
      this.loadingCatalogo.set(false);
    }
  }

  async toggleIncludeHidden(): Promise<void> {
    if (!this.includeHiddenAllowed()) {
      this.includeHidden = false;
      return;
    }
    await this.cargarCatalogoInicial();
  }

  limpiarFiltros(): void {
    this.filtroNombre = '';
    this.filtroCategoriaId = 0;
    this.filtroMarcaId = 0;
    this.orderAsc = true;
    this.aplicarFiltros();
  }

  async seleccionarProducto(producto: Producto): Promise<void> {
    try {
      this.selectedProducto = await firstValueFrom(
        this.api.getProductoById(producto.id, this.includeHidden)
      );
    } catch {
      this.selectedProducto = producto;
    }
    await this.cargarArchivosProducto(producto.id);
  }

  async cargarArchivosProducto(productoId: number): Promise<void> {
    this.loadingArchivos.set(true);
    this.errorMessage.set('');

    try {
      this.selectedProductoArchivos = await firstValueFrom(
        this.api.getProductoArchivos(productoId, this.includeHidden)
      );
    } catch (error) {
      console.error(error);
      this.selectedProductoArchivos = [];
      this.errorMessage.set('No se pudieron cargar los archivos del producto seleccionado.');
    } finally {
      this.loadingArchivos.set(false);
    }
  }

  async descargarArchivo(archivo: ProductoArchivo): Promise<void> {
    try {
      const blob = await firstValueFrom(
        this.api.downloadProductoArchivo(archivo.productoId, archivo.id, this.includeHidden)
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

  async buscarExternos(): Promise<void> {
    if (this.externosQuery.trim().length < 2) {
      this.errorMessage.set('La busqueda externa requiere al menos 2 caracteres.');
      return;
    }

    this.loadingExternos.set(true);
    this.errorMessage.set('');

    try {
      this.externosResultado = await firstValueFrom(
        this.api.buscarProductosExternos(
          this.externosQuery.trim(),
          this.externosLimit,
          this.externosSkip
        )
      );
    } catch (error) {
      console.error(error);
      this.errorMessage.set('No se pudo consultar la integracion externa.');
    } finally {
      this.loadingExternos.set(false);
    }
  }

  categoriaNombre(categoriaId: number): string {
    return this.categorias.find((categoria) => categoria.id === categoriaId)?.nombre ?? 'Sin categoria';
  }

  marcaNombre(marcaId: number): string {
    return this.marcas.find((marca) => marca.id === marcaId)?.nombre ?? 'Sin marca';
  }

  bytesToMb(bytes: number): string {
    return `${(bytes / (1024 * 1024)).toFixed(2)} MB`;
  }
}
