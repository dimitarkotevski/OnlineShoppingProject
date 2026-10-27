import { Component, Input, OnInit } from '@angular/core';

@Component({ standalone: false,
  selector: 'app-loading-grid',
  templateUrl: './loading-grid.component.html',
  styleUrls: ['./loading-grid.component.css']
})
export class LoadingGridComponent implements OnInit {

  @Input() isGridLoading = true;

  /** Placeholder counts chosen to fill a typical grid/table without looking sparse. */
  readonly productPlaceholders = Array.from({ length: 8 });
  readonly listPlaceholders = Array.from({ length: 4 });

  constructor() { }

  ngOnInit(): void {
  }

}

