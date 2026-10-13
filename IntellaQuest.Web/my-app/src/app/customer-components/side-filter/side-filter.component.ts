import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { ProductService } from '../product-components/product.service';
import { RequestModel } from 'src/app/models/request';

@Component({ standalone: false,
  selector: 'app-side-filter',
  templateUrl: './side-filter.component.html',
  styleUrls: ['./side-filter.component.css']
})
export class SideFilterComponent implements OnInit {

  @Input() category:any
  @Output() refreshState: EventEmitter<RequestModel> = new EventEmitter<RequestModel>();;
  filterProducts: RequestModel;
  @Input() brands:any;
  filter: string[] = [];

  constructor(
    private productService: ProductService
  ) { 
    this.filterProducts = new RequestModel();
  }

  ngOnInit(): void {
    this.productService.GetAllBrands(this.category)?.subscribe((res)=>{
      this.brands = res;
    })
  }

  onChangeBrands(event: any) {
    if(event.target.checked){
      this.filter.push(event.target.value);
    }else{
      const index = this.filter.indexOf(event.target.value);
      if (index > -1) {
        this.filter.splice(index, 1);
      }
    }

    // Keep the other filter values (price range, etc.); only the brand list changes.
    this.filterProducts.SearchBrands = this.filter
    this.refreshState.emit(this.filterProducts);
  }

  onChange(event: any) {
    if(event.target.checked){
      this.filter.push(event.target.value);
    }else{
      const index = this.filter.indexOf(event.target.value);
      if (index > -1) {
        this.filter.splice(index, 1);
      }
    }
    this.filterProducts.SearchList = this.filter;
    this.refreshState.emit(this.filterProducts);
  }

  searchPrice(){
    this.refreshState.emit(this.filterProducts);
  }

}

