import { Routes } from '@angular/router';
import { AuthGuard, GuestGuard } from './core/guards/auth.guard';

export const routes: Routes = [
    // Public routes
    {
        path: '',
        redirectTo: 'login',
        pathMatch: 'full'
    },
    {
        path: 'login',
        loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent),
        canActivate: [GuestGuard]
    },
    {
        path: 'register',
        loadComponent: () => import('./features/auth/register/register.component').then(m => m.RegisterComponent),
        canActivate: [GuestGuard]
    },

    // Patient routes
    {
        path: 'patient',
        canActivate: [AuthGuard],
        data: { roles: ['PATIENT'] },
        children: [
            {
                path: 'dashboard',
                loadComponent: () => import('./features/patient/dashboard/dashboard.component').then(m => m.PatientDashboardComponent)
            },
            {
                path: 'profile',
                loadComponent: () => import('./features/patient/profile/profile.component').then(m => m.PatientProfileComponent)
            },
            {
                path: 'images',
                loadComponent: () => import('./features/patient/images/images.component').then(m => m.PatientImagesComponent)
            }
        ]
    },

    // Doctor routes
    {
        path: 'doctor',
        canActivate: [AuthGuard],
        data: { roles: ['MEDECIN'] },
        children: [
            {
                path: 'dashboard',
                loadComponent: () => import('./features/doctor/dashboard/dashboard.component').then(m => m.DoctorDashboardComponent)
            },
            {
                path: 'patients',
                loadComponent: () => import('./features/doctor/patients/patients.component').then(m => m.DoctorPatientsComponent)
            },
            {
                path: 'patients/:id',
                loadComponent: () => import('./features/doctor/patient-detail/patient-detail.component').then(m => m.PatientDetailComponent)
            },
            {
                path: 'imaging',
                loadComponent: () => import('./features/doctor/imaging/imaging.component').then(m => m.DoctorImagingComponent)
            },
            {
                path: 'pending-validations',
                loadComponent: () => import('./features/doctor/pending-validations/pending-validations.component').then(m => m.PendingValidationsComponent)
            }
        ]
    },

    // Admin routes
    {
        path: 'admin',
        canActivate: [AuthGuard],
        data: { roles: ['ADMIN'] },
        children: [
            {
                path: 'dashboard',
                loadComponent: () => import('./features/admin/dashboard/dashboard.component').then(m => m.AdminDashboardComponent)
            },
            {
                path: 'users',
                loadComponent: () => import('./features/admin/users/users.component').then(m => m.AdminUsersComponent)
            }
        ]
    },

    // Error routes
    {
        path: 'unauthorized',
        loadComponent: () => import('./shared/unauthorized/unauthorized.component').then(m => m.UnauthorizedComponent)
    },
    {
        path: '**',
        loadComponent: () => import('./shared/not-found/not-found.component').then(m => m.NotFoundComponent)
    }
];
