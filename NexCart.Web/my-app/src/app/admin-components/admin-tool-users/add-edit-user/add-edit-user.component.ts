import { Component, Inject, Input, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';
import { ToastrService } from 'ngx-toastr';
import { RoleModel } from 'src/app/models/role-model';
import { UserGridModel } from 'src/app/models/user-grid-model';
import { AdminToolUserService } from '../admin-tool-user-service/admin-tool-user.service';

@Component({ standalone: false,
  selector: 'app-add-edit-user',
  templateUrl: './add-edit-user.component.html',
  styleUrls: ['./add-edit-user.component.css']
})
export class AddEditUserComponent implements OnInit {

  @Input() model: UserGridModel
  title: string = ""
  roles: RoleModel[] = [];

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: any,
    private adminToolUserService: AdminToolUserService,
    private toastr: ToastrService,
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
      this.toastr.error("Failed to load roles");
    })
  }

}
