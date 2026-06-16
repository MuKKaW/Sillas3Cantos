import { CommonModule, CurrencyPipe, DOCUMENT } from '@angular/common';
import { Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { BackendApiService } from '../../core/services/backend-api.service';
import { Categoria, ConfiguracionCatalogo, Marca, Producto } from '../../core/models/api.models';

interface LandingService {
  icon: string;
  title: string;
  text: string;
}

interface FiltroTab {
  id: number;
  nombre: string;
}

@Component({
  selector: 'app-public-catalog',
  imports: [CommonModule, FormsModule, CurrencyPipe],
  templateUrl: './public-catalog.html',
  styleUrl: './public-catalog.scss',
})
export class PublicCatalog implements OnInit, OnDestroy {
  private readonly api = inject(BackendApiService);
  private readonly document = inject(DOCUMENT);

  readonly services: LandingService[] = [
    {
      icon: '♿',
      title: 'Sillas de ruedas',
      text: 'Venta y alquiler para movilidad diaria o temporal.'
    },
    {
      icon: '▭',
      title: 'Camas articuladas',
      text: 'Descanso cómodo con soluciones geriátricas.'
    },
    {
      icon: '⚡',
      title: 'Scooters eléctricos',
      text: 'Autonomía sencilla para moverse cada día.'
    },
    {
      icon: '↗',
      title: 'Andadores',
      text: 'Apoyo estable, ligero y fácil de manejar.'
    },
    {
      icon: '⌁',
      title: 'Grúas de traslado',
      text: 'Ayuda segura para movilización en casa.'
    },
    {
      icon: '+',
      title: 'Ortesis',
      text: 'Soportes técnicos para articulaciones y cuidado.'
    },
    {
      icon: '□',
      title: 'Ayudas de baño',
      text: 'Seguridad y autonomía para el aseo diario.'
    },
    {
      icon: '✓',
      title: 'Plantillas y calzado',
      text: 'Adaptación y comodidad para pies delicados.'
    }
  ];

  categorias: Categoria[] = [];
  marcas: Marca[] = [];
  productos: Producto[] = [];
  productosReferencia: Producto[] = [];
  selectedProducto: Producto | null = null;

  usarFiltroTabs = false;
  mostrarPrecios = false;
  mostrarStock = false;
  filtroNombre = '';
  filtroCategoriaId = 0;
  filtroMarcaId = 0;
  orderAsc = true;

  readonly loadingCatalogo = signal(false);
  readonly statusMessage = signal('');
  readonly errorMessage = signal('');

  async ngOnInit(): Promise<void> {
    await this.cargarCatalogoInicial();
  }

  ngOnDestroy(): void {
    this.setBodyScrollLocked(false);
  }

  async cargarCatalogoInicial(): Promise<void> {
    this.loadingCatalogo.set(true);
    this.errorMessage.set('');
    this.statusMessage.set('');

    try {
      const configuracionPorDefecto: ConfiguracionCatalogo = {
        usarFiltroTabs: false,
        mostrarPrecios: false,
        mostrarStock: false,
        fechaActualizacion: ''
      };
      const [configuracion, categorias, marcas, productos] = await Promise.all([
        firstValueFrom(this.api.getConfiguracionCatalogo()).catch(() => configuracionPorDefecto),
        firstValueFrom(this.api.getCategorias({ includeHidden: false, orderAsc: true })),
        firstValueFrom(this.api.getMarcas({ includeHidden: false, orderAsc: true })),
        firstValueFrom(this.api.getProductos({ includeHidden: false, orderAsc: this.orderAsc }))
      ]);

      this.usarFiltroTabs = configuracion.usarFiltroTabs;
      this.mostrarPrecios = configuracion.mostrarPrecios;
      this.mostrarStock = configuracion.mostrarStock;
      this.categorias = categorias;
      this.marcas = marcas;
      this.productos = productos;
      this.productosReferencia = productos;
    } catch (error) {
      console.error(error);
      this.errorMessage.set('Ahora mismo no podemos mostrar el catálogo online. Puedes consultarnos disponibilidad por teléfono o en tienda.');
    } finally {
      this.loadingCatalogo.set(false);
    }
  }

  async aplicarFiltros(): Promise<void> {
    this.loadingCatalogo.set(true);
    this.errorMessage.set('');
    this.statusMessage.set('');

    try {
      this.productos = await firstValueFrom(
        this.api.getProductos({
          nombre: this.filtroNombre.trim(),
          categoriaId: this.filtroCategoriaId || undefined,
          marcaId: this.filtroMarcaId || undefined,
          includeHidden: false,
          orderAsc: this.orderAsc
        })
      );
      this.cerrarFichaProducto();
    } catch (error) {
      console.error(error);
      this.errorMessage.set('No hemos podido actualizar el catálogo. Inténtalo de nuevo en unos minutos.');
    } finally {
      this.loadingCatalogo.set(false);
    }
  }

  async limpiarFiltros(): Promise<void> {
    this.filtroNombre = '';
    this.filtroCategoriaId = 0;
    this.filtroMarcaId = 0;
    this.orderAsc = true;
    await this.aplicarFiltros();
  }

  async seleccionarCategoriaTab(categoriaId: number): Promise<void> {
    this.filtroCategoriaId = categoriaId;
    this.filtroMarcaId = 0;
    await this.aplicarFiltros();
  }

  async seleccionarMarcaTab(marcaId: number): Promise<void> {
    this.filtroMarcaId = marcaId;
    await this.aplicarFiltros();
  }

  categoriaTabs(): FiltroTab[] {
    return this.categorias.map((categoria) => ({
      id: categoria.id,
      nombre: categoria.nombre
    }));
  }

  marcaTabs(): FiltroTab[] {
    if (!this.filtroCategoriaId) {
      return [];
    }

    const productosCategoria = this.productosReferencia.filter((producto) =>
      producto.categoriaId === this.filtroCategoriaId
    );
    const marcaIdsCategoria = new Set(productosCategoria.map((producto) => producto.marcaId));

    return this.marcas
      .filter((marca) => marcaIdsCategoria.has(marca.id))
      .map((marca) => ({
        id: marca.id,
        nombre: marca.nombre
      }));
  }

  async seleccionarProducto(producto: Producto): Promise<void> {
    this.errorMessage.set('');
    const productoId = producto.id;
    this.selectedProducto = producto;
    this.setBodyScrollLocked(true);

    try {
      const detalleProducto = await firstValueFrom(this.api.getProductoById(productoId, false));
      if (this.selectedProducto?.id === productoId) {
        this.selectedProducto = detalleProducto;
      }
    } catch {
      if (this.selectedProducto?.id !== productoId) {
        return;
      }
    }

  }

  cerrarFichaProducto(): void {
    this.selectedProducto = null;
    this.setBodyScrollLocked(false);
  }

  categoriaNombre(categoriaId: number): string {
    return this.categorias.find((categoria) => categoria.id === categoriaId)?.nombre ?? 'Sin categoría';
  }

  marcaNombre(marcaId: number): string {
    return this.marcas.find((marca) => marca.id === marcaId)?.nombre ?? 'Consultar marca';
  }

  private setBodyScrollLocked(isLocked: boolean): void {
    this.document.body.classList.toggle('modal-open', isLocked);
  }
}
