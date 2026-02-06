import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { PatientService } from '../../../core/services/patient.service';
import { ImagingService } from '../../../core/services/imaging.service';
import { User, Patient, MammographyImage } from '../../../core/models';

@Component({
    selector: 'app-patient-dashboard',
    standalone: true,
    imports: [CommonModule, RouterLink],
    template: `
    <div class="dashboard">
      <aside class="sidebar">
        <div class="sidebar-header">
          <div class="logo">
            <svg width="32" height="32" viewBox="0 0 48 48" fill="none">
              <circle cx="24" cy="24" r="24" fill="url(#gradient)"/>
              <path d="M24 12C17.37 12 12 17.37 12 24C12 30.63 17.37 36 24 36C30.63 36 36 30.63 36 24C36 17.37 30.63 12 24 12ZM24 32C19.58 32 16 28.42 16 24C16 19.58 19.58 16 24 16C28.42 16 32 19.58 32 24C32 28.42 28.42 32 24 32Z" fill="white"/>
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
          <a routerLink="/patient/dashboard" class="nav-item active">
            <span class="nav-icon">📊</span>
            <span>Tableau de bord</span>
          </a>
          <a routerLink="/patient/profile" class="nav-item">
            <span class="nav-icon">👤</span>
            <span>Mon profil</span>
          </a>
          <a routerLink="/patient/images" class="nav-item">
            <span class="nav-icon">🖼️</span>
            <span>Mes images</span>
          </a>
        </nav>

        <div class="sidebar-footer">
          <button class="btn btn-secondary w-full" (click)="logout()">
            <span>🚪</span>
            <span>Déconnexion</span>
          </button>
        </div>
      </aside>

      <main class="main-content">
        <header class="page-header">
          <div>
            <h1>Bienvenue, {{ user?.firstName || 'Patient' }} !</h1>
            <p class="text-muted">Voici votre tableau de bord santé</p>
          </div>
          <div class="header-actions">
            <span class="badge badge-primary">{{ user?.roles?.[0] }}</span>
          </div>
        </header>

        <div class="dashboard-grid fade-in">
          <!-- Stats cards -->
          <div class="card stat-card">
            <div class="stat-icon" style="background: var(--primary-100); color: var(--primary-600);">🖼️</div>
            <div class="stat-content">
              <span class="stat-value">{{ images.length }}</span>
              <span class="stat-label">Images</span>
            </div>
          </div>

          <div class="card stat-card">
            <div class="stat-icon" style="background: var(--success-light); color: var(--success);">✓</div>
            <div class="stat-content">
              <span class="stat-value">{{ analyzedCount }}</span>
              <span class="stat-label">Analysées</span>
            </div>
          </div>

          <div class="card stat-card">
            <div class="stat-icon" style="background: var(--warning-light); color: var(--warning);">⏳</div>
            <div class="stat-content">
              <span class="stat-value">{{ pendingCount }}</span>
              <span class="stat-label">En attente</span>
            </div>
          </div>

          <!-- Profile summary -->
          <div class="card profile-card">
            <div class="card-header">
              <h3 class="card-title">Mon profil</h3>
              <a routerLink="/patient/profile" class="btn btn-sm btn-secondary">Modifier</a>
            </div>
            
            <div *ngIf="patient" class="profile-info">
              <div class="info-row">
                <span class="info-label">Nom complet</span>
                <span class="info-value">{{ patient.firstName }} {{ patient.lastName }}</span>
              </div>
              <div class="info-row">
                <span class="info-label">Email</span>
                <span class="info-value">{{ patient.email }}</span>
              </div>
              <div class="info-row" *ngIf="patient.bloodGroup">
                <span class="info-label">Groupe sanguin</span>
                <span class="info-value badge badge-primary">{{ patient.bloodGroup.code }}</span>
              </div>
              <div class="info-row" *ngIf="patient.bmi">
                <span class="info-label">IMC</span>
                <span class="info-value">{{ patient.bmi | number:'1.1-1' }}</span>
              </div>
            </div>

            <div *ngIf="!patient" class="empty-state">
              <p>Complétez votre profil médical</p>
              <a routerLink="/patient/profile" class="btn btn-primary btn-sm">Compléter</a>
            </div>
          </div>

          <!-- Recent images -->
          <div class="card recent-card">
            <div class="card-header">
              <h3 class="card-title">Images récentes</h3>
              <a routerLink="/patient/images" class="btn btn-sm btn-secondary">Voir tout</a>
            </div>

            <div *ngIf="images.length > 0" class="image-list">
              <div *ngFor="let image of images.slice(0, 3)" class="image-item">
                <div class="image-info">
                  <span class="image-name">{{ image.originalFileName }}</span>
                  <span class="image-date">{{ image.uploadedAt | date:'dd/MM/yyyy' }}</span>
                </div>
                <div class="image-status">
                  <span class="badge" [ngClass]="{
                    'badge-success': image.status === 'ANALYZED',
                    'badge-warning': image.status === 'PENDING' || image.status === 'ANALYZING',
                    'badge-error': image.status === 'FAILED'
                  }">
                    {{ getStatusLabel(image.status) }}
                  </span>
                </div>
              </div>
            </div>

            <div *ngIf="images.length === 0" class="empty-state">
              <p>Aucune image pour le moment</p>
            </div>
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
      font-size: 1.125rem;
      color: var(--gray-900);
    }

    .sidebar-nav {
      flex: 1;
      padding: 1rem;
      display: flex;
      flex-direction: column;
      gap: 0.25rem;
    }

    .nav-item {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      padding: 0.875rem 1rem;
      border-radius: var(--border-radius-sm);
      color: var(--gray-600);
      transition: all var(--transition);
      text-decoration: none;

      &:hover {
        background: var(--gray-100);
        color: var(--gray-900);
      }

      &.active {
        background: var(--primary-50);
        color: var(--primary-600);
        font-weight: 500;
      }
    }

    .nav-icon {
      font-size: 1.25rem;
    }

    .sidebar-footer {
      padding: 1rem;
      border-top: 1px solid var(--gray-200);
    }

    .w-full {
      width: 100%;
    }

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

      h1 {
        font-size: 1.5rem;
        margin-bottom: 0.25rem;
      }
    }

    .dashboard-grid {
      display: grid;
      grid-template-columns: repeat(3, 1fr);
      gap: 1.5rem;
    }

    .stat-card {
      display: flex;
      align-items: center;
      gap: 1rem;
    }

    .stat-icon {
      width: 48px;
      height: 48px;
      border-radius: var(--border-radius-sm);
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 1.5rem;
    }

    .stat-content {
      display: flex;
      flex-direction: column;
    }

    .stat-value {
      font-size: 1.5rem;
      font-weight: 600;
      color: var(--gray-900);
    }

    .stat-label {
      font-size: 0.875rem;
      color: var(--gray-500);
    }

    .profile-card,
    .recent-card {
      grid-column: span 3;
    }

    @media (min-width: 1024px) {
      .profile-card {
        grid-column: span 1;
      }
      .recent-card {
        grid-column: span 2;
      }
    }

    .profile-info {
      display: flex;
      flex-direction: column;
      gap: 0.75rem;
    }

    .info-row {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 0.5rem 0;
      border-bottom: 1px solid var(--gray-100);

      &:last-child {
        border-bottom: none;
      }
    }

    .info-label {
      font-size: 0.875rem;
      color: var(--gray-500);
    }

    .info-value {
      font-weight: 500;
      color: var(--gray-900);
    }

    .image-list {
      display: flex;
      flex-direction: column;
      gap: 0.75rem;
    }

    .image-item {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 0.875rem;
      background: var(--gray-50);
      border-radius: var(--border-radius-sm);
    }

    .image-info {
      display: flex;
      flex-direction: column;
    }

    .image-name {
      font-weight: 500;
      color: var(--gray-900);
    }

    .image-date {
      font-size: 0.75rem;
      color: var(--gray-500);
    }

    .empty-state {
      text-align: center;
      padding: 2rem;
      color: var(--gray-500);
    }
  `]
})
export class PatientDashboardComponent implements OnInit {
    user: User | null = null;
    patient: Patient | null = null;
    images: MammographyImage[] = [];

