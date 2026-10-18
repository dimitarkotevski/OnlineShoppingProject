import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import {
  CATEGORY_NAME_MAX,
  CATEGORY_NAME_MIN,
  CategoryEditModel,
  CategoryGridModel,
} from 'src/app/models/category-grid';

@Component({ standalone: false,
  selector: 'app-add-edit-category',
  templateUrl: './add-edit-category.component.html',
  styleUrls: ['./add-edit-category.component.css']
})
export class AddEditCategoryComponent {

  readonly nameMin = CATEGORY_NAME_MIN;
  readonly nameMax = CATEGORY_NAME_MAX;
  /** At least one letter or digit, since the name becomes the category's URL slug. */
  readonly namePattern = '.*[A-Za-z0-9\\u00C0-\\uFFFF].*';

  model: CategoryEditModel;

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: { category?: CategoryGridModel },
    private dialogRef: MatDialogRef<AddEditCategoryComponent>,
  ) {
    // Edit a copy so cancelling leaves the grid row untouched.
    const category = data?.category;
    this.model = {
      Id: category?.Id,
      Name: category?.Name || '',
      Status: category?.Status ?? true,
    };
  }

  get title(): string {
    return this.model.Id ? 'Edit' : 'Add';
  }

  /** Whitespace-only names pass `required`, so they are checked separately. */
  get isBlank(): boolean {
    return this.model.Name.trim().length === 0;
  }

  save(): void {
    this.model.Name = this.model.Name.trim();
    this.dialogRef.close(this.model);
  }
}
