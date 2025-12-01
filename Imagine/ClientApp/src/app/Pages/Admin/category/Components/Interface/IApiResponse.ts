export interface IApiResponse <T>{
    successs:boolean;
    data:T;
    message:string;
    currentpage?:number;
    totalpages?:number;
    pageSize?:number;
    totalItems?:number;
}
