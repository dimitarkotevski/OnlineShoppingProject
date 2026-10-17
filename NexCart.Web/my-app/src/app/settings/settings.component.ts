import { Component, OnInit } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { countries } from '../countries/countries';
import { UserDetails } from '../models/login/user-details';
import { AuthentificationService } from '../authentification/authentification.service';
import { ChangePasswordViewModel } from '../models/change-password-view-model';
import { PASSWORD_REQUIREMENTS, PasswordRequirement, validatePassword } from '../utils/password-policy';

@Component({ standalone: false,
  selector: 'app-settings',
  templateUrl: './settings.component.html',
  styleUrls: ['./settings.component.css']
})
export class SettingsComponent implements OnInit {
  countriesList = countries;
  activeTab: string = 'account';
  userDetails?: UserDetails;

  changePasswordViewModel: ChangePasswordViewModel;
  repeatNewPassword: string = "";
  passwordErrorMessage: string[] = [];
  passwordRequirements: PasswordRequirement[] = PASSWORD_REQUIREMENTS;

  constructor( private authService: AuthentificationService,private toastr: ToastrService ) { 
    this.changePasswordViewModel = new ChangePasswordViewModel();
  }

  ngOnInit(): void {
    this.authService.getUserDetails(this.authService.getLoggedUserId()).subscribe((response:UserDetails)=>{
      this.userDetails = response;
    });
    
  }
  updateUserPublicInfo(user:UserDetails){
    this.authService.updateUserDetails(user).subscribe(()=>{
      this.toastr.success('Update info')
    })
  }

  switchTab(tab: string) {
    this.activeTab = tab;
  }

  changePassword(){
    this.changePasswordViewModel.Id = this.authService.getLoggedUserId() || undefined;
    this.passwordErrorMessage = [];

    if(!this.changePasswordViewModel.Id){
      return;
    }
    if(!this.changePasswordViewModel.OldPassword){
      this.passwordErrorMessage.push("Current password is required");
    }
    this.passwordErrorMessage.push(...validatePassword(this.changePasswordViewModel.NewPassword));
    if(this.repeatNewPassword === ""){
      this.passwordErrorMessage.push("Please confirm your new password");
    }else if(this.repeatNewPassword !== this.changePasswordViewModel.NewPassword){
      this.passwordErrorMessage.push("Passwords do not match");
    }
    if(this.passwordErrorMessage.length > 0){
      return;
    }

    this.authService.changePassword(this.changePasswordViewModel).subscribe(()=>{
      this.toastr.success("Successfully changed password");
      this.changePasswordViewModel.OldPassword = "";
      this.changePasswordViewModel.NewPassword = "";
      this.repeatNewPassword = "";
      this.passwordErrorMessage = [];
    })
  }
}

