import { SupportMaterial } from './support-material.interface';
import { Requirement } from '../settings/modules/new-module/models/new-requirement.model';
import { EventSchedule } from './event-schedule.model';
import { TutorInfo } from './previews/tutor-info.interface';
import { PrepQuizQuestions } from '../prepQuizQuestions.interface';

export class Event {
  public id?: string;
  public createdAt?: Date;
  public title: string;
  public instructorId: string;
  public instructor: string;
  public instructorMiniBio: string;
  public difficulty: string;
  public excerpt: string;
  public tags: Array<string>;
  public duration: number;
  public imageUrl: string;
  public instructorImageUrl: string;
  public status: string;
  public videoUrl: string;
  public videoDuration: number;
  public videoId?: string;
  public videoTitle?: string;
  public storeUrl?: string;
  public createInEcommerce?: boolean;
  public ecommerceId?: number;
  public schedules: Array<EventSchedule>;
  public supportMaterials: Array<SupportMaterial>;
  public requirements: Array<Requirement>;
  public requiredModules: Array<Requirement>;
  public optionalModules: Array<Requirement>;
  public prepQuizQuestionList: Array<PrepQuizQuestions>;
  public certificateUrl: string;
  public tutorsIds: string[];
  public tutors?: TutorInfo[];
  public isDraft?: boolean;
  public forceProblemStatement?: boolean;

  constructor(event: Event = null) {
    if (event)
      Object.keys(event).forEach(key => this[key] = event[key]);
  }

  public setEventInfo(eventInfo: Event) {
    this.title = eventInfo.title;
    this.difficulty = eventInfo.difficulty;
    this.instructorId = eventInfo.instructorId;
    this.instructor = eventInfo.instructor;
    this.excerpt = eventInfo.excerpt;
    this.instructorMiniBio = eventInfo.instructorMiniBio;
    this.imageUrl = eventInfo.imageUrl;
    this.instructorImageUrl = eventInfo.instructorImageUrl;
    this.tags = eventInfo.tags;
    this.certificateUrl = eventInfo.certificateUrl;
    this.tutorsIds = eventInfo.tutorsIds;
    this.storeUrl = eventInfo.storeUrl;
    this.createInEcommerce = eventInfo.createInEcommerce;
    this.ecommerceId = eventInfo.ecommerceId;
    this.forceProblemStatement = eventInfo.forceProblemStatement;
  }

  public setVideoInfo(videoInfo: Event) {
    this.videoUrl = videoInfo.videoUrl;
    this.videoDuration = videoInfo.videoDuration;
    this.videoId = videoInfo.videoId;
    this.videoTitle = videoInfo.videoTitle;
  }

}
