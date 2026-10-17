import { Component, Input } from '@angular/core';

export type SkeletonShape = 'text' | 'block' | 'pill' | 'circle';

/**
 * Reusable shimmer placeholder. Drop it anywhere content is loading and pick a
 * `shape` (text / block / pill / circle) plus an optional `count` and `row` layout.
 *
 * Examples:
 *   <app-skeleton></app-skeleton>                                   // one text line
 *   <app-skeleton [count]="3" height="1rem"></app-skeleton>         // three text lines
 *   <app-skeleton shape="block" height="300px"></app-skeleton>      // a card placeholder
 *   <app-skeleton [row]="true" shape="pill" [count]="6"></app-skeleton> // a row of pills
 */
@Component({
  standalone: false,
  selector: 'app-skeleton',
  templateUrl: './skeleton.component.html',
  styleUrls: ['./skeleton.component.css']
})
export class SkeletonComponent {

  /** Visual shape; provides sensible default width/height (override with `width`/`height`). */
  @Input() shape: SkeletonShape = 'text';

  /** How many shimmer items to render. */
  @Input() count = 1;

  /** Lay the items out in a row (default: stacked in a column). */
  @Input() row = false;

  /** Gap between items. */
  @Input() gap = '0.75rem';

  /** Explicit CSS width applied to every item (e.g. '80%', '12rem'). */
  @Input() width?: string;

  /** Explicit CSS height applied to every item (e.g. '300px', '1rem'). */
  @Input() height?: string;

  /** Extra class(es) applied to every item, for one-off tweaks. */
  @Input() itemClass = '';

  get items(): number[] {
    return Array.from({ length: Math.max(0, this.count) }, (_, index) => index);
  }
}
