import { UserRelationalItem, UserLocationRelationalItem } from './user-relational-item';


export class User {
    // Basic info
    public id: string;
    public role: string = 'Student';
    public name: string;
    public registrationId: string;
    public cpf: UserCpf;
    public address: Address;
    public userName: string;
    public password: string;
    public responsibleId: string;
    public responsibleName: string;
    public lineManager: string;
    public info: string;
    public specialNeeds: boolean;
    public specialNeedsDescription: string;
    public imageUrl: string;
    public linkedIn?: string;
    public dateBorn?: Date;
    public profile: UserProfile;
    public forumActivities: boolean;
    public forumEmail: string;
    // Contact
    public email: string;
    public phone: string;
    public phone2: string;
    // Document
    public document: number;
    public documentNumber: string;
    public documentEmitter: string;
    public emitDate: Date;
    public expirationDate: Date;
    // Category
    public businessGroup: UserRelationalItem;
    public businessUnit: UserRelationalItem;
    public country: UserRelationalItem;
    public frontBackOffice: UserRelationalItem;
    public job: UserRelationalItem;
    public location: UserLocationRelationalItem;
    public rank: UserRelationalItem;
    public sectors: UserRelationalItem[];
    public segment: UserRelationalItem;
    // Appsettings blocked fields
    public blockedFields?: string [];
}

export class UserCpf {
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

export class UserProfile {
    title: string;
    biasOne: string;
    biasTwo: string;
}
