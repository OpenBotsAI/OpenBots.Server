export interface Consent {
    personId: string;
    person: Person;
    userAgreementID: string;
    userAgreements: UserAgreements;
    recordedOn: Date;
    isAccepted: boolean;
    expiresOnUTC: Date;
    id: string;
    isDeleted: boolean;
    createdBy: string;
    createdOn: Date;
    deletedBy: string;
    deleteOn: Date;
    timestamp: string;
}

export interface Person {
    firstName: string;
    lastName: string;
    company: string;
    department: string;
    credentials: Credential[];
    name: string;
    id: string;
    isDeleted: boolean;
    createdBy: string;
    createdOn: Date;
    deletedBy: string;
    deleteOn: Date;
    timestamp: string;
}

export interface UserAgreements {
    version: number;
    title: string;
    contentStaticUrl: string;
    effectiveOnUTC: Date;
    expiresOnUTC: Date;
    id: string;
    isDeleted: boolean;
    createdBy: string;
    createdOn: Date;
    deletedBy: string;
    deleteOn: Date;
    timestamp: string;
}