import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { HTTP_INTERCEPTORS, provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatTooltipModule } from '@angular/material/tooltip';
import { NgbPaginationModule } from '@ng-bootstrap/ng-bootstrap';
import { ToastrModule } from 'ngx-toastr';
import { JWT_OPTIONS, JwtHelperService } from '@auth0/angular-jwt';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { AuthInterceptor } from './core/auth.interceptor';
import { ErrorInterceptor } from './core/error.interceptor';
import { AdminLayoutComponent } from './layout/admin-layout.component';
import { LoginComponent } from './login/login.component';

import { AdminToolProductsComponent } from './products/admin-tool-products.component';
import { AddEditProductDialogComponent } from './products/add-edit-product-dialog/add-edit-product-dialog.component';
import { PhotoDialogComponent } from './products/photo-dialog/photo-dialog.component';
import { AdminToolOrdersComponent } from './orders/admin-tool-orders.component';
import { AddEditOrderComponent } from './orders/add-edit-order/add-edit-order.component';
import { AdminToolUsersComponent } from './users/admin-tool-users.component';
import { AddEditUserComponent } from './users/add-edit-user/add-edit-user.component';

import { YesNoDialogComponent } from './shared/yes-no-dialog/yes-no-dialog.component';
import { NoDataComponent } from './shared/no-data/no-data.component';
import { ByPassSecurityPipe } from './shared/pipes/by-pass-security';
import { DateConverter } from './shared/pipes/convert-date';
import { OrderStatusPipe } from './shared/pipes/order-pipe';
import { PaymentStatusPipe } from './shared/pipes/payment-status.pipe';

@NgModule({
  declarations: [
    AppComponent,
    AdminLayoutComponent,
    LoginComponent,
    AdminToolProductsComponent,
    AddEditProductDialogComponent,
    PhotoDialogComponent,
    AdminToolOrdersComponent,
    AddEditOrderComponent,
    AdminToolUsersComponent,
    AddEditUserComponent,
    YesNoDialogComponent,
    NoDataComponent,
    ByPassSecurityPipe,
    DateConverter,
    OrderStatusPipe,
    PaymentStatusPipe,
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    FormsModule,
    AppRoutingModule,
    MatDialogModule,
    MatButtonModule,
    MatInputModule,
    MatFormFieldModule,
    MatSelectModule,
    MatTooltipModule,
    NgbPaginationModule,
    ToastrModule.forRoot({
      timeOut: 5000,
      positionClass: 'toast-top-right',
      preventDuplicates: false,
    }),
  ],
  providers: [
    provideHttpClient(withInterceptorsFromDi()),
    // ErrorInterceptor runs first on the response path so the toastr notification
    // reflects the final error, after AuthInterceptor has cleared an expired session.
    { provide: HTTP_INTERCEPTORS, useClass: ErrorInterceptor, multi: true },
    { provide: HTTP_INTERCEPTORS, useClass: AuthInterceptor, multi: true },
    { provide: JWT_OPTIONS, useValue: JWT_OPTIONS },
    JwtHelperService,
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
