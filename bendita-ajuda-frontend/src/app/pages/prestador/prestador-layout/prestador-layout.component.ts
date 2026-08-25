import { Component } from '@angular/core';
import { Router } from '@angular/router';

import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-prestador-layout',
  standalone: false,
  templateUrl: './prestador-layout.component.html',
  styleUrl: './prestador-layout.component.scss',
})
export class PrestadorLayoutComponent {
  constructor(
    private readonly authService: AuthService,
    private readonly router: Router,
  ) {}

  protected logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
