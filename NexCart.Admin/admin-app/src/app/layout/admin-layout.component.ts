import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from 'src/app/core/auth.service';

interface NavItem {
  label: string;
  path: string;
  icon: string;
}

/** Sidebar + content shell wrapped around every signed-in admin page. */
@Component({ standalone: false,
  selector: 'app-admin-layout',
  templateUrl: './admin-layout.component.html',
  styleUrls: ['./admin-layout.component.css']
})
export class AdminLayoutComponent {

  navItems: NavItem[] = [
    { label: 'Products', path: '/products', icon: 'fa-box-open' },
    { label: 'Categories', path: '/categories', icon: 'fa-tags' },
    { label: 'Orders', path: '/orders', icon: 'fa-receipt' },
    { label: 'Users', path: '/users', icon: 'fa-users' },
  ];

  /** Sidebar visibility on small screens; on desktop it is always shown. */
  menuOpen = false;

  username?: string;

  constructor(
    private authService: AuthService,
    private router: Router
  ) {
    this.username = this.authService.getLoggedUsername();
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
