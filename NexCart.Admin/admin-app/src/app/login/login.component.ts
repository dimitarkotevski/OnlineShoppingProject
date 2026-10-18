import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from 'src/app/core/auth.service';

@Component({ standalone: false,
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {

  username = '';
  password = '';
  errorMessage = '';
  isSubmitting = false;

  private returnUrl = '/products';

  constructor(
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute
  ) { }

  ngOnInit(): void {
    const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl');
    // Only follow in-app paths, never an absolute URL from the query string.
    if (returnUrl && returnUrl.startsWith('/') && !returnUrl.startsWith('//')) {
      this.returnUrl = returnUrl;
    }

    // Still signed in from an earlier visit: skip the form.
    if (this.authService.isAdmin()) {
      this.router.navigateByUrl(this.returnUrl);
    }
  }

  submit(): void {
    this.errorMessage = '';
    this.isSubmitting = true;

    this.authService.login(this.username, this.password).subscribe({
      next: () => {
        this.isSubmitting = false;
        if (this.authService.isAdmin()) {
          this.router.navigateByUrl(this.returnUrl);
          return;
        }
        // The server only issues admin tokens here; anything else is not a usable session.
        this.authService.logout();
        this.errorMessage = 'This account does not have administrator access.';
      },
      error: (error) => {
        this.isSubmitting = false;
        // e.g. "Username or password are incorrect" or "This account does not have administrator access."
        this.errorMessage = error?.error?.exception || 'Incorrect username or password.';
      }
    });
  }
}
