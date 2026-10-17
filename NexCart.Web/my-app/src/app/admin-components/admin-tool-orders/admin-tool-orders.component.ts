import { Component, OnInit } from '@angular/core';
import { AdminToolOrderService } from './admin-tool-order-service/admin-tool-order.service';
import { ResponseModel } from '../admin-tool-models/admin-tool-response-model';
import { OrderGridViewModel } from 'src/app/models/order-grid';
import { RequestModel } from '../admin-tool-models/admin-tool-request-model';
import { OrderViewModel } from '../admin-tool-models/order-view-model';
import { YesNoDialogComponent } from 'src/app/customer-components/yes-no-dialog/yes-no-dialog.component';
import { ToastrService } from 'ngx-toastr';
import { MatDialog } from '@angular/material/dialog';
import { AddEditOrderComponent } from './add-edit-order/add-edit-order.component';
import { Router } from '@angular/router';
import { AuthentificationService } from 'src/app/authentification/authentification.service';
import { OrderStatus } from 'src/app/models/enum/order-status';

@Component({ standalone: false,
  selector: 'app-admin-tool-orders',
  templateUrl: './admin-tool-orders.component.html',
  styleUrls: ['./admin-tool-orders.component.css']
})
export class AdminToolOrdersComponent implements OnInit {

  request: RequestModel;
  response: ResponseModel<OrderGridViewModel>
  newItem: OrderViewModel;

  pagination: number[] = [5 , 10, 20, 50, 100]

  constructor(
    private router: Router,
    private dialog: MatDialog,
    private toastr: ToastrService,
    private authService: AuthentificationService,
    private adminToolOrderService: AdminToolOrderService,
    ) { 
      this.request = new RequestModel();
      this.response = new ResponseModel<OrderGridViewModel>()
      this.newItem = new OrderViewModel();
    }

  ngOnInit(): void {

    if(!this.authService.isAdmin()){
      this.router.navigate(['error-404']);
      return;
    }

    this.request = {
      PageNeeded: 1,
      Size: 10,
    }

    this.getData();
  }

  addEditOrder(order?: OrderGridViewModel){
    const dialogRef = this.dialog.open(AddEditOrderComponent, {
      data: { order },
      width: '500px', // Set the width of the modal
      panelClass: ['custom-modalbox', 'admin-dialog']
    });
  
    dialogRef.afterClosed().subscribe((result : OrderViewModel) => {
      // Cancel / Esc / backdrop click close without a model: nothing to save.
      if(result){
        if(result.Id){
          this.adminToolOrderService.update(result).subscribe(()=>{
            this.toastr.success("Success changed order")
            this.getData();
          })
        }else{
          this.adminToolOrderService.create(result).subscribe(()=>{
            this.toastr.success("Success added order")
            this.getData();
          })
        }
      }
    });
    //No need admin to add orders
    this.newItem = {
      Id: undefined,
    };
  }

  deleteOrder(id:string | undefined){
    if(id){
      const dialogRef = this.dialog.open(YesNoDialogComponent, {
        data: { text: 'Do you want to delete this order?' },
        width: '500px', // Set the width of the modal
        panelClass: ['custom-modalbox', 'admin-dialog']
      });
      dialogRef.afterClosed().subscribe((result : OrderViewModel) => {
        if(result){
          this.adminToolOrderService.delete(id).subscribe(()=>{
            this.toastr.success("Success deleted order")
            this.getData()
          })
        }
      });
    }
  }

  get totalPages(): number {
    return this.response.Size > 0 ? Math.ceil(this.response.TotalItems / this.response.Size) : 0;
  }

  get pageTotalAmount(): number {
    return (this.response.Items || []).reduce((sum, order) => sum + (order.TotalAmount || 0), 0);
  }

  statusPillClass(status?: OrderStatus): string {
    switch (status) {
      case OrderStatus.Canceled: return 'pill--danger';
      case OrderStatus.OnHold: return 'pill--warning';
      case OrderStatus.PendingPayment: return 'pill--coral';
      case OrderStatus.Completed: return 'pill--success';
      default: return '';
    }
  }

  getData(){
    this.adminToolOrderService.getTable(this.request).subscribe((response: ResponseModel<OrderGridViewModel>)=>{
      this.response = response
    }, ()=>{
      // The failure is reported globally by the error interceptor.
    })
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

