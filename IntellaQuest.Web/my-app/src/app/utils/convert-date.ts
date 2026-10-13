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
        const timestamp = parseInt(date.substr(6)); // Extract the timestamp without "/Date(" and "/)"
        var formatedDate = new Date(timestamp);
        return formatedDate.getDate()+"/"+formatedDate.getMonth()+"/"+formatedDate.getFullYear();
      }    
}
