import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthentificationService } from '../authentification.service';
import { UserRegister } from 'src/app/models/register/register-user';

@Component({ standalone: false,
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
  errorMessage: string[] = [];

  registerUserModel: UserRegister = new UserRegister();

  repaetPassword: string = "";

  constructor(
    private router: Router,
    private authService:AuthentificationService) 
    { }

  ngOnInit(): void {
    if(this.authService.isAuthenticated()){
      this.router.navigate(["/"])
    }
  }
  registerUser(){
    this.errorMessage = [];
    if(this.registerUserModel.Email ===""){
      this.errorMessage.push("Email is required");
    }
    if(this.registerUserModel.Username ===""){
      this.errorMessage.push("Username is required");
    }
    if(this.registerUserModel.Password ===""){
      this.errorMessage.push("Password is required");
    }
    if(this.registerUserModel.Email === "" || this.registerUserModel.Username ==="" || this.registerUserModel.Password === ""){
      return;
    }

    this.authService.registerUser(this.registerUserModel).subscribe((result)=>{
       if(result === false){
         this.errorMessage = ["Registration failed"];
         return;
       }
       window.location.replace('/login')
      },
      (error) => {
        this.errorMessage = [];
        if(error?.error?.Errors){
          this.errorMessage = error.error.Errors;
        }else{
          this.errorMessage.push(error?.error?.exception || "Registration failed");
        }
      }
    )
  }
}

