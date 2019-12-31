export interface UserCareer {
    id?: string;
    professionalExperience: boolean;
    professionalExperiences: ProfessionalExperience[];
    colleges: College[];
    rewards: Reward[];
    languages: PerkLanguage[];
    abilities: Perk[];
    certificates: Certificate[];
    skills: string[];
    travelAvailability: boolean;
    movingAvailability: boolean;
    shortDateObjectives?: string;
    longDateObjectives?: string;
}

export interface ProfessionalExperience {
    title: string;
    role: string;
    description: string;
    startDate: Date;
    endDate: Date;
}

export interface College {
    instituteId: string;
    title: string;
    campus: string;
    name: string;
    academicDegree: string;
    status: string;
    completePeriod: string;
    startDate: Date;
    endDate: Date;
    cr: string;
}

export interface Reward {
    title: string;
    name: string;
    link: string;
    date: Date;
}

export interface Perk {
    name: string;
    level: string;
    hasLevel: boolean;
}
export interface PerkLanguage {
    names: string;
    level: string;
    languages: string;
}



export interface Certificate {
    title: string;
    link: string;
}

export interface Institute {
    name: string;
    type: number;
}
