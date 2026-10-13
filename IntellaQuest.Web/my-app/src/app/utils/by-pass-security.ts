import { Pipe, PipeTransform } from "@angular/core";
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';

@Pipe({ standalone: false,
    name: 'byPassSecurity'
})
export class ByPassSecurityPipe implements PipeTransform {

    constructor(
        private sanitizer: DomSanitizer
        ) 
    {}

    transform (value: string): SafeResourceUrl {
        return this.sanitizer.bypassSecurityTrustResourceUrl('data:image/png;base64, '+value);
    }
}
