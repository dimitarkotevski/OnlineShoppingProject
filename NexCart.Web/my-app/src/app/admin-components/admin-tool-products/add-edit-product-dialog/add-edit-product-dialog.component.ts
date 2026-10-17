import { Component, Inject, Input, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';
import { CategoryService } from 'src/app/category-products/category.service';
import { CategoryViewModel } from 'src/app/models/category';
import { ProductViewModel } from 'src/app/models/product-view-model';
import { ResponseModel } from '../../admin-tool-models/admin-tool-response-model';
import { LookupViewModel } from 'src/app/models/lookup-view-model';

@Component({ standalone: false,
  selector: 'app-add-edit-product-dialog',
  templateUrl: './add-edit-product-dialog.component.html',
  styleUrls: ['./add-edit-product-dialog.component.css']
})
export class AddEditProductDialogComponent implements OnInit {

  @Input() model: ProductViewModel
  title: string = ""

  categories?: CategoryViewModel[]

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: any,
    private categoryService: CategoryService
    ) {
    // Edit a copy so cancelling leaves the grid row untouched.
    this.model = { ...data.product };
  }

  ngOnInit(): void {
    if(this.model.Id){
      this.title = "Edit"
    }else{
      this.title = "Add"
    }

    this.categoryService.getAllCategories().subscribe((response: ResponseModel<CategoryViewModel>)=>{
      if(response.Items){
        this.categories = response.Items;
      }
    })
  }

  /** The category select binds to the id string; keep the object in sync for the save handler. */
  get selectedCategoryId(): string | undefined {
    return this.model?.Category?.Id;
  }

  set selectedCategoryId(value: string | undefined) {
    if (!this.model) return;
    this.model.Category = value ? { Id: value } : undefined;
  }

}

