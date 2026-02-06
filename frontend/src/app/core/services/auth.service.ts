import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap, catchError, of } from 'rxjs';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';
import { AuthResponse, LoginRequest, RegisterRequest, User, ApiResponse } from '../models';

@Injectable({
    providedIn: 'root'
})
export class AuthService {
    private readonly TOKEN_KEY = 'access_token';
    private readonly REFRESH_TOKEN_KEY = 'refresh_token';
    private readonly USER_KEY = 'user';
    private readonly EXPIRES_AT_KEY = 'expires_at';

    private currentUserSubject = new BehaviorSubject<User | null>(null);
    public currentUser$ = this.currentUserSubject.asObservable();

    private isAuthenticatedSubject = new BehaviorSubject<boolean>(false);
    public isAuthenticated$ = this.isAuthenticatedSubject.asObservable();

    private refreshTokenTimeout?: any;

    constructor(
        private http: HttpClient,
        private router: Router
    ) {
        this.loadStoredUser();
    }

    private loadStoredUser(): void {
        const storedUser = localStorage.getItem(this.USER_KEY);
        const token = localStorage.getItem(this.TOKEN_KEY);
        const expiresAt = localStorage.getItem(this.EXPIRES_AT_KEY);

        if (storedUser && token && expiresAt) {
            const expiry = new Date(expiresAt);
            if (expiry > new Date()) {
                this.currentUserSubject.next(JSON.parse(storedUser));
                this.isAuthenticatedSubject.next(true);
                this.startRefreshTokenTimer();
            } else {
                this.logout();
            }
        }
    }

    register(request: RegisterRequest): Observable<AuthResponse> {
        return this.http.post<AuthResponse>(`${environment.apiUrl}/auth/register`, request)
            .pipe(
                tap(response => {
                    if (response.success) {
                        this.handleAuthSuccess(response);
                    }
                })
            );
    }

    login(request: LoginRequest): Observable<AuthResponse> {
        return this.http.post<AuthResponse>(`${environment.apiUrl}/auth/login`, request)
            .pipe(
                tap(response => {
                    if (response.success) {
                        this.handleAuthSuccess(response);
                    }
                })
            );
    }

    refreshToken(): Observable<AuthResponse | null> {
        const refreshToken = localStorage.getItem(this.REFRESH_TOKEN_KEY);
        if (!refreshToken) {
            this.logout();
            return of(null);
        }

        return this.http.post<AuthResponse>(`${environment.apiUrl}/auth/refresh`, { refreshToken })
            .pipe(
                tap(response => {
                    if (response.success) {
                        this.handleAuthSuccess(response);
                    }
                }),
                catchError(error => {
                    this.logout();
                    return of(null);
                })
            );
    }

    logout(): void {
        const refreshToken = localStorage.getItem(this.REFRESH_TOKEN_KEY);

        if (refreshToken) {
            this.http.post(`${environment.apiUrl}/auth/logout`, { refreshToken })
                .pipe(catchError(() => of(null)))
                .subscribe();
        }

        this.clearStorage();
        this.currentUserSubject.next(null);
        this.isAuthenticatedSubject.next(false);
        this.stopRefreshTokenTimer();
        this.router.navigate(['/login']);
    }

    changePassword(currentPassword: string, newPassword: string): Observable<ApiResponse<void>> {
        return this.http.post<ApiResponse<void>>(`${environment.apiUrl}/auth/change-password`, {
            currentPassword,
            newPassword,
            confirmNewPassword: newPassword
        });
    }

    private handleAuthSuccess(response: AuthResponse): void {
        localStorage.setItem(this.TOKEN_KEY, response.accessToken);
        localStorage.setItem(this.REFRESH_TOKEN_KEY, response.refreshToken);
        localStorage.setItem(this.USER_KEY, JSON.stringify(response.user));
        localStorage.setItem(this.EXPIRES_AT_KEY, response.expiresAt.toString());

        this.currentUserSubject.next(response.user);
        this.isAuthenticatedSubject.next(true);
        this.startRefreshTokenTimer();
    }

    private clearStorage(): void {
        localStorage.removeItem(this.TOKEN_KEY);
        localStorage.removeItem(this.REFRESH_TOKEN_KEY);
        localStorage.removeItem(this.USER_KEY);
        localStorage.removeItem(this.EXPIRES_AT_KEY);
    }

    private startRefreshTokenTimer(): void {
        this.stopRefreshTokenTimer();

        const expiresAt = localStorage.getItem(this.EXPIRES_AT_KEY);
        if (!expiresAt) return;

        const expiry = new Date(expiresAt);
        const timeout = expiry.getTime() - Date.now() - (60 * 1000); // Refresh 1 minute before expiry

        if (timeout > 0) {
            this.refreshTokenTimeout = setTimeout(() => {
                this.refreshToken().subscribe();
            }, timeout);
        }
    }

    private stopRefreshTokenTimer(): void {
        if (this.refreshTokenTimeout) {
            clearTimeout(this.refreshTokenTimeout);
        }
    }

    getToken(): string | null {
        return localStorage.getItem(this.TOKEN_KEY);
    }

    get currentUser(): User | null {
        return this.currentUserSubject.value;
    }

    get isAuthenticated(): boolean {
        return this.isAuthenticatedSubject.value;
    }

    hasRole(role: string): boolean {
        return this.currentUser?.roles?.includes(role) ?? false;
    }

    isAdmin(): boolean {
        return this.hasRole('ADMIN');
    }

    isDoctor(): boolean {
        return this.hasRole('MEDECIN');
    }

    isPatient(): boolean {
        return this.hasRole('PATIENT');
    }
}
