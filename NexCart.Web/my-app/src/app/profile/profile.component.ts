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

  /** Card number with everything but the last four digits hidden. */
  get maskedCardNumber(): string {
    const digits = (this.userDetails?.Payment?.CardNumber ?? '').replace(/\D/g, '');
    return digits.length >= 4
      ? `•••• •••• •••• ${digits.slice(-4)}`
      : '•••• •••• •••• ••••';
  }

  /** Expiration formatted as MM/YY rather than the value the API returns. */
  get cardExpiry(): string {
    const value = this.userDetails?.Payment?.ExpirationDate;
    if (!value) {
      return '';
    }

    // The API already returns the expiry as "M/YY" (e.g. "5/26"); parsing that with
    // `new Date()` would treat it as a calendar date, so format it directly instead.
    const short = /^(\d{1,2})\s*\/\s*(\d{2}|\d{4})$/.exec(value.trim());
    if (short) {
      const month = short[1].padStart(2, '0');
      return `${month}/${short[2].slice(-2)}`;
    }

    // Fallback for ISO timestamps: read the UTC parts, since the value is stored in UTC.
    const date = new Date(value);
    if (isNaN(date.getTime())) {
      return value;
    }
    const month = String(date.getUTCMonth() + 1).padStart(2, '0');
    return `${month}/${String(date.getUTCFullYear()).slice(-2)}`;
  }

  /** Card network inferred from the stored number's leading digits (IIN). */
  get cardBrand(): string {
    const digits = (this.userDetails?.Payment?.CardNumber ?? '').replace(/\D/g, '');
    if (/^4/.test(digits)) {
      return 'Visa';
    }
    if (/^(5[1-5]|2[2-7])/.test(digits)) {
      return 'Mastercard';
    }
    if (/^3[47]/.test(digits)) {
      return 'American Express';
    }
    if (/^6(?:011|5|4[4-9])/.test(digits)) {
      return 'Discover';
    }
    if (/^3(?:0[0-5]|[68])/.test(digits)) {
      return 'Diners Club';
    }
    if (/^35/.test(digits)) {
      return 'JCB';
    }
    return 'Card';
  }

  /** Font Awesome classes for the detected brand's logo. */
  get cardBrandIcon(): string {
    switch (this.cardBrand) {
      case 'Visa':
        return 'fa-brands fa-cc-visa';
      case 'Mastercard':
        return 'fa-brands fa-cc-mastercard';
      case 'American Express':
        return 'fa-brands fa-cc-amex';
      case 'Discover':
        return 'fa-brands fa-cc-discover';
      case 'Diners Club':
        return 'fa-brands fa-cc-diners-club';
      case 'JCB':
        return 'fa-brands fa-cc-jcb';
      default:
        return 'fa-solid fa-credit-card';
    }
  }

  /** Whole days until the card expires, or null when the date is unavailable. */
  private get daysToExpiry(): number | null {
    const value = this.userDetails?.Payment?.ExpirationDate;
    if (!value) {
      return null;
    }

    let month: number;
    let year: number;
    const short = /^(\d{1,2})\s*\/\s*(\d{2}|\d{4})$/.exec(value.trim());
    if (short) {
      month = parseInt(short[1], 10);
      year = parseInt(short[2], 10);
      if (year < 100) {
        year += 2000;
      }
    } else {
      const date = new Date(value);
      if (isNaN(date.getTime())) {
        return null;
      }
      month = date.getUTCMonth() + 1;
      year = date.getUTCFullYear();
    }

    // A card stays valid through the last day of its expiry month.
    const endOfMonth = new Date(Date.UTC(year, month, 0));
    const now = new Date();
    const todayUtc = Date.UTC(now.getFullYear(), now.getMonth(), now.getDate());
    return Math.round((endOfMonth.getTime() - todayUtc) / 86400000);
  }

  /** Derived card health used to colour the status pill. */
  get cardStatus(): 'valid' | 'expiring' | 'expired' {
    const days = this.daysToExpiry;
    if (days === null) {
      return 'valid';
    }
    if (days < 0) {
      return 'expired';
    }
    return days <= 60 ? 'expiring' : 'valid';
  }

  get cardStatusLabel(): string {
    switch (this.cardStatus) {
      case 'expired':
        return 'Expired';
      case 'expiring':
        return 'Expires soon';
      default:
        return 'Active';
    }
  }

  addPayment(){
    const dialogRef = this.dialog.open(AddPaymentComponent, {
      width: '760px',
      maxWidth: '95vw',
      panelClass: ['custom-modalbox', 'payment-modalbox']
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

