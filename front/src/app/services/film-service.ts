import { inject, Injectable, signal } from '@angular/core';
import { Subscription } from 'rxjs/internal/Subscription';
import { Film } from '../models/film';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class FilmService {
  private readonly BASE_URL = environment.apiUrl;
  private readonly BASE_URL_READ = this.BASE_URL + '/read/film';
  private readonly BASE_URL_WRITE = this.BASE_URL + '/write/film';
  private _http = inject(HttpClient);

  private _films$ = signal<Film[]>([]);
  public readonly films$ = this._films$.asReadonly();

  getFilms(): Subscription {
    return this._http.get<Film[]>(`${this.BASE_URL_READ}/`).subscribe({
      next: (films) => {
        this._films$.set(films);
        console.log('Fetched films:', films);
      },
      error: (error) => {
        console.error('Error fetching films:', error);
      },
    });
  }

  createFilm(film: Film): Subscription {
    return this._http.post<Film>(`${this.BASE_URL_WRITE}/`, film).subscribe({
      next: (newFilm) => {
        this._films$.update((films) => [...films, newFilm]);
        console.log('Created film:', newFilm);
      },
      error: (error) => {
        console.error('Error creating film:', error);
      },
    });
  }
}
