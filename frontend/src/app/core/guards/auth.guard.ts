import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, Router, UrlTree } from '@angular/router';
import { Observable } from 'rxjs';
import { AuthService } from '../services/auth.service';

@Injectable({
    providedIn: 'root'
})
export class AuthGuard implements CanActivate {
    constructor(
        private authService: AuthService,
        private router: Router
    ) { }

    canActivate(
        route: ActivatedRouteSnapshot,
        state: RouterStateSnapshot
    ): Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree {
        if (this.authService.isAuthenticated) {
            // Check for required roles
            const requiredRoles = route.data['roles'] as string[] | undefined;

            if (requiredRoles && requiredRoles.length > 0) {
                const hasRole = requiredRoles.some(role => this.authService.hasRole(role));
                if (!hasRole) {
                    this.router.navigate(['/unauthorized']);
                    return false;
                }
            }

            return true;
        }

        // Store the attempted URL for redirecting after login
        this.router.navigate(['/login'], { queryParams: { returnUrl: state.url } });
        return false;
    }
}

@Injectable({
    providedIn: 'root'
})
export class GuestGuard implements CanActivate {
    constructor(
        private authService: AuthService,
        private router: Router
    ) { }

    canActivate(): boolean {
        if (this.authService.isAuthenticated) {
            // Redirect to appropriate dashboard based on role
            if (this.authService.isAdmin()) {
                this.router.navigate(['/admin/dashboard']);
            } else if (this.authService.isDoctor()) {
                this.router.navigate(['/doctor/dashboard']);
            } else {
                this.router.navigate(['/patient/dashboard']);
            }
            return false;
        }
        return true;
    }
}
