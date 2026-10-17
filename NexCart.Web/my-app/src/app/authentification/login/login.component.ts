import { Component, Input, OnInit } from '@angular/core';
import { AuthentificationService } from '../authentification.service';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';


@Component({ standalone: false,
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css', '../auth-shared.css']
})
export class LoginComponent implements OnInit {

  @Input() isAuthenticated?: boolean;

  errorMessage?:string = "";
  username: string = "";
  password: string = "";
  showPassword: boolean = false;
  constructor(
    private authService:AuthentificationService,
    private router:Router,
    ) { }

  ngOnInit(): void {
    if(this.authService.isAuthenticated()){
      this.router.navigate(["/"])
    }
  }
  loginUser(){

    if(this.username ==="" || this.password ===""){
      this.errorMessage = "Fields are required";
      return;
    }
    this.authService.login(this.username, this.password).subscribe(()=>{
      window.location.replace('/')
    },
    (error) => {
      this.errorMessage = error.error.exception;
    })
  }
}

