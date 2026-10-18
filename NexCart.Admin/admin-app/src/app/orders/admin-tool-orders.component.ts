import { Component, OnInit } from '@angular/core';
import { AdminToolOrderService } from './admin-tool-order-service/admin-tool-order.service';
import { ResponseModel } from 'src/app/models/admin-tool-response-model';
import { OrderGridViewModel } from 'src/app/models/order-grid';
import { RequestModel } from 'src/app/models/admin-tool-request-model';
import { OrderViewModel } from 'src/app/models/order-view-model';
import { YesNoDialogComponent } from 'src/app/shared/yes-no-dialog/yes-no-dialog.component';
import { GridSelection } from 'src/app/shared/grid-selection';
import { ToastrService } from 'ngx-toastr';
import { MatDialog } from '@angular/material/dialog';
import { AddEditOrderComponent } from './add-edit-order/add-edit-order.component';
import { OrderStatus } from 'src/app/models/enum/order-status';
import { PaymentType } from 'src/app/models/enum/payment-type';
import { FilterableGrid } from 'src/app/shared/filterable-grid';

@Component({ standalone: false,
  selector: 'app-admin-tool-orders',
  templateUrl: './admin-tool-orders.component.html',
  styleUrls: ['./admin-tool-orders.component.css']
})
export class AdminToolOrdersComponent extends FilterableGrid implements OnInit {

  response: ResponseModel<OrderGridViewModel>
  newItem: OrderViewModel;
  /** Rows ticked for bulk delete (current page only). */
  selection = new GridSelection();

  pagination: number[] = [5 , 10, 20, 50, 100]

  /** Options for the status and payment filters (values match the API enums). */
  readonly statusOptions = [
    { value: OrderStatus.PendingPayment, label: 'Pending payment' },
    { value: OrderStatus.OnHold, label: 'On hold' },
    { value: OrderStatus.Completed, label: 'Completed' },
    { value: OrderStatus.Canceled, label: 'Canceled' },
  ];
  readonly paymentOptions = [
    { value: PaymentType.Cash, label: 'Cash' },
    { value: PaymentType.Card, label: 'Card' },
  ];

  constructor(
    private dialog: MatDialog,
    private toastr: ToastrService,
    private adminToolOrderService: AdminToolOrderService,
    ) {
      super();
      this.response = new ResponseModel<OrderGridViewModel>()
      this.newItem = new OrderViewModel();
    }

  ngOnInit(): void {

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
    this.adminToolOrderService.getTable(this.apiRequest()).subscribe((response: ResponseModel<OrderGridViewModel>)=>{
      this.response = response
      this.selection.clear();
    }, ()=>{
      // The failure is reported globally by the error interceptor.
    })
  }

  /** Deletes every ticked row on the current page after a single confirmation. */
  deleteSelected(){
    const ids = this.selection.selectedIds;
    if(ids.length === 0){
      return;
    }
    const label = ids.length === 1 ? '1 order' : `${ids.length} orders`;
    const dialogRef = this.dialog.open(YesNoDialogComponent, {
      data: { text: `Do you want to delete ${label}?` },
      width: '500px', // Set the width of the modal
      panelClass: ['custom-modalbox', 'admin-dialog']
    });
    dialogRef.afterClosed().subscribe((confirmed: boolean) => {
      if(confirmed){
        this.adminToolOrderService.deleteMany(ids).subscribe(()=>{
          this.toastr.success(`Deleted ${label}`);
          // Step back a page when everything on the last page was removed.
          const page = this.request.PageNeeded || 1;
          if(ids.length >= (this.response.Items?.length || 0) && page > 1){
            this.request.PageNeeded = page - 1;
          }
          this.getData();
        })
      }
    });
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

