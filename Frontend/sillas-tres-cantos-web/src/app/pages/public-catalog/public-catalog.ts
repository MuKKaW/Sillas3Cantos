import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { BackendApiService } from '../../core/services/backend-api.service';
import { Categoria, Marca, Producto, ProductoArchivo } from '../../core/models/api.models';

interface LandingService {
  icon: string;
  title: string;
  text: string;
}

@Component({
  selector: 'app-public-catalog',
  imports: [CommonModule, FormsModule, CurrencyPipe, DatePipe],
  templateUrl: './public-catalog.html',
  styleUrl: './public-catalog.scss',
})
export class PublicCatalog implements OnInit {
  private readonly api = inject(BackendApiService);

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
  selectedProducto: Producto | null = null;
  selectedProductoArchivos: ProductoArchivo[] = [];

  filtroNombre = '';
  filtroCategoriaId = 0;
  filtroMarcaId = 0;
  orderAsc = true;

  readonly loadingCatalogo = signal(false);
  readonly loadingArchivos = signal(false);
  readonly statusMessage = signal('');
  readonly errorMessage = signal('');

  async ngOnInit(): Promise<void> {
    await this.cargarCatalogoInicial();
  }

  async cargarCatalogoInicial(): Promise<void> {
    this.loadingCatalogo.set(true);
    this.errorMessage.set('');
    this.statusMessage.set('');

    try {
      const [categorias, marcas, productos] = await Promise.all([
        firstValueFrom(this.api.getCategorias({ includeHidden: false, orderAsc: true })),
        firstValueFrom(this.api.getMarcas({ includeHidden: false, orderAsc: true })),
        firstValueFrom(this.api.getProductos({ includeHidden: false, orderAsc: this.orderAsc }))
      ]);

      this.categorias = categorias;
      this.marcas = marcas;
      this.productos = productos;
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
      this.selectedProducto = null;
      this.selectedProductoArchivos = [];
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

  async seleccionarProducto(producto: Producto): Promise<void> {
    this.errorMessage.set('');

    try {
      this.selectedProducto = await firstValueFrom(this.api.getProductoById(producto.id, false));
    } catch {
      this.selectedProducto = producto;
    }

    await this.cargarArchivosProducto(producto.id);
  }

  cerrarFichaProducto(): void {
    this.selectedProducto = null;
    this.selectedProductoArchivos = [];
    this.loadingArchivos.set(false);
  }

  async cargarArchivosProducto(productoId: number): Promise<void> {
    this.loadingArchivos.set(true);

    try {
      this.selectedProductoArchivos = await firstValueFrom(this.api.getProductoArchivos(productoId, false));
    } catch (error) {
      console.error(error);
      this.selectedProductoArchivos = [];
    } finally {
      this.loadingArchivos.set(false);
    }
  }

  async descargarArchivo(archivo: ProductoArchivo): Promise<void> {
    this.errorMessage.set('');

    try {
      const blob = await firstValueFrom(
        this.api.downloadProductoArchivo(archivo.productoId, archivo.id, false)
      );
      const objectUrl = URL.createObjectURL(blob);
      const anchor = document.createElement('a');
      anchor.href = objectUrl;
      anchor.download = archivo.nombreOriginal;
      anchor.click();
      URL.revokeObjectURL(objectUrl);
    } catch (error) {
      console.error(error);
      this.errorMessage.set('No hemos podido descargar el archivo seleccionado.');
    }
  }

  categoriaNombre(categoriaId: number): string {
    return this.categorias.find((categoria) => categoria.id === categoriaId)?.nombre ?? 'Sin categoría';
  }

  marcaNombre(marcaId: number): string {
    return this.marcas.find((marca) => marca.id === marcaId)?.nombre ?? 'Consultar marca';
  }

  bytesToMb(bytes: number): string {
    return `${(bytes / (1024 * 1024)).toFixed(2)} MB`;
  }
}
