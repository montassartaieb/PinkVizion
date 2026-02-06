import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { PatientService } from '../../../core/services/patient.service';
import { ImagingService } from '../../../core/services/imaging.service';
import { User, Patient, DiagnosisResult } from '../../../core/models';

@Component({
    selector: 'app-doctor-dashboard',
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
          <a routerLink="/doctor/dashboard" class="nav-item active">
            <span class="nav-icon">📊</span>
            <span>Tableau de bord</span>
          </a>
          <a routerLink="/doctor/patients" class="nav-item">
            <span class="nav-icon">👥</span>
            <span>Patients</span>
          </a>
          <a routerLink="/doctor/imaging" class="nav-item">
            <span class="nav-icon">🖼️</span>
            <span>Imagerie</span>
          </a>
          <a routerLink="/doctor/pending-validations" class="nav-item">
            <span class="nav-icon">⏳</span>
            <span>En attente</span>
            <span *ngIf="pendingValidations.length > 0" class="nav-badge">{{ pendingValidations.length }}</span>
          </a>
        </nav>

        <div class="sidebar-footer">
          <div class="user-info">
            <div class="user-avatar">👩‍⚕️</div>
            <div class="user-details">
              <span class="user-name">Dr. {{ user?.lastName }}</span>
              <span class="user-role">Médecin</span>
            </div>
          </div>
          <button class="btn btn-secondary w-full mt-3" (click)="logout()">
            Déconnexion
          </button>
        </div>
      </aside>

      <main class="main-content">
        <header class="page-header">
          <div>
            <h1>Tableau de bord Médecin</h1>
            <p class="text-muted">Bienvenue, Dr. {{ user?.firstName }} {{ user?.lastName }}</p>
          </div>
          <div class="header-actions">
            <div class="ai-status" [class.available]="aiStatus?.available">
              <span class="status-dot"></span>
              <span>IA {{ aiStatus?.available ? 'Disponible' : 'Indisponible' }}</span>
            </div>
          </div>
        </header>

        <div class="dashboard-grid fade-in">
          <!-- AI Status Card -->
          <div class="card ai-card">
            <div class="card-header">
              <h3 class="card-title">État du Service IA</h3>
            </div>
            <div class="ai-info">
              <div class="ai-row">
                <span>Modèle Image</span>
                <span class="badge" [ngClass]="aiStatus?.imageModelLoaded ? 'badge-success' : 'badge-error'">
                  {{ aiStatus?.imageModelLoaded ? 'Chargé' : 'Erreur' }}
                </span>
              </div>
              <div class="ai-row">
                <span>Modèle Tabulaire</span>
                <span class="badge" [ngClass]="aiStatus?.tabularModelLoaded ? 'badge-success' : 'badge-error'">
                  {{ aiStatus?.tabularModelLoaded ? 'Chargé' : 'Erreur' }}
                </span>
              </div>
            </div>
          </div>

          <!-- Stats -->
          <div class="card stat-card">
            <div class="stat-icon">👥</div>
            <div class="stat-content">
              <span class="stat-value">{{ patientCount }}</span>
              <span class="stat-label">Patients suivis</span>
            </div>
          </div>

          <div class="card stat-card warning">
            <div class="stat-icon">⏳</div>
            <div class="stat-content">
              <span class="stat-value">{{ pendingValidations.length }}</span>
              <span class="stat-label">Validations en attente</span>
            </div>
          </div>

          <!-- Pending Validations -->
          <div class="card pending-card">
            <div class="card-header">
              <h3 class="card-title">Diagnostics à valider</h3>
              <a routerLink="/doctor/pending-validations" class="btn btn-sm btn-secondary">Voir tout</a>
            </div>

            <div *ngIf="pendingValidations.length > 0" class="validation-list">
              <div *ngFor="let diagnosis of pendingValidations.slice(0, 5)" class="validation-item">
                <div class="validation-info">
                  <span class="validation-label">
                    {{ diagnosis.label }}
                  </span>
                  <span class="risk-badge" [ngClass]="diagnosis.riskLevel.toLowerCase()">
                    {{ getRiskLabel(diagnosis.riskLevel) }}
                  </span>
                </div>
                <div class="validation-meta">
                  <span>{{ diagnosis.probabilityPercent }}</span>
                  <span class="text-muted">{{ diagnosis.createdAt | date:'dd/MM HH:mm' }}</span>
                </div>
              </div>
            </div>

            <div *ngIf="pendingValidations.length === 0" class="empty-state">
              <p>✅ Aucun diagnostic en attente</p>
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
      width: 280px;
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
      position: relative;

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

    .nav-badge {
      position: absolute;
      right: 0.75rem;
      background: var(--error);
      color: white;
      font-size: 0.75rem;
      padding: 0.125rem 0.5rem;
      border-radius: 9999px;
      font-weight: 600;
    }

    .nav-icon {
      font-size: 1.25rem;
    }

    .sidebar-footer {
      padding: 1rem;
      border-top: 1px solid var(--gray-200);
    }

    .user-info {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      padding-bottom: 1rem;
    }

    .user-avatar {
      width: 40px;
      height: 40px;
      background: var(--primary-100);
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 1.25rem;
    }

    .user-details {
      display: flex;
      flex-direction: column;
    }

    .user-name {
      font-weight: 500;
      color: var(--gray-900);
    }

    .user-role {
      font-size: 0.75rem;
      color: var(--gray-500);
    }

    .main-content {
      flex: 1;
      margin-left: 280px;
      padding: 2rem;
    }

    .page-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 2rem;
    }

    .ai-status {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      padding: 0.5rem 1rem;
      background: var(--gray-100);
      border-radius: var(--border-radius-sm);
      font-size: 0.875rem;

      &.available {
        background: var(--success-light);
        color: var(--success);
        
        .status-dot {
          background: var(--success);
        }
      }
    }

    .status-dot {
      width: 8px;
      height: 8px;
      border-radius: 50%;
      background: var(--gray-400);
    }

    .dashboard-grid {
      display: grid;
      grid-template-columns: repeat(3, 1fr);
      gap: 1.5rem;
    }

    .ai-card {
      grid-column: span 1;
    }

    .ai-info {
      display: flex;
      flex-direction: column;
      gap: 0.75rem;
    }

    .ai-row {
      display: flex;
      justify-content: space-between;
      align-items: center;
    }

    .stat-card {
      display: flex;
      align-items: center;
      gap: 1rem;

      &.warning .stat-icon {
        background: var(--warning-light);
        color: var(--warning);
      }
    }

    .stat-icon {
      width: 56px;
      height: 56px;
      background: var(--primary-100);
      color: var(--primary-600);
      border-radius: var(--border-radius);
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 1.75rem;
    }

    .stat-content {
      display: flex;
      flex-direction: column;
    }

    .stat-value {
      font-size: 1.75rem;
      font-weight: 700;
      color: var(--gray-900);
    }

    .stat-label {
      font-size: 0.875rem;
      color: var(--gray-500);
    }

    .pending-card {
      grid-column: span 3;
    }

    .validation-list {
      display: flex;
      flex-direction: column;
      gap: 0.75rem;
    }

    .validation-item {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 1rem;
      background: var(--gray-50);
      border-radius: var(--border-radius-sm);
      border-left: 4px solid var(--primary-500);
    }

    .validation-info {
      display: flex;
      align-items: center;
      gap: 1rem;
    }

    .validation-label {
      font-weight: 600;
      text-transform: uppercase;
    }

    .validation-meta {
      display: flex;
      align-items: center;
      gap: 1rem;
    }

    .empty-state {
      text-align: center;
      padding: 2rem;
      color: var(--gray-500);
    }

    .w-full { width: 100%; }
    .mt-3 { margin-top: 0.75rem; }
  `]
})
export class DoctorDashboardComponent implements OnInit {
    user: User | null = null;
    patientCount = 0;
    pendingValidations: DiagnosisResult[] = [];
    aiStatus: any = null;

    constructor(
        private authService: AuthService,
        private patientService: PatientService,
        private imagingService: ImagingService
    ) { }

    ngOnInit(): void {
        this.user = this.authService.currentUser;
        this.loadData();
    }

    loadData(): void {
        // Load patients count
        this.patientService.getAllPatients(1, 1).subscribe({
            next: (response) => {
                if (response.success && response.data) {
                    this.patientCount = response.data.totalCount;
                }
            }
        });

        // Load pending validations
        this.imagingService.getPendingValidations().subscribe({
            next: (response) => {
                if (response.success && response.data) {
                    this.pendingValidations = response.data;
                }
            }
        });

        // Load AI status
        this.imagingService.getAIStatus().subscribe({
            next: (response) => {
                if (response.success && response.data) {
                    this.aiStatus = response.data;
                }
            }
        });
    }

    getRiskLabel(riskLevel: string): string {
        switch (riskLevel) {
            case 'HIGH': return 'Risque élevé';
            case 'MODERATE': return 'Risque modéré';
            case 'LOW': return 'Risque faible';
            case 'VERY_LOW': return 'Très faible';
            default: return riskLevel;
        }
    }

    logout(): void {
        this.authService.logout();
    }
}
