import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Film } from './components/film/film';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Film],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
  protected readonly title = signal('Best Films Ever (or not)');
}
