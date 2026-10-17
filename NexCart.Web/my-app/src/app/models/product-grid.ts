import { LookupViewModel } from "./lookup-view-model";

export class ProductGridModel{
    Id:string = '';
    Name?:string;
    Price?:number;
    Image?:any;
    DateCreated?: string;
    Category?: LookupViewModel
}