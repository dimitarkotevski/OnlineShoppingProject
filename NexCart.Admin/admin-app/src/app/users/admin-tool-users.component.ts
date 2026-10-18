import { Component, OnInit } from '@angular/core';
import { AdminToolUserService } from './admin-tool-user-service/admin-tool-user.service';
import { RequestModel } from 'src/app/models/admin-tool-request-model';
import { ResponseModel } from 'src/app/models/admin-tool-response-model';
import { UserGridModel } from 'src/app/models/user-grid-model';
import { YesNoDialogComponent } from 'src/app/shared/yes-no-dialog/yes-no-dialog.component';
import { MatDialog } from '@angular/material/dialog';
import { ToastrService } from 'ngx-toastr';
import { AddEditUserComponent } from './add-edit-user/add-edit-user.component';
import { GridSelection } from 'src/app/shared/grid-selection';
import { AuthService } from 'src/app/core/auth.service';

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

  /** Rows ticked for bulk delete (current page only); your own account can't be ticked. */
  selection: GridSelection;

  constructor(
    private dialog: MatDialog,
    private toastr: ToastrService,
    private adminToolUserService: AdminToolUserService,
    private authService: AuthService
  ) { 
    this.selection = new GridSelection(
      id => id.toLowerCase() !== (this.authService.getLoggedUserId() || '').toLowerCase());
    this.request = new RequestModel();
      this.response = new ResponseModel<UserGridModel>()
      this.newItem = new UserGridModel();
  }

  ngOnInit(): void {

    this.request = {
      PageNeeded: 1,
      Size: 5,
    }

    this.getData();
  }

  get totalPages(): number {
    return this.response.Size > 0 ? Math.ceil(this.response.TotalItems / this.response.Size) : 0;
  }

  fullName(user: UserGridModel): string {
    return [user.FirstName, user.LastName].filter(Boolean).join(' ') || '—';
  }

  /** Address, city, state and zip joined into one line, skipping empty parts. */
  formatAddress(user: UserGridModel): string {
    return [user.Address, user.City, user.State, user.ZipCode].filter(Boolean).join(', ') || '—';
  }

  isAdminRole(role?: string): boolean {
    return (role || '').toLowerCase() === 'admin';
  }

  getData(){
    this.adminToolUserService.getTable(this.request).subscribe((res: ResponseModel<UserGridModel>)=>{
      this.response = res;
      this.selection.clear();
    }, ()=>{
      // The failure is reported globally by the error interceptor.
    })
  }

  addEditUser(user:UserGridModel){
    const dialogRef = this.dialog.open(AddEditUserComponent, {
      data: { user },
      width: '700px', // Set the width of the modal
      panelClass: ['custom-modalbox', 'admin-dialog']
    });
  
    dialogRef.afterClosed().subscribe((result : UserGridModel) => {
      // Cancel / Esc / backdrop click close without a model: nothing to save.
      if(result){
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
        panelClass: ['custom-modalbox', 'admin-dialog']
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

  /** Deletes every ticked row on the current page after a single confirmation. */
  deleteSelected(){
    const ids = this.selection.selectedIds;
    if(ids.length === 0){
      return;
    }
    const label = ids.length === 1 ? '1 user' : `${ids.length} users`;
    const dialogRef = this.dialog.open(YesNoDialogComponent, {
      data: { text: `Do you want to delete ${label}?` },
      width: '500px', // Set the width of the modal
      panelClass: ['custom-modalbox', 'admin-dialog']
    });
    dialogRef.afterClosed().subscribe((confirmed: boolean) => {
      if(confirmed){
        this.adminToolUserService.deleteMany(ids).subscribe(()=>{
          this.toastr.success(`Deleted ${label}`);
          // Step back a page when everything on the last page was removed.
          const page = this.request.PageNeeded || 1;
          if(ids.length >= (this.response.Items?.length || 0) && page > 1){
            this.request.PageNeeded = page - 1;
          }
          this.getData();
        })
      }
    });
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
    this.request.PageNeeded = 1;
    this.getData();
  }
}

