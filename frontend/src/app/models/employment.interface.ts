export interface Employment {
  record: Record;
  activities: Activities;
  preRequirements: PreRequirements;
  values: string[];
  benefits: Benefits;
}

export interface Record {
  function: string;
  contractType: string;
}

export interface Activities {
  activity: string;
  character: string;
  abilities: string;
  report: string;
}

export interface PreRequirements {
  education: string;
  curseName: string;
  dateConclusion: string;
  crAcumulation: string;
  minTime: number;
  complementaryInfo: ComplementaryInfo[];
  others: Others[];
  certification: string;
  languageInfo: LanguageInfo[];
}
export interface Others {
  name: string;
  level: string;
}
export interface LanguageInfo {
  language: string;
  level: string;
}

export interface ComplementaryInfo {
  name: string;
  done: boolean;
  level: string;
}

export interface Benefits {
  salary: string;
  complementaryBenefits: ComplementaryBenefits[];
  employmentType: string;
  employmentChangeHouse: string;
}

export interface ComplementaryBenefits {
  name: string;
  done: boolean;
  level: string;
}




