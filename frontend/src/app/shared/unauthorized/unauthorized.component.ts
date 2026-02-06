import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
    selector: 'app-unauthorized',
    standalone: true,
    imports: [CommonModule, RouterLink],
    template: `
    <div class="error-page">
      <div class="error-content fade-in">
        <div class="error-icon">🚫</div>
        <h1>403</h1>
        <h2>Accès refusé</h2>
        <p>Vous n'avez pas les autorisations nécessaires pour accéder à cette page.</p>
        <a routerLink="/" class="btn btn-primary">Retour à l'accueil</a>
      </div>
    </div>
  `,
    styles: [`
    .error-page {
      display: flex;
      align-items: center;
      justify-content: center;
      min-height: 100vh;
      text-align: center;
      padding: 2rem;
    }

    .error-content {
      max-width: 400px;
    }

    .error-icon {
      font-size: 4rem;
      margin-bottom: 1rem;
    }

    h1 {
      font-size: 5rem;
      background: linear-gradient(135deg, var(--error), #b91c1c);
      -webkit-background-clip: text;
      -webkit-text-fill-color: transparent;
      margin-bottom: 0.5rem;
    }

    h2 {
      font-size: 1.5rem;
      color: var(--gray-700);
      margin-bottom: 1rem;
    }

    p {
      color: var(--gray-500);
      margin-bottom: 2rem;
    }
  `]
})
export class UnauthorizedComponent { }
