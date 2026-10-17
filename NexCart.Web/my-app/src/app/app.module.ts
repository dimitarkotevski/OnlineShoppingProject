import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { HomeComponent } from './home/home.component';
import { NavbarComponent } from './navbar/navbar.component';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { ProductService } from './customer-components/product-components/product.service';
import { JwtHelperService, JWT_OPTIONS  } from '@auth0/angular-jwt';
import { AuthInterceptor } from './utils/auth.interceptor';
import { ErrorInterceptor } from './utils/error.interceptor';
import { ProfileComponent } from './profile/profile.component';
import { FavouriteComponent } from './favourite/favourite.component';
import { SettingsComponent } from './settings/settings.component';
import { ProductDetailComponent } from './customer-components/product-components/product-detail/product-detail.component';
import { FooterComponent } from './footer/footer.component';
import { ErrorPageComponent } from './customer-components/error-page/error-page.component';
import { ByPassSecurityPipe } from './utils/by-pass-security';
import { CategoryProductsComponent } from './category-products/category-products.component';
import { CategoriesNavbarComponent } from './navbar/categories-navbar/categories-navbar.component';
import { RegularNavbarComponent } from './navbar/regular-navbar/regular-navbar.component';
import { LoginComponent } from './authentification/login/login.component';
import { RegisterComponent } from './authentification/register/register.component';
import { CarouselComponent } from './customer-components/carousel/carousel.component';
import { BreadcrumbComponent } from './customer-components/breadcrumb/breadcrumb.component';
import { ToastrModule } from 'ngx-toastr';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { OrderDetailComponent } from './customer-components/order-components/order-detail/order-detail.component';
import { SideFilterComponent } from './customer-components/side-filter/side-filter.component';
import { LoadingDetailComponent } from './customer-components/loading-components/loading-detail/loading-detail.component';
import { AuthentificationService } from './authentification/authentification.service';
import { CartGridComponent } from './customer-components/cart-components/cart-grid/cart-grid.component';
import { LoadingGridComponent } from './customer-components/loading-components/loading-grid/loading-grid.component';
import { SkeletonComponent } from './customer-components/loading-components/skeleton/skeleton.component';
import { OrderGridComponent } from './customer-components/order-components/order-grid/order-grid.component';
import { ProductsGridComponent } from './customer-components/product-components/products-grid/products-grid.component';
import { CartComponent } from './cart/cart.component';
import { OrderComponent } from './order/order.component';
import { OrderService } from './customer-components/order-components/order.service';
import { CategoryService } from './category-products/category.service';
import { AdminToolComponent } from './admin-components/admin-tool/admin-tool.component';
import { AdminToolProductsComponent } from './admin-components/admin-tool-products/admin-tool-products.component';
import { AdminToolOrdersComponent } from './admin-components/admin-tool-orders/admin-tool-orders.component';
import { AdminToolUsersComponent } from './admin-components/admin-tool-users/admin-tool-users.component';
import { DateConverter } from './utils/convert-date';
import { OrderStatusPipe } from './utils/order-pipe';
import { MatDialogModule } from '@angular/material/dialog';
import { AddEditProductDialogComponent } from './admin-components/admin-tool-products/add-edit-product-dialog/add-edit-product-dialog.component';
import { PhotoDialogComponent } from './admin-components/admin-tool-products/photo-dialog/photo-dialog.component';
import { YesNoDialogComponent } from './customer-components/yes-no-dialog/yes-no-dialog.component';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { MatSelectModule } from '@angular/material/select';
import { PaymentStatusPipe } from './utils/payment-status.pipe';
import { AddEditOrderComponent } from './admin-components/admin-tool-orders/add-edit-order/add-edit-order.component';
import { AddEditUserComponent } from './admin-components/admin-tool-users/add-edit-user/add-edit-user.component';
import { MakeOrderConfirmationComponent } from './customer-components/order-components/order-detail/make-order-confirmation/make-order-confirmation.component';
import { AddPaymentComponent } from './profile/add-payment/add-payment.component';
import { NoDataComponent } from './customer-components/no-data/no-data.component';


@NgModule({
  declarations: [
    AppComponent,
    HomeComponent,
    NavbarComponent,
    LoginComponent,
    RegisterComponent,
    ProfileComponent,
    CartGridComponent,
    FavouriteComponent,
    SettingsComponent,
    ProductDetailComponent,
    FooterComponent,
    ErrorPageComponent,
    ByPassSecurityPipe,
    DateConverter,
    OrderStatusPipe,
    CategoryProductsComponent,
    CategoriesNavbarComponent,
    RegularNavbarComponent,
    ProductsGridComponent,
    CarouselComponent,
    BreadcrumbComponent,
    OrderGridComponent,
    OrderDetailComponent,
    SideFilterComponent,
    LoadingDetailComponent,
    LoadingGridComponent,
    SkeletonComponent,
    CartComponent,
    OrderComponent,
    AdminToolComponent,
    AdminToolProductsComponent,
    AdminToolOrdersComponent,
    AdminToolUsersComponent,
    PhotoDialogComponent,
    AddEditProductDialogComponent,
    YesNoDialogComponent,
    PaymentStatusPipe,
    AddEditOrderComponent,
    AddEditUserComponent,
    MakeOrderConfirmationComponent,
    AddPaymentComponent,
    NoDataComponent,
  ],
  imports: [
    BrowserModule,
    HttpClientModule,
    FormsModule,
    AppRoutingModule,
    ToastrModule.forRoot({
      timeOut: 5000,
      positionClass: 'toast-top-right',
      preventDuplicates: false,
    }),
    BrowserAnimationsModule,
    MatDialogModule,
    NgbModule,
    MatSelectModule
  ],
  providers: [
    AuthentificationService,
    ProductService,
    OrderService,
    CategoryService,
    // ErrorInterceptor runs first on the response path so the toastr notification
    // reflects the final error, after AuthInterceptor has cleared an expired session.
    { provide: HTTP_INTERCEPTORS, useClass: ErrorInterceptor, multi: true },
    { provide: HTTP_INTERCEPTORS, useClass: AuthInterceptor, multi: true },
    { provide: JWT_OPTIONS, useValue: JWT_OPTIONS },
    JwtHelperService
  ],
  bootstrap: [
    AppComponent,
  ]
})
export class AppModule { }
