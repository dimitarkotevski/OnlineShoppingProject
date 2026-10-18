import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { ToastrService } from 'ngx-toastr';
import { ResponseModel } from 'src/app/models/admin-tool-response-model';
import { CategoryEditModel, CategoryGridModel } from 'src/app/models/category-grid';
import { FilterableGrid } from 'src/app/shared/filterable-grid';
import { YesNoDialogComponent } from 'src/app/shared/yes-no-dialog/yes-no-dialog.component';
import { AddEditCategoryComponent } from './add-edit-category/add-edit-category.component';
import { AdminToolCategoryService } from './admin-tool-category-service/admin-tool-category.service';

@Component({ standalone: false,
  selector: 'app-admin-tool-categories',
  templateUrl: './admin-tool-categories.component.html',
  styleUrls: ['./admin-tool-categories.component.css']
})
export class AdminToolCategoriesComponent extends FilterableGrid implements OnInit {

  response: ResponseModel<CategoryGridModel> = new ResponseModel<CategoryGridModel>();

  pagination: number[] = [5 , 10, 20, 50, 100]

  constructor(
    private dialog: MatDialog,
    private toastr: ToastrService,
    private adminToolCategoryService: AdminToolCategoryService,
  ) {
    super();
  }

  ngOnInit(): void {
    this.request = {
      PageNeeded: 1,
      Size: 10,
    }

    this.getData();
  }

  get totalPages(): number {
    return this.response.Size > 0 ? Math.ceil(this.response.TotalItems / this.response.Size) : 0;
  }

  getData(){
    this.adminToolCategoryService.getTable(this.apiRequest()).subscribe((response: ResponseModel<CategoryGridModel>)=>{
      this.response = response;
    }, ()=>{
      // The failure is reported globally by the error interceptor.
    })
  }

  addEditCategory(category?: CategoryGridModel){
    const dialogRef = this.dialog.open(AddEditCategoryComponent, {
      data: { category },
      width: '500px', // Set the width of the modal
      panelClass: ['custom-modalbox', 'admin-dialog']
    });

    dialogRef.afterClosed().subscribe((result: CategoryEditModel) => {
      // Cancel / Esc / backdrop click close without a model: nothing to save.
      if(!result){
        return;
      }
      const request = result.Id
        ? this.adminToolCategoryService.update(result)
        : this.adminToolCategoryService.create(result);
      request.subscribe(()=>{
        this.toastr.success(result.Id ? "Success changed category" : "Success added category");
        this.getData();
      })
    });
  }

  deleteCategory(category: CategoryGridModel){
    if(!category.Id || (category.ProductCount || 0) > 0){
      return;
    }
    const id = category.Id;
    const dialogRef = this.dialog.open(YesNoDialogComponent, {
      data: { text: `Do you want to delete the category "${category.Name}"?` },
      width: '500px', // Set the width of the modal
      panelClass: ['custom-modalbox', 'admin-dialog']
    });
    dialogRef.afterClosed().subscribe((confirmed: boolean) => {
      if(confirmed){
        this.adminToolCategoryService.delete(id).subscribe(()=>{
          this.toastr.success("Success deleted category");
          // Step back a page when the last row of the last page was removed.
          const page = this.request.PageNeeded || 1;
          if((this.response.Items?.length || 0) <= 1 && page > 1){
            this.request.PageNeeded = page - 1;
          }
          this.getData();
        })
      }
    });
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
