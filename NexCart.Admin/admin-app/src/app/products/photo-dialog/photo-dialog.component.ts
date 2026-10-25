import { Component, Inject, Input, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
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
  selectedFileName = '';
  addPhotoButton: boolean = false;
  saving = false;

  /** Matches the limit the browser can comfortably base64-encode and post. */
  private readonly maxFileSizeBytes = 5 * 1024 * 1024;

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: any,
    private dialogRef: MatDialogRef<PhotoDialogComponent>,
    private productService: AdminToolProductService,
    private toasterService: ToastrService
    ) {
    this.photoUrl = data.data64;
    this.id = data.id;
  }
  ngOnInit() {
  }

  onFileSelected(event: any): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) {
      return;
    }

    if (!file.type.startsWith('image/')) {
      this.toasterService.error('Please choose an image file.');
      input.value = '';
      return;
    }
    if (file.size > this.maxFileSizeBytes) {
      this.toasterService.error('The image must be smaller than 5 MB.');
      input.value = '';
      return;
    }

    this.selectedFile = file;
    this.selectedFileName = file.name;
    this.convertFileToBase64();
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
    if (!base64 || this.saving) {
      return;
    }

    this.saving = true;
    this.productService.addImageToProduct(this.id, base64).subscribe({
      next: () => {
        this.toasterService.success('Added photo to the product');
        // Closing refreshes the grid, so the new thumbnail appears immediately.
        this.dialogRef.close(true);
      },
      error: () => {
        // The failure is reported globally by the error interceptor.
        this.saving = false;
      }
    });
  }
}
