import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
    selector: 'app-admin-dashboard',
    standalone: true,
    imports: [CommonModule, RouterLink],
    template: `
    <div class="dashboard">
      <aside class="sidebar">
        <div class="sidebar-header">
          <div class="logo">
            <svg width="32" height="32" viewBox="0 0 48 48" fill="none">
              <circle cx="24" cy="24" r="24" fill="url(#gradient)"/>
              <path d="M24 12C17.37 12 12 17.37 12 24C12 30.63 17.37 36 24 36C30.63 36 36 30.63 36 24C36 17.37 30.63 12 24 12Z" fill="white"/>
              <circle cx="24" cy="24" r="6" fill="white"/>
              <defs>
                <linearGradient id="gradient" x1="0" y1="0" x2="48" y2="48">
                  <stop stop-color="#ec4899"/>
                  <stop offset="1" stop-color="#db2777"/>
                </linearGradient>
              </defs>
            </svg>
            <span>PinkVision</span>
          </div>
        </div>

        <nav class="sidebar-nav">
          <a routerLink="/admin/dashboard" class="nav-item active">
            <span class="nav-icon">📊</span>
            <span>Dashboard</span>
          </a>
          <a routerLink="/admin/users" class="nav-item">
            <span class="nav-icon">👥</span>
            <span>Utilisateurs</span>
          </a>
        </nav>

        <div class="sidebar-footer">
          <button class="btn btn-secondary w-full" (click)="logout()">
            Déconnexion
          </button>
        </div>
      </aside>

      <main class="main-content">
        <header class="page-header">
          <h1>Administration</h1>
          <span class="badge badge-error">ADMIN</span>
        </header>

        <div class="dashboard-grid fade-in">
          <div class="card">
            <h3>🏥 Vue d'ensemble</h3>
            <p class="text-muted">Panneau d'administration PinkVision</p>
          </div>
        </div>
      </main>
    </div>
  `,
    styles: [`
    .dashboard {
      display: flex;
      min-height: 100vh;
    }

    .sidebar {
      width: 260px;
      background: white;
      border-right: 1px solid var(--gray-200);
      display: flex;
      flex-direction: column;
      position: fixed;
      height: 100vh;
    }

    .sidebar-header {
      padding: 1.5rem;
      border-bottom: 1px solid var(--gray-200);
    }

    .logo {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      font-weight: 600;
    }

    .sidebar-nav {
      flex: 1;
      padding: 1rem;
    }

    .nav-item {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      padding: 0.875rem 1rem;
      border-radius: var(--border-radius-sm);
      color: var(--gray-600);
      text-decoration: none;
      margin-bottom: 0.25rem;

      &:hover { background: var(--gray-100); color: var(--gray-900); }
      &.active { background: var(--primary-50); color: var(--primary-600); font-weight: 500; }
    }

    .sidebar-footer { padding: 1rem; border-top: 1px solid var(--gray-200); }
    .w-full { width: 100%; }

    .main-content {
      flex: 1;
      margin-left: 260px;
      padding: 2rem;
    }

    .page-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 2rem;
    }

    .dashboard-grid {
      display: grid;
      gap: 1.5rem;
    }
  `]
})
export class AdminDashboardComponent {
    constructor(private authService: AuthService) { }

    logout(): void {
        this.authService.logout();
    }
}