    constructor(
        private authService: AuthService,
        private patientService: PatientService,
        private imagingService: ImagingService
    ) { }

    ngOnInit(): void {
        this.user = this.authService.currentUser;
        this.loadPatientProfile();
        this.loadImages();
    }

    loadPatientProfile(): void {
        this.patientService.getMyProfile().subscribe({
            next: (response) => {
                if (response.success && response.data) {
                    this.patient = response.data;
                    this.loadPatientImages();
                }
            },
            error: (error) => console.error('Error loading profile:', error)
        });
    }

    loadImages(): void {
        // Will be loaded after patient profile
    }

    loadPatientImages(): void {
        if (this.patient) {
            this.imagingService.getPatientImages(this.patient.id).subscribe({
                next: (response) => {
                    if (response.success && response.data) {
                        this.images = response.data;
                    }
                },
                error: (error) => console.error('Error loading images:', error)
            });
        }
    }

    get analyzedCount(): number {
        return this.images.filter(i => i.status === 'ANALYZED').length;
    }

    get pendingCount(): number {
        return this.images.filter(i => i.status === 'PENDING' || i.status === 'ANALYZING').length;
    }

    getStatusLabel(status: string): string {
        switch (status) {
            case 'ANALYZED': return 'Analysée';
            case 'PENDING': return 'En attente';
            case 'ANALYZING': return 'En cours';
            case 'FAILED': return 'Échec';
            default: return status;
        }
    }

    logout(): void {
        this.authService.logout();
    }
}
