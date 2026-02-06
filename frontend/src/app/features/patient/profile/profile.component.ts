import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
    selector: 'app-patient-profile',
    standalone: true,
    imports: [CommonModule, RouterLink],
    template: `
    <div class="page-container">
      <a routerLink="/patient/dashboard" class="back-link">← Retour au tableau de bord</a>
      <h1>Mon Profil</h1>
      <div class="card">
        <p>Formulaire de profil patient à implémenter</p>
      </div>
    </div>
  `,
    styles: [`
    .page-container { padding: 2rem; max-width: 800px; margin: 0 auto; }
    .back-link { color: var(--primary-600); margin-bottom: 1rem; display: inline-block; }
    h1 { margin-bottom: 1.5rem; }
  `]
})
export class PatientProfileComponent { }
