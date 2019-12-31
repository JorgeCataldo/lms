
export class Company {
    public id?: string;
    public name: string;
    public cnpj: CompanyCnpj;
    public address: Address;
    public RHname: string;
    public RHemail: string;
    public RHphone: string;
    public ContactName: string;
    public ContactEmail: string;
    public ContactPhone: string;
    public size: number;
    public area: string;
    public yearlyHirings: number;

}

export class CompanyCnpj {
    public value: string;
}

export class Address {
    public street: string ;
    public address2: string ;
    public district: string ;
    public city: string ;
    public state: string ;
    public country: string ;
    public zipCode: string ;
}
