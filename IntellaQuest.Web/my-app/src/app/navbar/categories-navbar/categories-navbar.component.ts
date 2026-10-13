import { Component, Input, OnInit } from '@angular/core';
import { CategoryViewModel } from 'src/app/models/category';
import { ResponseListModel } from 'src/app/models/response';
import { CategoryService } from 'src/app/category-products/category.service';

@Component({ standalone: false,
  selector: 'app-categories-navbar',
  templateUrl: './categories-navbar.component.html',
  styleUrls: ['./categories-navbar.component.css']
})
export class CategoriesNavbarComponent implements OnInit {

  @Input() categoryNavBar?: boolean;
  
  categories?: CategoryViewModel[] | null | undefined;

  constructor(private categoryService: CategoryService) { }

  ngOnInit(): void {
    this.categoryService.getAllCategories().subscribe((result:ResponseListModel<CategoryViewModel>) => {
      this.categories = result.Items;
    })
  }
  isLinkActive(url: string | undefined | null): boolean {
    if (!url) {
      return false;
    }
    const currentUrl = window.location.pathname;
    return currentUrl.endsWith(url);// Check if the current URL ends with the specified category URL
  }

}

