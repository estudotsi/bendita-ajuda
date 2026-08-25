import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, Router, UrlTree } from '@angular/router';

import { AuthService } from '../services/auth.service';
import { UserRole } from '../../pages/auth/models/auth.models';

@Injectable({
  providedIn: 'root',
})
export class AuthGuard implements CanActivate {
  constructor(
    private readonly authService: AuthService,
    private readonly router: Router,
  ) {}

  canActivate(route: ActivatedRouteSnapshot): boolean | UrlTree {
    if (this.authService.isAuthenticated()) {
      const allowedRoles = route.data['roles'] as UserRole[] | undefined;

      if (!allowedRoles?.length || this.authService.hasAnyRole(allowedRoles)) {
        return true;
      }

      return this.router.createUrlTree([this.authService.getDefaultRouteForCurrentUser()]);
    }

    return this.router.createUrlTree(['/login']);
  }
}
