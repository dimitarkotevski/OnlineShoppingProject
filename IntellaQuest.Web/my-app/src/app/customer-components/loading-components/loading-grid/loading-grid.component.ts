import { Component, Input, OnInit } from '@angular/core';

@Component({ standalone: false,
  selector: 'app-loading-grid',
  templateUrl: './loading-grid.component.html',
  styleUrls: ['./loading-grid.component.css']
})
export class LoadingGridComponent implements OnInit {

  @Input() isGridLoading = true;

  constructor() { }

  ngOnInit(): void {
  }

}

