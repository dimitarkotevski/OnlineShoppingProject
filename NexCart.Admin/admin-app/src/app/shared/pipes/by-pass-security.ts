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

    /**
     * Product images are stored either as raw base64, as an absolute http(s)/data URL
     * (seeded catalog images) or as a site-relative path such as /images/products/x.jpg
     * (photos saved by the API); only raw base64 needs the data-URL prefix.
     */
    transform (value: string): SafeResourceUrl {
        const isUrl = /^(https?:|data:|\/)/i.test(value || '');
        return this.sanitizer.bypassSecurityTrustResourceUrl(isUrl ? value : 'data:image/png;base64, '+value);
    }
}
