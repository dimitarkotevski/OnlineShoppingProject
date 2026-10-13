import { Component, OnInit } from '@angular/core';
import { PaymentInfoViewModel } from 'src/app/models/payment-info-view-model';

@Component({ standalone: false,
  selector: 'app-add-payment',
  templateUrl: './add-payment.component.html',
  styleUrls: ['./add-payment.component.css']
})
export class AddPaymentComponent implements OnInit {

  model: PaymentInfoViewModel

  firstFourDigit: string="";
  secondFourDigit: string="";
  thirdFourDigit: string="";
  fourthFourDigit: string="";

  dateString: string = "";

  constructor() { 
    this.model = new PaymentInfoViewModel()
  }

  ngOnInit(): void {
  }

  makeCardNumber(){
    this.model.CardNumber = this.firstFourDigit+this.secondFourDigit+this.thirdFourDigit+this.fourthFourDigit
    // `type="month"` inputs are only "YYYY-MM", and an empty value yields an
    // Invalid Date whose toISOString() throws. Guard against that.
    if(this.dateString){
      const parsed = new Date(this.dateString);
      if(!isNaN(parsed.getTime())){
        this.model.ExpirationDate = parsed.toISOString();
      }
    }
  }
}

