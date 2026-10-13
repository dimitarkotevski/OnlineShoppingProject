import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthentificationService } from 'src/app/authentification/authentification.service';

@Component({ standalone: false,
  selector: 'app-admin-tool',
  templateUrl: './admin-tool.component.html',
  styleUrls: ['./admin-tool.component.css']
})
export class AdminToolComponent implements OnInit {

  activeAdminTool = ''

  constructor(
    private router: Router,
    private authService: AuthentificationService
    ) { }

  ngOnInit(): void {

    if(this.authService.getRole() != 'admin'){
      this.router.navigate(['error-404']);
    }

    if(this.activeAdminTool===''){
      this.router.navigate(['admin-tool/products']);
    }
  }

}

