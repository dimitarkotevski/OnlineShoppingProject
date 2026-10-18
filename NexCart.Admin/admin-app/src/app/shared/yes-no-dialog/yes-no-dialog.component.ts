import { Component, Inject, Input, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';

@Component({ standalone: false,
  selector: 'app-yes-no-dialog',
  templateUrl: './yes-no-dialog.component.html',
  styleUrls: ['./yes-no-dialog.component.css']
})
export class YesNoDialogComponent implements OnInit {

  /** The question shown in the dialog. Kept as an @Input for backwards compatibility. */
  @Input() text: string;

  /** Optional presentation overrides; all have sensible defaults. */
  title: string;
  confirmText: string;
  cancelText: string;
  /** When true the dialog is styled as a destructive action (red confirm button). */
  danger: boolean;

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: any,
      ) { 
        const options = data ?? {};
        this.text = options.text ?? 'Are you sure you want to continue?';

        // Deletions/removals/cancellations read better in red; anything else uses the
        // site's primary colour. Callers can override either way with `danger`.
        this.danger = options.danger
          ?? /(delete|remove|cancel)/i.test(this.text);

        this.title = options.title ?? (this.danger ? 'Please confirm' : 'Confirm');
        this.confirmText = options.confirmText ?? 'Yes';
        this.cancelText = options.cancelText ?? 'No';
      }

  ngOnInit(): void {
  }

}
