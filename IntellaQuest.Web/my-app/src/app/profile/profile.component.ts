import { Component, OnInit } from '@angular/core';
import { UserDetails } from '../models/login/user-details';
import { AuthentificationService } from '../authentification/authentification.service';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { YesNoDialogComponent } from '../customer-components/yes-no-dialog/yes-no-dialog.component';
import { ToastrService } from 'ngx-toastr';
import { AddPaymentComponent } from './add-payment/add-payment.component';
import { PaymentInfoViewModel } from '../models/payment-info-view-model';


@Component({ standalone: false,
  selector: 'app-profile',
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.css']
})
export class ProfileComponent implements OnInit {
  userDetails?: UserDetails;
  constructor(
    private dialog: MatDialog,
    private toastr: ToastrService,
    private authService: AuthentificationService
  ) { }

  ngOnInit(): void {
    if(!this.authService.getLoggedUserId()){
      window.location.replace("/error-404");
    }

    this.authService.getUserDetails(this.authService.getLoggedUserId()).subscribe((response:UserDetails)=>{
      this.userDetails = response;
    });
  }

  addPayment(){
    const dialogRef = this.dialog.open(AddPaymentComponent, {
      data: {  },
      width: '100%', // Set the width of the modal
      panelClass: 'custom-modalbox'
    });

    dialogRef.afterClosed().subscribe((result : PaymentInfoViewModel) => {
      if(result){
        this.authService.addPayment(result).subscribe(()=>{
          this.toastr.success("Success added payment")
          window.location.reload();
        })
      }else{
      }
    });
  }

  deletePayment(userId:string | undefined){
    if(userId){
      const text = "Do you want to delete payment?";
      const dialogRef = this.dialog.open(YesNoDialogComponent, {
        data: { text },
        width: '500px', // Set the width of the modal
        panelClass: 'custom-modalbox'
      });
    
      dialogRef.afterClosed().subscribe((result) => {
        if(result){
          this.authService.deletePayment(userId).subscribe(()=>{
            this.toastr.success("Success deleted payment")
            window.location.reload();
          })
        }else{
        }
      });
    }
  }

}

