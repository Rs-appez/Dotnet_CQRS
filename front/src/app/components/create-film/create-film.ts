import { Component, inject } from '@angular/core';
import {
  FormBuilder,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
} from '@angular/forms';
import { FilmService } from '../../services/film-service';

@Component({
  selector: 'app-create-film',
  imports: [ReactiveFormsModule],
  templateUrl: './create-film.html',
  styleUrl: './create-film.css',
})
export class CreateFilm {
  private _filmService = inject(FilmService);
  private _formBuilder = inject(FormBuilder);

  filmForm: FormGroup = this._formBuilder.group({
    frTitle: new FormControl(''),
    qcTitle: new FormControl(''),
    releaseYear: new FormControl(''),
  });

  onSubmit() {
    const newFilm = this.filmForm.value;
    this._filmService.createFilm(newFilm).add(() => {
      this.filmForm.reset();
    });
  }
}
