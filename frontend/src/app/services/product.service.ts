import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { Product, ProductPage } from '../models/product.model';

@Injectable({
  providedIn: 'root'
})
export class ProductService {
  private readonly API_URL = 'http://localhost:8080/api/products';

  constructor(private http: HttpClient) {}

  getAllProducts(page: number = 0, size: number = 10): Observable<ProductPage> {
    const params = new HttpParams()
      .set('page', page.toString())
      .set('size', size.toString());
    
    return this.http.get<ProductPage>(this.API_URL, { params });
  }

  getAllProductsList(): Observable<Product[]> {
    return this.http.get<Product[]>(`${this.API_URL}/all`);
  }

  getProducts(): Observable<Product[]> {
    console.log('ProductService: Making API call to', this.API_URL);
    return this.http.get<ProductPage>(this.API_URL).pipe(
      map(response => {
        console.log('ProductService: Received response', response);
        return response.content;
      })
    );
  }

  getProductById(id: number): Observable<Product> {
    return this.http.get<Product>(`${this.API_URL}/${id}`);
  }

  getProduct(id: number): Observable<Product> {
    return this.http.get<Product>(`${this.API_URL}/${id}`);
  }

  getProductsByCategory(category: string): Observable<Product[]> {
    return this.http.get<Product[]>(`${this.API_URL}/category/${category}`);
  }

  searchProducts(name: string): Observable<Product[]> {
    const params = new HttpParams().set('name', name);
    return this.http.get<Product[]>(`${this.API_URL}/search`, { params });
  }

  getAvailableProducts(): Observable<Product[]> {
    return this.http.get<Product[]>(`${this.API_URL}/available`);
  }

  getCategories(): Observable<string[]> {
    return this.http.get<string[]>(`${this.API_URL}/categories`);
  }

  createProduct(product: Product): Observable<Product> {
    return this.http.post<Product>(this.API_URL, product);
  }

  updateProduct(id: number, product: Product): Observable<Product> {
    return this.http.put<Product>(`${this.API_URL}/${id}`, product);
  }

  deleteProduct(id: number): Observable<void> {
    return this.http.delete<void>(`${this.API_URL}/${id}`);
  }

  reserveStock(id: number, quantity: number): Observable<string> {
    const params = new HttpParams().set('quantity', quantity.toString());
    return this.http.post<string>(`${this.API_URL}/${id}/reserve`, null, { 
      params,
      responseType: 'text' as 'json'
    });
  }

  releaseStock(id: number, quantity: number): Observable<string> {
    const params = new HttpParams().set('quantity', quantity.toString());
    return this.http.post<string>(`${this.API_URL}/${id}/release`, null, { 
      params,
      responseType: 'text' as 'json'
    });
  }
}


