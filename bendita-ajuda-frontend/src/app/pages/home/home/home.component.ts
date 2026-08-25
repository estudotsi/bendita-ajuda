import { Component } from '@angular/core';
import { Router } from '@angular/router';

import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-home',
  standalone: false,
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss',
})
export class HomeComponent {
  constructor(
    private readonly authService: AuthService,
    private readonly router: Router,
  ) {}

  protected logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
