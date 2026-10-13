import { Component, Input, OnInit } from '@angular/core';
import { AuthentificationService } from '../../authentification/authentification.service';

@Component({ standalone: false,
  selector: 'app-breadcrumb',
  templateUrl: './breadcrumb.component.html',
  styleUrls: ['./breadcrumb.component.css']
})
export class BreadcrumbComponent implements OnInit {

  @Input() isItem: boolean = false;
  @Input() itemDetail?: any;
  
  user?: string;

  constructor(private userAuth: AuthentificationService ) { }

  ngOnInit(): void {
    this.user = this.userAuth.getLoggedUsername();
  }
}

