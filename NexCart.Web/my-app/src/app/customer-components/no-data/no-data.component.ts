import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  standalone: false,
  selector: 'app-no-data',
  templateUrl: './no-data.component.html',
  styleUrls: ['./no-data.component.css']
})
export class NoDataComponent {
  /** Main title displayed in the empty state */
  @Input() title: string = 'No Data Available';

  /** Descriptive message explaining that there is no data in the database */
  @Input() message: string = 'There is currently no data in the database to display.';

  /** FontAwesome icon class to display in the circle */
  @Input() icon: string = 'fa-solid fa-database';

  /** Whether to show the action button */
  @Input() showButton: boolean = true;

  /** Primary button label */
  @Input() buttonText: string = 'Back to Home';

  /** Router link destination (can be null/empty if handled via buttonClick) */
  @Input() buttonRoute?: string | any[] = '/';

  /** Icon displayed in the primary button */
  @Input() buttonIcon?: string = 'fa-solid fa-arrow-left';

  /** Optional secondary button label */
  @Input() secondaryButtonText?: string;

  /** Optional icon for the secondary button */
  @Input() secondaryButtonIcon?: string;

  /** Minimum height of the container to prevent layout jump */
  @Input() minHeight: string = '300px';

  /** Event emitted when the primary button is clicked */
  @Output() buttonClick = new EventEmitter<void>();

  /** Event emitted when the secondary button is clicked */
  @Output() secondaryButtonClick = new EventEmitter<void>();

  onButtonClick(): void {
    this.buttonClick.emit();
  }

  onSecondaryButtonClick(): void {
    this.secondaryButtonClick.emit();
  }
}
