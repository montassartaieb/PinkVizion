import { Injectable } from '@angular/core';
import {
    HttpRequest,
    HttpHandler,
    HttpEvent,
    HttpInterceptor,
    HttpErrorResponse
} from '@angular/common/http';
import { Observable, throwError, BehaviorSubject } from 'rxjs';
import { catchError, filter, take, switchMap } from 'rxjs/operators';
import { AuthService } from '../services/auth.service';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
    private isRefreshing = false;
    private refreshTokenSubject: BehaviorSubject<any> = new BehaviorSubject<any>(null);

    constructor(private authService: AuthService) { }

    intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
        // Skip auth header for auth endpoints
        if (request.url.includes('/auth/login') ||
            request.url.includes('/auth/register') ||
            request.url.includes('/auth/refresh')) {
            return next.handle(request);
        }

        const token = this.authService.getToken();
        if (token) {
            request = this.addToken(request, token);
        }

        return next.handle(request).pipe(
            catchError((error: HttpErrorResponse) => {
                if (error.status === 401) {
                    return this.handle401Error(request, next);
                }
                return throwError(() => error);
            })
        );
    }

    private addToken(request: HttpRequest<any>, token: string): HttpRequest<any> {
        return request.clone({
            setHeaders: {
                Authorization: `Bearer ${token}`
            }
        });
    }

    private handle401Error(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        if (!this.isRefreshing) {
            this.isRefreshing = true;
            this.refreshTokenSubject.next(null);

            return this.authService.refreshToken().pipe(
                switchMap((response) => {
                    this.isRefreshing = false;
                    if (response) {
                        this.refreshTokenSubject.next(response.accessToken);
                        return next.handle(this.addToken(request, response.accessToken));
                    }
                    return throwError(() => new Error('Token refresh failed'));
                }),
                catchError((err) => {
                    this.isRefreshing = false;
                    this.authService.logout();
                    return throwError(() => err);
                })
            );
        } else {
            return this.refreshTokenSubject.pipe(
                filter(token => token != null),
                take(1),
                switchMap(token => next.handle(this.addToken(request, token)))
            );
        }
    }
}
