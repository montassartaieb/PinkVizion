import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
    selector: 'app-doctor-imaging',
    standalone: true,
    imports: [CommonModule, RouterLink],
    template: `
    <div class="page-container">
      <a routerLink="/doctor/dashboard" class="back-link">← Retour</a>
      <h1>Imagerie</h1>
      <div class="card">
        <p>Upload et analyse d'images à implémenter</p>
      </div>
    </div>
  `,
    styles: [`
    .page-container { padding: 2rem; max-width: 1200px; margin: 0 auto; }
    .back-link { color: var(--primary-600); margin-bottom: 1rem; display: inline-block; }
    h1 { margin-bottom: 1.5rem; }
  `]
})
export class DoctorImagingComponent { }
