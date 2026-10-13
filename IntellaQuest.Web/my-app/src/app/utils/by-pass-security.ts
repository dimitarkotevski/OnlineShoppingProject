import { Pipe, PipeTransform, SecurityContext } from "@angular/core";
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';

@Pipe({ standalone: false,
    name: 'byPassSecurity'
})
export class ByPassSecurityPipe implements PipeTransform {

    constructor(
        private sanitizer: DomSanitizer
        ) 
    {}

    transform (value: string): SafeHtml {
        return this.sanitizer.bypassSecurityTrustResourceUrl('data:image/png;base64, '+value);
    }
}
