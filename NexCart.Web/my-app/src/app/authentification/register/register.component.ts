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
  /** Server-side (API) errors, shown in the alert above the form. */
  errorMessage: string[] = [];

  registerUserModel: UserRegister = new UserRegister();

  repeatPassword: string = "";
  showPassword: boolean = false;
  showRepeatPassword: boolean = false;

  /** Set on the first submit so every field can reveal its error. */
  submitted: boolean = false;
  emailTouched: boolean = false;
  usernameTouched: boolean = false;
  passwordTouched: boolean = false;
  repeatPasswordTouched: boolean = false;

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

  get emailValid(): boolean {
    return (this.registerUserModel.Email ?? '').trim() !== '';
  }

  get emailFormatValid(): boolean {
    return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test((this.registerUserModel.Email ?? '').trim());
  }

  get usernameValid(): boolean {
    return (this.registerUserModel.Username ?? '').trim() !== '';
  }

  get passwordValid(): boolean {
    return validatePassword(this.registerUserModel.Password).length === 0;
  }

  get repeatPasswordValid(): boolean {
    return this.repeatPassword !== '' && this.repeatPassword === this.registerUserModel.Password;
  }

  /** Whether each field's inline error should currently be shown. */
  get showEmailError(): boolean {
    return (this.emailTouched || this.submitted) && (!this.emailValid || !this.emailFormatValid);
  }

  get showUsernameError(): boolean {
    return (this.usernameTouched || this.submitted) && !this.usernameValid;
  }

  get showPasswordError(): boolean {
    return (this.passwordTouched || this.submitted) && !this.passwordValid;
  }

  /** The password field only needs a text error when empty; otherwise the checklist explains what is missing. */
  get showPasswordRequiredError(): boolean {
    return (this.passwordTouched || this.submitted) && (this.registerUserModel.Password ?? '') === '';
  }

  get showRepeatPasswordError(): boolean {
    return (this.repeatPasswordTouched || this.submitted) && !this.repeatPasswordValid;
  }

  get repeatPasswordErrorMessage(): string {
    return this.repeatPassword === '' ? 'Please confirm your password.' : 'Passwords do not match.';
  }

  registerUser(){
    this.submitted = true;
    this.errorMessage = [];

    if(!this.emailValid || !this.emailFormatValid || !this.usernameValid
        || !this.passwordValid || !this.repeatPasswordValid){
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

