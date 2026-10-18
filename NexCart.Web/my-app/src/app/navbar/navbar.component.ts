import { AfterViewInit, Component, ElementRef, EventEmitter, Input, OnDestroy, OnInit, Output } from '@angular/core';
import { AuthentificationService } from '../authentification/authentification.service';
import { CategoryService } from '../category-products/category.service';
import { ResponseListModel } from '../models/response';
import { CategoryViewModel } from '../models/category';
import { ToastrService } from 'ngx-toastr';


@Component({ standalone: false,
  selector: 'app-navbar',
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.css']
})
export class NavbarComponent implements OnInit, AfterViewInit, OnDestroy {

  @Input() isUserAuthenticated?: boolean;
  @Output() refreshState = new EventEmitter<any>();

  categories?: CategoryViewModel[] | null | undefined;
  username?: string;
  id?: string | null;

  /** Keeps `--navbar-height` in sync with the rendered header size. */
  private headerResizeObserver?: ResizeObserver;

  constructor(
      private authService: AuthentificationService,
      private categoryService: CategoryService,
      private toastr: ToastrService,
      private elementRef: ElementRef<HTMLElement>,
    ){ }

  ngOnInit(): void {
    this.id = this.authService.getLoggedUserId();
    this.username = this.authService.getLoggedUsername();
  }

  ngAfterViewInit(): void {
    this.publishHeaderHeight();
  }

  ngOnDestroy(): void {
    this.headerResizeObserver?.disconnect();
  }

  /**
   * Publishes the header height as the `--navbar-height` CSS variable so overlays
   * (such as the toast notifications) can be positioned just below the navbar
   * instead of overlapping it. It follows the header as the categories load, the
   * layout wraps, or the category bar is toggled.
   */
  private publishHeaderHeight(): void {
    const header = this.elementRef.nativeElement.querySelector('header') ?? this.elementRef.nativeElement;

    const sync = () => {
      document.documentElement.style.setProperty('--navbar-height', `${header.offsetHeight}px`);
    };

    sync();

    if (typeof ResizeObserver !== 'undefined') {
      this.headerResizeObserver = new ResizeObserver(sync);
      this.headerResizeObserver.observe(header);
    }
  }

  logout(){
    this.authService.logout();
    this.refreshState.emit();
    this.toastr.success("Success logout!");
    window.location.replace('/')
  }
}
