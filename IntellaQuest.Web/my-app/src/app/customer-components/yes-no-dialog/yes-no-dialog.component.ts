import { Component, Inject, Input, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';

@Component({ standalone: false,
  selector: 'app-yes-no-dialog',
  templateUrl: './yes-no-dialog.component.html',
  styleUrls: ['./yes-no-dialog.component.css']
})
export class YesNoDialogComponent implements OnInit {

  @Input() text: string;
  constructor(
    @Inject(MAT_DIALOG_DATA) public data: any,
      ) { 
        this.text = data.text
      }

  ngOnInit(): void {
  }

}

