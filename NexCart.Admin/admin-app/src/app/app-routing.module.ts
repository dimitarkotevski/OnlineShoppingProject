import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { adminGuard } from './core/admin.guard';
import { AdminLayoutComponent } from './layout/admin-layout.component';
import { LoginComponent } from './login/login.component';
import { AdminToolProductsComponent } from './products/admin-tool-products.component';
import { AdminToolOrdersComponent } from './orders/admin-tool-orders.component';
import { AdminToolUsersComponent } from './users/admin-tool-users.component';
import { AdminToolCategoriesComponent } from './categories/admin-tool-categories.component';

// Paths are relative to <base href="/">, so 'products' is served at /products on the admin site.
const routes: Routes = [
  { path: 'login', component: LoginComponent },
  {
    path: '',
    component: AdminLayoutComponent,
    canActivate: [adminGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'products' },
      { path: 'products', component: AdminToolProductsComponent },
      { path: 'categories', component: AdminToolCategoriesComponent },
      { path: 'orders', component: AdminToolOrdersComponent },
      { path: 'users', component: AdminToolUsersComponent },
    ]
  },
  { path: '**', redirectTo: 'products' },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
