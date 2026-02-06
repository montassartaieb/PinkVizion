import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterLink, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
    selector: 'app-login',
    standalone: true,
    imports: [CommonModule, ReactiveFormsModule, RouterLink],
    template: `
    <div class="auth-container">
      <div class="auth-card glass fade-in">
        <div class="auth-header">
          <div class="logo">
            <svg width="48" height="48" viewBox="0 0 48 48" fill="none">
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
          </div>
          <h1>PinkVision</h1>
          <p class="subtitle">Plateforme d'aide au diagnostic du cancer du sein</p>
        </div>

        <form [formGroup]="loginForm" (ngSubmit)="onSubmit()">
          <div class="form-group">
            <label class="form-label">Email</label>
            <input 
              type="email" 
              class="form-input" 
              formControlName="email"
              [class.error]="submitted && f['email'].errors"
              placeholder="votre@email.com">
            <div *ngIf="submitted && f['email'].errors" class="form-error">
              <span *ngIf="f['email'].errors['required']">L'email est requis</span>
              <span *ngIf="f['email'].errors['email']">Format d'email invalide</span>
            </div>
          </div>

          <div class="form-group">
            <label class="form-label">Mot de passe</label>
            <input 
              type="password" 
              class="form-input" 
              formControlName="password"
              [class.error]="submitted && f['password'].errors"
              placeholder="••••••••">
            <div *ngIf="submitted && f['password'].errors" class="form-error">
              <span *ngIf="f['password'].errors['required']">Le mot de passe est requis</span>
            </div>
          </div>

          <div *ngIf="errorMessage" class="alert alert-error">
            {{ errorMessage }}
          </div>

          <button type="submit" class="btn btn-primary btn-lg w-full" [disabled]="loading">
            <span *ngIf="loading" class="spinner-small"></span>
            <span *ngIf="!loading">Se connecter</span>
          </button>
        </form>

        <div class="auth-footer">
          <p>Pas encore de compte? <a routerLink="/register">S'inscrire</a></p>
        </div>
      </div>

      <div class="auth-info">
        <div class="info-content fade-in">
          <h2>Bienvenue sur PinkVision</h2>
          <p>Plateforme intelligente d'aide au diagnostic utilisant l'IA pour l'analyse de mammographies.</p>
          
          <div class="features">
            <div class="feature">
              <span class="feature-icon">🔬</span>
              <span>Analyse IA avancée</span>
            </div>
            <div class="feature">
              <span class="feature-icon">🔒</span>
              <span>Données sécurisées</span>
            </div>
            <div class="feature">
              <span class="feature-icon">👩‍⚕️</span>
              <span>Validation médicale</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
    styles: [`
    .auth-container {
      display: flex;
      min-height: 100vh;
    }

    .auth-card {
      flex: 1;
      max-width: 480px;
      margin: auto;
      padding: 3rem;
      border-radius: var(--border-radius-lg);
    }

    .auth-header {
      text-align: center;
      margin-bottom: 2rem;
    }

    .logo {
      margin-bottom: 1rem;
    }

    .auth-header h1 {
      font-size: 1.75rem;
      background: linear-gradient(135deg, var(--primary-600), var(--primary-400));
      -webkit-background-clip: text;
      -webkit-text-fill-color: transparent;
      background-clip: text;
      margin-bottom: 0.5rem;
    }

    .subtitle {
      color: var(--gray-500);
      font-size: 0.875rem;
    }

    .w-full {
      width: 100%;
    }

    .spinner-small {
      width: 20px;
      height: 20px;
      border: 2px solid rgba(255,255,255,0.3);
      border-top-color: white;
      border-radius: 50%;
      animation: spin 1s linear infinite;
    }

    .auth-footer {
      margin-top: 1.5rem;
      text-align: center;
      color: var(--gray-500);
      font-size: 0.875rem;
    }

    .auth-info {
      display: none;
      flex: 1;
      background: linear-gradient(135deg, var(--primary-600) 0%, var(--primary-800) 100%);
      padding: 3rem;
      color: white;
    }

    @media (min-width: 1024px) {
      .auth-card {
        margin-right: 0;
        border-radius: var(--border-radius-lg) 0 0 var(--border-radius-lg);
      }

      .auth-info {
        display: flex;
        align-items: center;
        border-radius: 0 var(--border-radius-lg) var(--border-radius-lg) 0;
      }
    }

    .info-content {
      max-width: 400px;
    }

    .info-content h2 {
      font-size: 2rem;
      color: white;
      margin-bottom: 1rem;
    }

    .info-content p {
      opacity: 0.9;
      margin-bottom: 2rem;
      line-height: 1.7;
    }

    .features {
      display: flex;
      flex-direction: column;
      gap: 1rem;
    }

    .feature {
      display: flex;
      align-items: center;
      gap: 1rem;
      padding: 1rem;
      background: rgba(255,255,255,0.1);
      border-radius: var(--border-radius-sm);
      backdrop-filter: blur(5px);
    }

    .feature-icon {
      font-size: 1.5rem;
    }
  `]
})
export class LoginComponent {
    loginForm: FormGroup;
    loading = false;
    submitted = false;
    errorMessage = '';

    constructor(
        private fb: FormBuilder,
        private authService: AuthService,
        private router: Router,
        private route: ActivatedRoute
    ) {
        this.loginForm = this.fb.group({
            email: ['', [Validators.required, Validators.email]],
            password: ['', Validators.required]
        });
    }

    get f() {
        return this.loginForm.controls;
    }

    onSubmit(): void {
        this.submitted = true;
        this.errorMessage = '';

        if (this.loginForm.invalid) {
            return;
        }

        this.loading = true;

        this.authService.login(this.loginForm.value).subscribe({
            next: (response) => {
                if (response.success) {
                    const returnUrl = this.route.snapshot.queryParams['returnUrl'] || this.getDefaultRoute();
                    this.router.navigate([returnUrl]);
                } else {
                    this.errorMessage = response.message || 'Erreur de connexion';
                    this.loading = false;
                }
            },
            error: (error) => {
                this.errorMessage = error.error?.message || 'Email ou mot de passe incorrect';
                this.loading = false;
            }
        });
    }

    private getDefaultRoute(): string {
        if (this.authService.isAdmin()) return '/admin/dashboard';
        if (this.authService.isDoctor()) return '/doctor/dashboard';
        return '/patient/dashboard';
    }
}
