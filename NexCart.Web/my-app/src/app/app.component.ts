import { Component, OnInit } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { AuthentificationService } from './authentification/authentification.service';
import { FlashMessageService } from './utils/flash-message.service';

@Component({ standalone: false,
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  isUserAuthenticated: boolean = false;
  constructor(
    private authService: AuthentificationService,
    private toastr: ToastrService,
    private flashMessage: FlashMessageService
  ) {
  }
  ngOnInit(): void {
    this.refreshState();
    this.showPendingFlashMessage();
  }
  refreshState(){
    this.isUserAuthenticated = this.authService.isAuthenticated();
  }
  /** Displays a message queued before the last full page reload (login/registration). */
  private showPendingFlashMessage(): void {
    const flash = this.flashMessage.consume();
    if (!flash) {
      return;
    }
    switch (flash.type) {
      case 'error':
        this.toastr.error(flash.message);
        break;
      case 'warning':
        this.toastr.warning(flash.message);
        break;
      case 'info':
        this.toastr.info(flash.message);
        break;
      default:
        this.toastr.success(flash.message);
    }
  }
}
