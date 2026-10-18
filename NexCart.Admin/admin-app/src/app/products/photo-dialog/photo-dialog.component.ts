import { Component, Inject, Input, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';
import { ToastrService } from 'ngx-toastr';
import { AdminToolProductService } from '../admin-tool-product-service/admin-tool-product.service';

@Component({ standalone: false,
  selector: 'app-photo-dialog',
  templateUrl: './photo-dialog.component.html',
  styleUrls: ['./photo-dialog.component.css'],
})
export class PhotoDialogComponent implements OnInit {

  @Input() photoUrl: string;
  @Input() id: string;

  selectedFile?: File;
  selectedFileBase64: string = '';
  addPhotoButton: boolean = false;

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: any,
    private productService: AdminToolProductService,
    private toasterService: ToastrService
    ) {
    this.photoUrl = data.data64;
    this.id = data.id;
  }
  ngOnInit() {
  }

  onFileSelected(event: any): void {
    this.selectedFile = event.target.files[0];
    if (this.selectedFile) {
      this.convertFileToBase64();
    }
  }

  convertFileToBase64(): void {
    if (this.selectedFile) {
      const reader = new FileReader();
      reader.onload = (e: any) => {
        this.selectedFileBase64 = e.target.result;
        this.selectedFileBase64 = this.selectedFileBase64.replace(/^data:image\/\w+;base64,/, '');
        this.photoUrl = this.selectedFileBase64;
        this.addPhotoButton = true;
      };
      reader.readAsDataURL(this.selectedFile);
    }
  }

  addPhoto(base64:string){
    this.productService.addImageToProduct(this.id,base64).subscribe(()=>{
      this.toasterService.success("Added photo to the product")
    })
  }
}

