import { Component, Inject, Input, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { RoleModel } from 'src/app/models/role-model';
import { UserGridModel } from 'src/app/models/user-grid-model';
import { AdminToolUserService } from '../admin-tool-user-service/admin-tool-user.service';
import { PASSWORD_REQUIREMENTS, PasswordRequirement, validatePassword } from 'src/app/utils/password-policy';

@Component({ standalone: false,
  selector: 'app-add-edit-user',
  templateUrl: './add-edit-user.component.html',
  styleUrls: ['./add-edit-user.component.css']
})
export class AddEditUserComponent implements OnInit {

  @Input() model: UserGridModel
  title: string = ""
  roles: RoleModel[] = [];
  passwordErrorMessage: string[] = [];
  passwordRequirements: PasswordRequirement[] = PASSWORD_REQUIREMENTS;

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: any,
    private dialogRef: MatDialogRef<AddEditUserComponent>,
    private adminToolUserService: AdminToolUserService,
  ) { 
    this.model = data.user;
  }

  ngOnInit(): void {
    if(this.model.Id){
      this.title = "Edit"
    }else{
      this.title = "Add"
    }

    this.adminToolUserService.getRoles().subscribe((res: RoleModel[]) => {
      this.roles = res;
    }, () => {
      // The failure is reported globally by the error interceptor.
    })
  }

  /** Validates the password (new users only) before closing the dialog with the model. */
  save(){
    this.passwordErrorMessage = [];

    if(!this.model.Id){
      this.passwordErrorMessage = validatePassword(this.model.Password);
      if(this.passwordErrorMessage.length > 0){
        return;
      }
    }

    this.dialogRef.close(this.model);
  }

}
