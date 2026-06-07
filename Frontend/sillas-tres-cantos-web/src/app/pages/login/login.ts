import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-login',
  imports: [CommonModule, FormsModule],
  templateUrl: './login.html',
  styleUrl: './login.scss',
})
export class Login {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  username = '';
  password = '';
  readonly loading = signal(false);
  readonly errorMessage = signal('');
  readonly successMessage = signal('');

  async submit(): Promise<void> {
    this.loading.set(true);
    this.errorMessage.set('');
    this.successMessage.set('');

    try {
      await firstValueFrom(
        this.authService.login({
          username: this.username.trim(),
          password: this.password
        })
      );

      this.successMessage.set('Login correcto. Redirigiendo a backoffice...');
      await this.router.navigateByUrl('/backoffice');
    } catch (error) {
      console.error(error);
      this.errorMessage.set('Credenciales invalidas o API no disponible.');
    } finally {
      this.loading.set(false);
    }
  }

  usarCredencialesDemo(tipo: 'admin' | 'user'): void {
    if (tipo === 'admin') {
      this.username = 'admin';
      this.password = 'Admin12345!';
      return;
    }

    this.username = 'user';
    this.password = 'User12345!';
  }
}
