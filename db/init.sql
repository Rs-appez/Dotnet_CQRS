create table if not exists film (
  id              SERIAL,
  fr_name            VARCHAR(100) NOT NULL,
  qc_name            VARCHAR(100) NOT NULL,
  year            INTEGER NOT NULL
);

insert into film (fr_name, qc_name, year) values
('Cars', 'Les Bagnoles', 2006),
('Delire Express', 'Ananas Express', 2008),
('The Dark Knight', 'Le Chevalier Noir', 2008);
