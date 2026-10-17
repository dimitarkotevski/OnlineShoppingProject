import { PaymentInfoViewModel } from "../payment-info-view-model";

export class UserDetails{
    Id?:string;
    FirstName?: string;
    LastName?: string;
    Email?: string;
    Description?:string;
    Username?: string;
    Payment?: PaymentInfoViewModel;
    Address?:string; 
    State?:string;
    City?:string;
    ZipCode?:string;
}