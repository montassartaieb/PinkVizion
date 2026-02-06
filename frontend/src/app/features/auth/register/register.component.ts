import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
    selector: 'app-register',
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
          <h1>Créer un compte</h1>
          <p class="subtitle">Rejoignez la plateforme PinkVision</p>
        </div>

        <form [formGroup]="registerForm" (ngSubmit)="onSubmit()">
          <div class="form-row">
            <div class="form-group">
              <label class="form-label">Prénom</label>
              <input 
                type="text" 
                class="form-input" 
                formControlName="firstName"
                [class.error]="submitted && f['firstName'].errors"
                placeholder="Jean">
              <div *ngIf="submitted && f['firstName'].errors" class="form-error">
                <span *ngIf="f['firstName'].errors['required']">Prénom requis</span>
              </div>
            </div>

            <div class="form-group">
              <label class="form-label">Nom</label>
              <input 
                type="text" 
                class="form-input" 
                formControlName="lastName"
                [class.error]="submitted && f['lastName'].errors"
                placeholder="Dupont">
              <div *ngIf="submitted && f['lastName'].errors" class="form-error">
                <span *ngIf="f['lastName'].errors['required']">Nom requis</span>
              </div>
            </div>
          </div>

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
            <label class="form-label">Téléphone</label>
            <input 
              type="tel" 
              class="form-input" 
              formControlName="phone"
              placeholder="+33 6 12 34 56 78">
          </div>

          <div class="form-group">
            <label class="form-label">Type de compte</label>
            <select class="form-select" formControlName="userType">
              <option value="PATIENT">Patient</option>
              <option value="MEDECIN">Médecin</option>
            </select>
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
              <span *ngIf="f['password'].errors['minlength']">Minimum 8 caractères</span>
            </div>
          </div>

          <div class="form-group">
            <label class="form-label">Confirmer le mot de passe</label>
            <input 
              type="password" 
              class="form-input" 
              formControlName="confirmPassword"
              [class.error]="submitted && f['confirmPassword'].errors"
              placeholder="••••••••">
            <div *ngIf="submitted && f['confirmPassword'].errors" class="form-error">
              <span *ngIf="f['confirmPassword'].errors['required']">Confirmation requise</span>
            </div>
            <div *ngIf="submitted && passwordMismatch" class="form-error">
              Les mots de passe ne correspondent pas
            </div>
          </div>

          <div *ngIf="errorMessage" class="alert alert-error">
            {{ errorMessage }}
          </div>

          <button type="submit" class="btn btn-primary btn-lg w-full" [disabled]="loading">
            <span *ngIf="loading" class="spinner-small"></span>
            <span *ngIf="!loading">Créer mon compte</span>
          </button>
        </form>

        <div class="auth-footer">
          <p>Déjà inscrit? <a routerLink="/login">Se connecter</a></p>
        </div>
      </div>
    </div>
  `,
    styles: [`
    .auth-container {
      display: flex;
      min-height: 100vh;
      align-items: center;
      justify-content: center;
      padding: 2rem;
    }

    .auth-card {
      width: 100%;
      max-width: 520px;
      padding: 2.5rem;
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
      font-size: 1.5rem;
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

    .form-row {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 1rem;
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

    @media (max-width: 480px) {
      .form-row {
        grid-template-columns: 1fr;
      }
    }
  `]
})
export class RegisterComponent {
    registerForm: FormGroup;
    loading = false;
    submitted = false;
    errorMessage = '';

    constructor(
        private fb: FormBuilder,
        private authService: AuthService,
        private router: Router
    ) {
        this.registerForm = this.fb.group({
            firstName: ['', Validators.required],
            lastName: ['', Validators.required],
            email: ['', [Validators.required, Validators.email]],
            phone: [''],
            userType: ['PATIENT', Validators.required],
            password: ['', [Validators.required, Validators.minLength(8)]],
            confirmPassword: ['', Validators.required]
        });
    }

    get f() {
        return this.registerForm.controls;
    }

    get passwordMismatch(): boolean {
        return this.registerForm.value.password !== this.registerForm.value.confirmPassword;
    }

    onSubmit(): void {
        this.submitted = true;
        this.errorMessage = '';

        if (this.registerForm.invalid || this.passwordMismatch) {
            return;
        }

        this.loading = true;

        this.authService.register(this.registerForm.value).subscribe({
            next: (response) => {
                if (response.success) {
                    const route = response.user.roles.includes('MEDECIN')
                        ? '/doctor/dashboard'
                        : '/patient/dashboard';
                    this.router.navigate([route]);
                } else {
                    this.errorMessage = response.message || 'Erreur lors de l\'inscription';
                    this.loading = false;
                }
            },
            error: (error) => {
                this.errorMessage = error.error?.message || 'Une erreur est survenue';
                this.loading = false;
            }
        });
    }
}
