import { LookupViewModel } from "./lookup-view-model"

export class ProductViewModel{
    Id?: string
    Name?: string
    Image?: any
    Price?: number
    Description?: string
    Category?: LookupViewModel
    DateCreated?: string
}