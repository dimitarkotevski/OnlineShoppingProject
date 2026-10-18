/**
 * Tracks the rows ticked in an admin grid for bulk actions. Selection is per page: the grids
 * call clear() whenever they reload, so an action never touches rows the admin can't see.
 */
export class GridSelection {
  private readonly ids = new Set<string>();

  /**
   * @param canSelect Rows for which this returns false are never selected (for example the
   * signed-in admin's own account in the users grid).
   */
  constructor(private readonly canSelect: (id: string) => boolean = () => true) {}

  get count(): number {
    return this.ids.size;
  }

  get selectedIds(): string[] {
    return Array.from(this.ids);
  }

  isSelectable(id: string | undefined): boolean {
    return !!id && this.canSelect(id);
  }

  isSelected(id: string | undefined): boolean {
    return !!id && this.ids.has(id);
  }

  toggle(id: string | undefined): void {
    if (!this.isSelectable(id)) {
      return;
    }
    if (this.ids.has(id!)) {
      this.ids.delete(id!);
    } else {
      this.ids.add(id!);
    }
  }

  /** True when every selectable row on the page is ticked (and there is at least one). */
  allSelected(rows: { Id?: string }[] | undefined): boolean {
    const selectable = this.selectableIds(rows);
    return selectable.length > 0 && selectable.every(id => this.ids.has(id));
  }

  /** True when some, but not all, selectable rows on the page are ticked. */
  someSelected(rows: { Id?: string }[] | undefined): boolean {
    return this.count > 0 && !this.allSelected(rows);
  }

  /** Header checkbox: tick every selectable row on the page, or untick them all. */
  toggleAll(rows: { Id?: string }[] | undefined): void {
    if (this.allSelected(rows)) {
      this.clear();
    } else {
      this.selectableIds(rows).forEach(id => this.ids.add(id));
    }
  }

  clear(): void {
    this.ids.clear();
  }

  private selectableIds(rows: { Id?: string }[] | undefined): string[] {
    return (rows || [])
      .map(row => row.Id)
      .filter((id): id is string => this.isSelectable(id));
  }
}
