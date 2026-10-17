import {
  AfterViewInit,
  ChangeDetectorRef,
  Component,
  ElementRef,
  Input,
  OnDestroy,
  OnInit,
  ViewChild
} from '@angular/core';
import { CategoryViewModel } from 'src/app/models/category';
import { ResponseListModel } from 'src/app/models/response';
import { CategoryService } from 'src/app/category-products/category.service';

@Component({
  standalone: false,
  selector: 'app-categories-navbar',
  templateUrl: './categories-navbar.component.html',
  styleUrls: ['./categories-navbar.component.css']
})
export class CategoriesNavbarComponent implements OnInit, AfterViewInit, OnDestroy {

  @Input() categoryNavBar?: boolean;

  categories?: CategoryViewModel[] | null | undefined;

  /** True until the category list has been fetched (drives the skeleton placeholder). */
  isCategoriesLoading = true;

  /** How many categories fit on the single line. The remainder go into the "More" dropdown. */
  visibleCount = 0;
  overflowCategories: CategoryViewModel[] = [];

  @ViewChild('catList') catList?: ElementRef<HTMLElement>;
  @ViewChild('measureList') measureList?: ElementRef<HTMLElement>;
  @ViewChild('measureMore') measureMore?: ElementRef<HTMLElement>;

  /** Horizontal gap between items; must match the `gap` in the stylesheet. */
  private readonly itemGap = 6;

  private hasLaidOut = false;
  private layoutScheduled = false;
  private resizeObserver?: ResizeObserver;
  private observedElement?: HTMLElement;

  constructor(
    private categoryService: CategoryService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.categoryService.getAllCategories().subscribe({
      next: (result: ResponseListModel<CategoryViewModel>) => {
        this.categories = result.Items ?? [];
        this.isCategoriesLoading = false;
        this.visibleCount = 0;
        this.hasLaidOut = false;
        this.scheduleLayout();
      },
      error: () => {
        this.categories = [];
        this.isCategoriesLoading = false;
      }
    });
  }

  ngAfterViewInit(): void {
    this.scheduleLayout();
  }

  ngOnDestroy(): void {
    this.resizeObserver?.disconnect();
  }

  isLinkActive(url: string | undefined | null): boolean {
    if (!url) {
      return false;
    }
    const currentUrl = window.location.pathname;
    return currentUrl.endsWith(url);// Check if the current URL ends with the specified category URL
  }

  /** Defers layout to the next frame so the DOM has been rendered before measuring. */
  private scheduleLayout(): void {
    if (this.layoutScheduled) {
      return;
    }
    this.layoutScheduled = true;
    requestAnimationFrame(() => {
      this.layoutScheduled = false;
      this.attachResizeObserver();
      this.layout();
    });
  }

  private attachResizeObserver(): void {
    const element = this.catList?.nativeElement;
    if (!element || element === this.observedElement) {
      return;
    }
    this.resizeObserver?.disconnect();
    this.observedElement = element;
    this.resizeObserver = new ResizeObserver(() => this.scheduleLayout());
    this.resizeObserver.observe(element);
  }

  /**
   * Measures the available width and decides how many categories can be shown
   * before the rest have to move into the "More" dropdown.
   */
  private layout(): void {
    const list = this.catList?.nativeElement;
    const measure = this.measureList?.nativeElement;
    const categories = this.categories;

    if (!list || !measure || !categories || categories.length === 0) {
      return;
    }

    const available = list.clientWidth;
    const items = Array.from(measure.children) as HTMLElement[];
    const moreWidth = this.measureMore?.nativeElement.getBoundingClientRect().width ?? 0;

    if (available <= 0 || items.length === 0) {
      return;
    }

    let count = 0;
    let used = 0;
    for (let i = 0; i < items.length; i++) {
      const width = items[i].getBoundingClientRect().width;
      const withGap = (count === 0 ? 0 : this.itemGap) + width;
      const remaining = items.length - (i + 1);
      // If more categories are left, space has to be kept for the "More" toggle.
      const reserveMore = remaining > 0 ? this.itemGap + moreWidth : 0;

      if (used + withGap + reserveMore <= available) {
        used += withGap;
        count++;
      } else {
        break;
      }
    }

    const changed = !this.hasLaidOut || count !== this.visibleCount;
    this.visibleCount = count;
    this.overflowCategories = count < categories.length ? categories.slice(count) : [];
    this.hasLaidOut = true;

    if (changed) {
      this.cdr.detectChanges();
    }
  }
}
