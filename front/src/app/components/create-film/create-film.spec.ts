import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CreateFilm } from './create-film';

describe('CreateFilm', () => {
  let component: CreateFilm;
  let fixture: ComponentFixture<CreateFilm>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CreateFilm]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CreateFilm);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
