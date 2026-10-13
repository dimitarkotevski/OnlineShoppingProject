import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ProductService } from '../product.service';
import { ProductGridModel } from '../../../models/product-grid';
import { ProductViewModel } from 'src/app/models/product-view-model';

@Component({ standalone: false,
  selector: 'app-product-detail',
  templateUrl: './product-detail.component.html',
  styleUrls: ['./product-detail.component.css']
})
export class ProductDetailComponent implements OnInit {

  productDetail?: ProductViewModel

  message?: string="";
  type?: any="";

  constructor(
    private route: ActivatedRoute,
    private productService: ProductService
    ) { }

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      const id = params.get('id');
      if(id){
        this.productService.get(id).subscribe( (result: ProductGridModel)=>{
          this.productDetail = result;
        })
      }
    });
  }

  addToFavourite(Id:string | undefined){
    this.message = 'Success message';
    this.type = 'success';
  }

}

