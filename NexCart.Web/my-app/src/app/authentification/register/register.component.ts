import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthentificationService } from '../authentification.service';
import { UserRegister } from 'src/app/models/register/register-user';
import { PASSWORD_REQUIREMENTS, PasswordRequirement, validatePassword } from 'src/app/utils/password-policy';
import { extractValidationMessages } from 'src/app/utils/error.interceptor';
import { FlashMessageService } from 'src/app/utils/flash-message.service';

@Component({ standalone: false,
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css', '../auth-shared.css']
})
export class RegisterComponent implements OnInit {
  errorMessage: string[] = [];

  registerUserModel: UserRegister = new UserRegister();

  repeatPassword: string = "";
  showPassword: boolean = false;
  showRepeatPassword: boolean = false;

  /** Live password checklist rendered under the password field. */
  passwordRequirements: PasswordRequirement[] = PASSWORD_REQUIREMENTS;

  constructor(
    private router: Router,
    private authService:AuthentificationService,
    private flashMessage: FlashMessageService) 
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
    this.errorMessage.push(...validatePassword(this.registerUserModel.Password));
    if(this.repeatPassword ===""){
      this.errorMessage.push("Please confirm your password");
    }else if(this.repeatPassword !== this.registerUserModel.Password){
      this.errorMessage.push("Passwords do not match");
    }
    if(this.errorMessage.length > 0){
      return;
    }

    this.authService.registerUser(this.registerUserModel).subscribe((result)=>{
       if(result === false){
         this.errorMessage = ["Registration failed"];
         return;
       }
       this.flashMessage.set("User successfully created! Please log in.");
       window.location.replace('/login')
      },
      (error) => {
        this.errorMessage = extractValidationMessages(error?.error?.Errors);
        if(this.errorMessage.length === 0){
          this.errorMessage.push(error?.error?.exception || "Registration failed");
        }
      }
    )
  }
}

