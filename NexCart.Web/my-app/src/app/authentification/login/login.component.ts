import { Component, Input, OnInit } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { AuthentificationService } from '../authentification.service';
import { Router } from '@angular/router';
import { extractValidationMessages } from 'src/app/utils/error.interceptor';
import { FlashMessageService } from 'src/app/utils/flash-message.service';


@Component({ standalone: false,
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css', '../auth-shared.css']
})
export class LoginComponent implements OnInit {

  @Input() isAuthenticated?: boolean;

  username: string = "";
  password: string = "";
  showPassword: boolean = false;

  /** Set on the first submit so both fields can reveal their error. */
  submitted: boolean = false;
  usernameTouched: boolean = false;
  passwordTouched: boolean = false;

  constructor(
    private authService:AuthentificationService,
    private router:Router,
    private toastr:ToastrService,
    private flashMessage: FlashMessageService,
    ) { }

  ngOnInit(): void {
    if(this.authService.isAuthenticated()){
      this.router.navigate(["/"])
    }
  }

  get usernameValid(): boolean {
    return this.username.trim() !== '';
  }

  get passwordValid(): boolean {
    return this.password !== '';
  }

  /** Whether each field's inline error should currently be shown. */
  get showUsernameError(): boolean {
    return (this.usernameTouched || this.submitted) && !this.usernameValid;
  }

  get showPasswordError(): boolean {
    return (this.passwordTouched || this.submitted) && !this.passwordValid;
  }

  loginUser(){
    this.submitted = true;

    if(!this.usernameValid || !this.passwordValid){
      return;
    }

    this.authService.login(this.username, this.password).subscribe((result)=>{
      if(result === false){
        this.toastr.error("Invalid username or password", "Error");
        return;
      }
      this.flashMessage.set("Successfully logged in!");
      window.location.replace('/')
    },
    (error) => {
      const messages = extractValidationMessages(error?.error?.Errors);
      this.toastr.error(
        messages.length > 0
          ? messages.join(', ')
          : (error?.error?.exception || "Invalid username or password"),
        "Error"
      );
    })
  }
}

