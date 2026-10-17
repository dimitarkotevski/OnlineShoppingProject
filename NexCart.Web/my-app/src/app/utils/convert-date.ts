import { Pipe, PipeTransform } from "@angular/core";

@Pipe({ standalone: false,
    name: 'dateConverter'
})
export class DateConverter implements PipeTransform {

    constructor() {}

    transform(date: string): string | number {
        if (!date) {
            return 0;
        }

        // Support both the legacy ASP.NET "/Date(1234567890000)/" format and ISO strings.
        const legacy = /\/Date\((-?\d+)(?:[+-]\d+)?\)\//.exec(date);
        const parsed = legacy ? new Date(parseInt(legacy[1], 10)) : new Date(date);

        if (isNaN(parsed.getTime())) {
            return 0;
        }

        // getMonth() is zero-based, so add 1 to render the correct month.
        return parsed.getDate() + "/" + (parsed.getMonth() + 1) + "/" + parsed.getFullYear();
      }
}
