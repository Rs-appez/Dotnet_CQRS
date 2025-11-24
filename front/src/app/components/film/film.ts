import { Component, inject } from '@angular/core';
import { FilmService } from '../../services/film-service';
import { CreateFilm } from "../create-film/create-film";

@Component({
  selector: 'app-film',
  imports: [CreateFilm],
  templateUrl: './film.html',
  styleUrl: './film.css',
})
export class Film {
  private filmService = inject(FilmService);

  films$ = this.filmService.films$;

  ngOnInit(): void {
    this.filmService.getFilms();
  }
}
