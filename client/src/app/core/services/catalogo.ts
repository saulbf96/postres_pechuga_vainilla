import { HttpClient } from '@angular/common/http';
import { Service, inject } from '@angular/core';
import type { ProductoDto, VendedorDto } from '../models/catalogo.model';

// Service central para leer el catalogo. Un solo lugar que sabe la ruta de la API;
// el componente no conoce URLs, solo pide datos.
@Service()
export class CatalogoService {
  private readonly http = inject(HttpClient);

  obtenerVendedores() {
    return this.http.get<VendedorDto[]>('/api/v1/vendedores');
  }

  obtenerProductos() {
    return this.http.get<ProductoDto[]>('/api/v1/productos');
  }
}
