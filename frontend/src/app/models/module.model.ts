import { SupportMaterial } from './support-material.interface';
import { Requirement } from '../settings/modules/new-module/models/new-requirement.model';
import { Subject } from './subject.model';
import { Content, ConceptReference } from './content.model';
import { Question } from './question.model';
import { TutorInfo } from './previews/tutor-info.interface';
import { ModuleGradeTypeEnum } from './enums/ModuleGradeTypeEnum';
import { ModuleConfiguration } from './module-configuration';
import { EcommerceProduct } from './ecommerce-product.model';

export class Module {
  public id: string;
  public title: string;
  public instructor: string;
  public excerpt: string;
  public instructorMiniBio: string;
  public imageUrl: string;
  public instructorImageUrl: string;
  public duration: number;
  public videoUrl?: string;
  public videoDuration?: number;
  public videoId?: string;
  public videoTitle?: string;
  public storeUrl?: string;
  public ecommerceUrl?: string;
  public createInEcommerce?: boolean;
  public ecommerceId?: number;
  public tags: Array<string>;
  public supportMaterials: Array<SupportMaterial>;
  public requirements: Array<Requirement>;
  public requiredModules: Array<Requirement>;
  public optionalModules: Array<Requirement>;
  public subjects: Array<Subject>;
  public moduleWeights: Array<ModuleWeights>;
  public contents: Array<Content>;
  public questions: Array<Question>;
  public instructorId: string;
  public tutorsIds: string[];
  public tutors?: TutorInfo[];
  public certificateUrl: string;
  public published?: boolean;
  public questionsLimit?: number;
  public isDraft?: boolean;
  public moduleGradeType: ModuleGradeTypeEnum;
  public ecommerceProducts?: Array<EcommerceProduct>;
  public grade?: number;
  public extraInstructorIds: string[];
  public extraInstructors?: TutorInfo[];
  public moduleConfiguration: ModuleConfiguration;
  public validFor: string;
  public hasUserProgess: boolean;

  public hovered?: boolean;

  constructor(module: Module = null) {
    if (module)
      Object.keys(module).forEach(key => this[key] = module[key]);
  }

  public setModuleInfo(moduleInfo: Module) {
    this.title = moduleInfo.title;
    this.instructor = moduleInfo.instructor;
    this.excerpt = moduleInfo.excerpt;
    this.instructorMiniBio = moduleInfo.instructorMiniBio;
    this.imageUrl = moduleInfo.imageUrl;
    this.instructorImageUrl = moduleInfo.instructorImageUrl;
    this.tags = moduleInfo.tags;
    this.instructorId = moduleInfo.instructorId;
    this.published = moduleInfo.published;
    this.certificateUrl = moduleInfo.certificateUrl;
    this.tutorsIds = moduleInfo.tutorsIds;
    this.extraInstructorIds = moduleInfo.extraInstructorIds;
    this.storeUrl = moduleInfo.storeUrl;
    this.ecommerceUrl = moduleInfo.ecommerceUrl;
    this.createInEcommerce = moduleInfo.createInEcommerce;
    this.ecommerceId = moduleInfo.ecommerceId;
    this.moduleGradeType = moduleInfo.moduleGradeType;
    this.validFor = moduleInfo.validFor;
    this.ecommerceProducts = moduleInfo.ecommerceProducts;
  }

  public setVideoInfo(module: Module) {
    this.videoUrl = module.videoUrl;
    this.videoDuration = module.videoDuration;
    this.videoId = module.videoId;
    this.videoTitle = module.videoTitle;
  }

  public addSupportMaterials(materials: Array<SupportMaterial>) {
    this.supportMaterials = materials;
  }

  public setRequirements(mandatory: Array<Requirement>, optional: Array<Requirement>) {
    this.requiredModules = mandatory;
    this.optionalModules = optional;
  }

  public addSubjects(subjects: Array<Subject>) {
    this.subjects = subjects;
  }

  public addModulesWeights(weights: Array<ModuleWeights>) {
    this.moduleWeights = weights;
  }

  public addContents(contents: Array<Content>) {
    contents.forEach(content => {
      content.concepts = content.concepts.filter(conc => conc.checked);
    });

    this.contents = contents;
  }

  public addQuestions(questions: Array<Question>) {
    this.questions = questions;
  }
}

export interface ModuleWeights {
  content: string;
  weight: number;
  label: string;
}
