import { Component, OnInit } from '@angular/core';
import { AdminToolUserService } from './admin-tool-user-service/admin-tool-user.service';
import { RequestModel } from '../admin-tool-models/admin-tool-request-model';
import { ResponseModel } from '../admin-tool-models/admin-tool-response-model';
import { UserGridModel } from 'src/app/models/user-grid-model';
import { YesNoDialogComponent } from 'src/app/customer-components/yes-no-dialog/yes-no-dialog.component';
import { MatDialog } from '@angular/material/dialog';
import { ToastrService } from 'ngx-toastr';
import { AddEditUserComponent } from './add-edit-user/add-edit-user.component';
import { AuthentificationService } from 'src/app/authentification/authentification.service';
import { Router } from '@angular/router';

@Component({ standalone: false,
  selector: 'app-admin-tool-users',
  templateUrl: './admin-tool-users.component.html',
  styleUrls: ['./admin-tool-users.component.css']
})
export class AdminToolUsersComponent implements OnInit {

  request: RequestModel;
  response: ResponseModel<UserGridModel>

  pagination: number[] = [5 , 10, 20, 50, 100]

  newItem: UserGridModel;

  
  constructor(
    private router: Router,
    private dialog: MatDialog,
    private toastr: ToastrService,
    private authService: AuthentificationService,
    private adminToolUserService: AdminToolUserService
  ) { 
    this.request = new RequestModel();
      this.response = new ResponseModel<UserGridModel>()
      this.newItem = new UserGridModel();
  }

  ngOnInit(): void {

    if(this.authService.getRole() != 'admin'){
      this.router.navigate(['error-404']);
      return;
    }

    this.request = {
      PageNeeded: 1,
      Size: 5,
    }

    this.getData();
  }

  getData(){
    this.adminToolUserService.getTable(this.request).subscribe((res: ResponseModel<UserGridModel>)=>{
      this.response = res;
    }, ()=>{
      this.toastr.error("Failed to load users");
    })
  }

  addEditUser(user:UserGridModel){
    const dialogRef = this.dialog.open(AddEditUserComponent, {
      data: { user },
      width: '700px', // Set the width of the modal
      panelClass: 'custom-modalbox'
    });
  
    dialogRef.afterClosed().subscribe((result : UserGridModel) => {
      if(result!= null){
        if(result.Id){
          this.adminToolUserService.update(result).subscribe(()=>{
            this.toastr.success("Success changed user")
            this.getData();
          })
        }else{
          this.adminToolUserService.create(result).subscribe(()=>{
            this.toastr.success("Success added user")
            this.getData();
          })
        }
      }
    });
    this.newItem = {
      Id: "",
      FirstName: "",
      LastName: "",
      Username: "",
      Password: "",
      Email: "",
      Role: "",
      City: "",
      State: "",
      Address : "",
      ZipCode: ""
    };
  }

  deleteUser(id:string | undefined){
    if(id){
      const dialogRef = this.dialog.open(YesNoDialogComponent, {
        data: { text: 'Do you want to delete this user?' },
        width: '500px', // Set the width of the modal
        panelClass: 'custom-modalbox'
      });
      dialogRef.afterClosed().subscribe((result : UserGridModel) => {
        if(result){
          this.adminToolUserService.delete(id).subscribe(()=>{
            this.toastr.success("Success deleted user")
            this.getData()
          })
        }
      });
    }
  }

  resetSearchString(){
    this.request.SearchString = '';
    this.getData();
  }

  onPageChange(value: number){
    this.request.PageNeeded = value;
    this.getData();
  }
  onPageSizeChange(value : number){
    this.request.Size = value;
    this.getData();
  }
}

