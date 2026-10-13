import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { CategoryViewModel } from 'src/app/models/category';
import { AuthentificationService } from 'src/app/authentification/authentification.service';
import { CategoryService } from 'src/app/category-products/category.service';

@Component({ standalone: false,
  selector: 'app-regular-navbar',
  templateUrl: './regular-navbar.component.html',
  styleUrls: ['./regular-navbar.component.css']
})
export class RegularNavbarComponent implements OnInit {

  @Input() isUserAuthenticated?: boolean;
  @Input() refreshState: EventEmitter<any>;
  @Output() changeCategoryNavBar = new EventEmitter<any>();
  @Input() categoryNavBar?: boolean ;
  
  categories?: CategoryViewModel[] | null | undefined;
  username?: string;
  canAccessToAdminTool: boolean = false;
  amount: any;
  id?: string | null;

  constructor(
    private authService: AuthentificationService,
    private categoryService: CategoryService,
  ) { 
    this.refreshState = new EventEmitter();
  }

  ngOnInit(): void {
    this.username = this.authService.getLoggedUsername();
    this.id = this.authService.getLoggedUserId();

    if(this.authService.getRole() == 'admin'){
      this.canAccessToAdminTool = true;
    }

    this.authService.getAmountMoneyOfUser(this.id)?.subscribe(res=>{
      if(res === 0){
        this.amount = null;
      }else{
        this.amount ="( $"+res+")";
      }
    })
  }

  logout(){
    this.authService.logout();
    this.refreshState.emit();
    window.location.replace('/')
  }

  disableNavbar(){
    this.changeCategoryNavBar.emit();
  }
}

