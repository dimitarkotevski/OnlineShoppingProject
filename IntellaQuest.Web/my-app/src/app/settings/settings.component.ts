import { Component, OnInit } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { countries } from '../countries/countries';
import { UserDetails } from '../models/login/user-details';
import { AuthentificationService } from '../authentification/authentification.service';
import { ChangePasswordViewModel } from '../models/change-password-view-model';

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
    if(this.changePasswordViewModel.Id && this.changePasswordViewModel.OldPassword != "" && this.changePasswordViewModel.NewPassword != ""){
      this.authService.changePassword(this.changePasswordViewModel).subscribe(()=>{
        this.toastr.success("Successfully changed password");
      })
    }
  }
}

