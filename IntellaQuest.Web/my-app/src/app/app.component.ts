import { Component, OnInit } from '@angular/core';
import { AuthentificationService } from './authentification/authentification.service';

@Component({ standalone: false,
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  isUserAuthenticated: boolean = false;
  constructor(
    private authService: AuthentificationService
  ) {
  }
  ngOnInit(): void {
    this.refreshState();
  }
  refreshState(){
    this.isUserAuthenticated = this.authService.isAuthenticated();
  }
}

