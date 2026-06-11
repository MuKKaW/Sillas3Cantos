import { Component, computed, inject, signal } from '@angular/core';
import { NavigationEnd, Router, RouterLink, RouterOutlet } from '@angular/router';
import { filter } from 'rxjs';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  private readonly router = inject(Router);
  private readonly currentUrl = signal(this.router.url);

  readonly currentPath = computed(() => this.currentUrl().split('?')[0].split('#')[0]);
  readonly isPublicLanding = computed(() => {
    const path = this.currentPath();
    return path === '' || path === '/';
  });
  readonly isPublicPage = computed(() => this.isPublicLanding() || this.currentPath() === '/legal');
  readonly showHeaderPhone = this.isPublicLanding;
  readonly showPublicFooter = this.isPublicPage;

  constructor() {
    this.router.events
      .pipe(filter((event): event is NavigationEnd => event instanceof NavigationEnd))
      .subscribe((event) => this.currentUrl.set(event.urlAfterRedirects));
  }
}
