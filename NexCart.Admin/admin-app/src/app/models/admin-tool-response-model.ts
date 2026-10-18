export class ResponseModel<T>{
    TotalItems:number = 0
    Items?: Array<T>
    Size: number = 10
    CurrentPage: number = 0
}