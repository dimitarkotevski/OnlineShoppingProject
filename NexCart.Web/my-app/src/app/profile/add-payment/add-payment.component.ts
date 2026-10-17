import { Component, HostListener, OnInit } from '@angular/core';
import { MatDialogRef } from '@angular/material/dialog';
import { PaymentInfoViewModel } from 'src/app/models/payment-info-view-model';

@Component({ standalone: false,
  selector: 'app-add-payment',
  templateUrl: './add-payment.component.html',
  styleUrls: ['./add-payment.component.css']
})
export class AddPaymentComponent implements OnInit {

  model: PaymentInfoViewModel;

  firstFourDigit: string = "";
  secondFourDigit: string = "";
  thirdFourDigit: string = "";
  fourthFourDigit: string = "";

  /** Expiry date typed on the keyboard as MM / YY. */
  expireMonth: string = "";
  expireYear: string = "";

  /** Calendar panel state. */
  expiryPickerOpen: boolean = false;
  /** Year currently shown inside the calendar panel (4 digits). */
  expiryViewYear: number = new Date().getFullYear();

  readonly monthLabels: string[] = [
    'Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun',
    'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'
  ];

  /** True while the CVC field is focused, so the card preview shows its back. */
  cvcFocused: boolean = false;

  /** Validation results; only rendered once the first save has been attempted. */
  errorMessages: string[] = [];
  submitted: boolean = false;
  /** True once the user has interacted with the expiry field. */
  expiryTouched: boolean = false;
  /** Per-field "touched" flags so errors appear after blur instead of while typing. */
  cardHolderTouched: boolean = false;
  cardNumberTouched: boolean = false;
  securityCodeTouched: boolean = false;

  constructor(private dialogRef: MatDialogRef<AddPaymentComponent>) {
    this.model = new PaymentInfoViewModel();
  }

  ngOnInit(): void {
  }

  /** True when the typed expiry is a real month and year, so nothing invalid reaches the API. */
  get expiryValid(): boolean {
    if (!this.expireMonth || !this.expireYear) {
      return false;
    }
    const month = Number(this.expireMonth);
    const year = Number(this.expireYear);
    return month >= 1 && month <= 12 && year >= 0;
  }

  /** Expiry shown on the card preview, e.g. "05/28". */
  get dateString(): string {
    if (!this.expireMonth && !this.expireYear) {
      return '';
    }
    const month = this.expireMonth ? this.expireMonth.padStart(2, '0') : '';
    return `${month}/${this.expireYear}`;
  }

  /** Keeps the month box numeric and jumps to the year once a valid month is in. */
  onExpireMonthChange(value: string){
    const digits = (value ?? '').replace(/\D/g, '').slice(0, 2);
    // Always write the sanitized value back: the inputs are one-way bound, so the
    // model would otherwise stay empty while the user types plain digits.
    this.expireMonth = digits;
    const month = Number(digits);
    if (digits.length === 2 && month >= 1 && month <= 12) {
      this.focusField('expireYear');
    }
  }

  /** Keeps the year box numeric and keeps the calendar panel in sync. */
  onExpireYearChange(value: string){
    const digits = (value ?? '').replace(/\D/g, '').slice(0, 2);
    this.expireYear = digits;
    if (digits.length === 2) {
      this.expiryViewYear = this.toFullYear(digits);
    }
  }

  /** Pads single digits (5 -> 05) so the saved date is never ambiguous. */
  onExpireBlur(){
    this.expiryTouched = true;
    this.normalizeExpiry();
  }

  /** Pads single-digit month/year so both are always two characters. */
  private normalizeExpiry(){
    if (this.expireMonth.length === 1) {
      this.expireMonth = this.expireMonth.padStart(2, '0');
    }
    if (this.expireYear.length === 1) {
      this.expireYear = this.expireYear.padStart(2, '0');
    }
    if (this.expireYear) {
      this.expiryViewYear = this.toFullYear(this.expireYear);
    }
  }

  toggleExpiryPicker(event: Event){
    event.stopPropagation();
    if (this.expireYear) {
      this.expiryViewYear = this.toFullYear(this.expireYear);
    }
    this.expiryPickerOpen = !this.expiryPickerOpen;
  }

  shiftExpiryYear(delta: number){
    this.expiryViewYear += delta;
  }

  isExpiryMonthSelected(index: number): boolean {
    return this.expireMonth === String(index + 1).padStart(2, '0')
      && this.toFullYear(this.expireYear) === this.expiryViewYear;
  }

  selectExpiryMonth(index: number){
    this.expireMonth = String(index + 1).padStart(2, '0');
    this.expireYear = String(this.expiryViewYear).slice(-2);
    this.expiryPickerOpen = false;
  }

