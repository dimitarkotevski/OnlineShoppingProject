/** Row of the categories grid (NexCart.Business.ViewModels.CategoryGridModel). */
export class CategoryGridModel {
    Id?: string;
    Name?: string;
    /** Active categories are listed in the storefront navbar. */
    Status?: boolean;
    Url?: string;
    ProductCount?: number;
}

/** Create / edit payload (NexCart.Business.ViewModels.CategoryViewModel). */
export class CategoryEditModel {
    Id?: string;
    Name: string = '';
    Status: boolean = true;
}

/** Same limits as CategoryViewModelValidator on the API. */
export const CATEGORY_NAME_MIN = 2;
export const CATEGORY_NAME_MAX = 50;
