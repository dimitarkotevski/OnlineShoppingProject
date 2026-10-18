import { Component, OnInit } from '@angular/core';
import { AdminToolProductService } from './admin-tool-product-service/admin-tool-product.service';
import { ProductGridModel } from 'src/app/models/product-grid';
import { ResponseModel } from 'src/app/models/admin-tool-response-model';
import { RequestModel } from 'src/app/models/admin-tool-request-model';
import { MatDialog } from '@angular/material/dialog';
import { PhotoDialogComponent } from './photo-dialog/photo-dialog.component';
import { ProductViewModel } from 'src/app/models/product-view-model';
import { AddEditProductDialogComponent } from './add-edit-product-dialog/add-edit-product-dialog.component';
import { ToastrService } from 'ngx-toastr';
import { YesNoDialogComponent } from 'src/app/shared/yes-no-dialog/yes-no-dialog.component';
import { GridSelection } from 'src/app/shared/grid-selection';

@Component({ standalone: false,
  selector: 'app-admin-tool-products',
  templateUrl: './admin-tool-products.component.html',
  styleUrls: ['./admin-tool-products.component.css']
})
export class AdminToolProductsComponent implements OnInit {

  // columns = [
  //   { prop: 'Name', name: 'Name' },
  //   { prop: 'Description', name: 'Description' },
  //   { prop: 'Price', name: 'Price' },
  //   { prop: 'Category.Name', name: 'Category' },
  //   { prop: 'View Photo', name: 'View Photo' },
  //   { prop: 'Edit', name: 'Edit' },
  //   { prop: 'Delete', name: 'Delete' },
  // ]
  request: RequestModel;
  response: ResponseModel<ProductGridModel>
  newItem: ProductViewModel;
  /** Rows ticked for bulk delete (current page only). */
  selection = new GridSelection();

  pagination: number[] = [5 , 10, 20, 50, 100]

  constructor(
    private dialog: MatDialog,
    private toastr: ToastrService,
    private adminToolProductService: AdminToolProductService,
    ) { 
      this.request = new RequestModel();
      this.response = new ResponseModel<ProductGridModel>()
      this.newItem = new ProductViewModel;
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
    this.adminToolProductService.getTable(this.request).subscribe((response: ResponseModel<ProductGridModel>)=>{
      this.response = response;
      this.selection.clear();
    }, ()=>{
      // The failure is reported globally by the error interceptor.
    })
  }

  openPhotoModal(data64: string,id : string): void {
    const dialogRef = this.dialog.open(PhotoDialogComponent, {
      data: { data64, id},
      width: '600px', // Set the width of the modal
      panelClass: ['custom-modalbox', 'admin-dialog']
    });
  
    dialogRef.afterClosed().subscribe(result => {
      this.getData();
    });
  }
  addEditProduct(product?: ProductViewModel){
    const dialogRef = this.dialog.open(AddEditProductDialogComponent, {
      data: { product },
      width: '600px', // Set the width of the modal
      panelClass: ['custom-modalbox', 'admin-dialog']
    });
  
    dialogRef.afterClosed().subscribe((result : ProductViewModel) => {
      // Cancel / Esc / backdrop click close without a product: nothing to save.
      if(result){
        if(result.Id){
          this.adminToolProductService.update(result).subscribe(()=>{
            this.toastr.success("Success changed product")
            this.getData();
          })
        }else{
          this.adminToolProductService.create(result).subscribe(()=>{
            this.toastr.success("Success added product")
            this.getData();
          })
        }
      }
    });
    this.newItem = {
      Id: undefined,
      Name: "",
      Price: undefined,
      Description: "",
      Category: undefined
    };
  }

  deleteProduct(id:string | undefined){
    if(id){
      const dialogRef = this.dialog.open(YesNoDialogComponent, {
        data: { text: 'Do you want to delete this product?' },
        width: '500px', // Set the width of the modal
        panelClass: ['custom-modalbox', 'admin-dialog']
      });
      dialogRef.afterClosed().subscribe((result : ProductViewModel) => {
        if(result){
          this.adminToolProductService.delete(id).subscribe(()=>{
            this.toastr.success("Success deleted product")
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
    const label = ids.length === 1 ? '1 product' : `${ids.length} products`;
    const dialogRef = this.dialog.open(YesNoDialogComponent, {
      data: { text: `Do you want to delete ${label}?` },
      width: '500px', // Set the width of the modal
      panelClass: ['custom-modalbox', 'admin-dialog']
    });
    dialogRef.afterClosed().subscribe((confirmed: boolean) => {
      if(confirmed){
        this.adminToolProductService.deleteMany(ids).subscribe(()=>{
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