  /** Closes the calendar when the user clicks anywhere else. */
  @HostListener('document:click')
  closeExpiryPicker(){
    this.expiryPickerOpen = false;
  }

  /** Expands a two-digit year to a full year near "today" (26 -> 2026). */
  private toFullYear(twoDigits: string): number {
    const value = Number(twoDigits);
    if (!twoDigits || isNaN(value)) {
      return this.expiryViewYear;
    }
    return 2000 + value;
  }

  private focusField(id: string){
    // Let the value propagate before moving focus.
    setTimeout(() => document.getElementById(id)?.focus());
  }

  /** True when the value is exactly four digits. */
  isDigitGroup(value: string): boolean {
    return /^\d{4}$/.test(value ?? '');
  }

  /** CVC is 3 digits (Visa/Mastercard) or 4 (Amex). */
  isValidSecurityCode(): boolean {
    return /^\d{3,4}$/.test((this.model.SecurityCode ?? '').trim());
  }

  /** The four card-number input groups, in order. */
  private get cardNumberGroups(): string[] {
    return [this.firstFourDigit, this.secondFourDigit, this.thirdFourDigit, this.fourthFourDigit];
  }

  get cardHolderValid(): boolean {
    return (this.model.CardHolder ?? '').trim() !== '';
  }

  get cardNumberValid(): boolean {
    return this.cardNumberGroups.every(group => this.isDigitGroup(group));
  }

  get securityCodeValid(): boolean {
    return this.isValidSecurityCode();
  }

  /** Whether each field's inline error should currently be shown. */
  get showCardHolderError(): boolean {
    return (this.cardHolderTouched || this.submitted) && !this.cardHolderValid;
  }

  get showCardNumberError(): boolean {
    if (!this.cardNumberTouched && !this.submitted) {
      return false;
    }
    // Don't nag while the user is still tabbing through the untouched inputs.
    if (!this.submitted && this.cardNumberGroups.every(group => !group)) {
      return false;
    }
    return !this.cardNumberValid;
  }

  get showSecurityCodeError(): boolean {
    return (this.securityCodeTouched || this.submitted) && !this.securityCodeValid;
  }

  get showExpiryError(): boolean {
    return (this.expiryTouched || this.submitted) && !this.expiryValid;
  }

  get showExpiryExpiredError(): boolean {
    return (this.expiryTouched || this.submitted) && this.expiryValid && this.isExpired();
  }

  /** Cards stay valid through the last day of their expiry month. */
  private isExpired(): boolean {
    if (!this.expiryValid) {
      return false;
    }
    const month = Number(this.expireMonth);
    const year = 2000 + Number(this.expireYear);
    // First day of the month after expiry, in UTC, compared against "today" in UTC.
    const endOfExpiryMonth = Date.UTC(year, month, 1);
    const now = new Date();
    const todayUtc = Date.UTC(now.getFullYear(), now.getMonth(), now.getDate());
    return endOfExpiryMonth <= todayUtc;
  }

  /** Validates the form and only closes the dialog (returning the model) when it passes. */
  save(): void {
    this.submitted = true;
    this.expiryTouched = true;
    this.normalizeExpiry();
    this.errorMessages = [];

    if ((this.model.CardHolder ?? '').trim() === '') {
      this.errorMessages.push('Card holder name is required.');
    }

    const groups = [this.firstFourDigit, this.secondFourDigit, this.thirdFourDigit, this.fourthFourDigit];
    if (groups.some(group => !this.isDigitGroup(group))) {
      this.errorMessages.push('Card number must be 16 digits (four groups of four).');
    }

    if (!this.expiryValid) {
      this.errorMessages.push('Expiry date is required (MM / YY).');
    } else if (this.isExpired()) {
      this.errorMessages.push('This card has already expired.');
    }

    if (!this.isValidSecurityCode()) {
      this.errorMessages.push('Security code must be 3 or 4 digits.');
    }

    if (this.errorMessages.length > 0) {
      return;
    }

    this.makeCardNumber();
    this.dialogRef.close(this.model);
  }

  makeCardNumber(){
    this.model.CardNumber = this.firstFourDigit+this.secondFourDigit+this.thirdFourDigit+this.fourthFourDigit
    // The backend stores this as a UTC DateTime and reads its .Month/.Year without any
    // timezone conversion, so the month/year must be encoded in UTC. Building the date
    // from local parts (or even local noon) can roll back a day for some offsets and
    // save the previous month; UTC midnight on the 1st is exact in every timezone.
    const month = Number(this.expireMonth);
    const year = Number(this.expireYear);
    if (this.expiryValid) {
      this.model.ExpirationDate = new Date(Date.UTC(2000 + year, month - 1, 1)).toISOString();
    }
  }
}
