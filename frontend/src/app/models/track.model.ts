import { TrackEvent } from './track-event.model';
import { TrackModule } from './track-module.model';
import { ModulePreview } from './previews/module.interface';
import { EcommerceProduct } from './ecommerce-product.model';

export class Track {
  public id?: string;
  public title: string;
  public description: string;
  public storeUrl?: string;
  public ecommerceUrl?: string;
  public createInEcommerce?: boolean;
  public imageUrl: string;
  public conclusion: number;
  public modulesConfiguration: Array<TrackModule>;
  public modules?: Array<ModulePreview>;
  public eventsConfiguration?: Array<TrackEvent>;
  public tags: Array<string>;
  public published: boolean;
  public certificateUrl: string;
  public videoUrl?: string;
  public videoDuration?: number;
  public videoId?: string;
  public videoTitle?: string;
  public mandatoryCourseVideo: boolean;
  public courseVideoUrl?: string;
  public courseVideoDuration?: number;
  public courseVideoId?: string;
  public courseVideoTitle?: string;
  public calendarEvents?: Array<TrackEvent>;
  public ecommerceProducts?: Array<EcommerceProduct>;
  public requireUserCareer: boolean;
  public allowedPercentageWithoutCareerInfo?: number;
  public profileTestId?: string;
  public profileTestName?: string;
  public validFor?: number;

  constructor(track: Track = null) {
    if (track)
      Object.keys(track).forEach(key => this[key] = track[key]);
  }

  public setTrackInfo(trackInfo: Track) {
    this.title = trackInfo.title;
    this.description = trackInfo.description;
    this.imageUrl = trackInfo.imageUrl;
    this.tags = trackInfo.tags;
    this.published = trackInfo.published ? true : false;
    this.certificateUrl = trackInfo.certificateUrl;
    this.storeUrl = trackInfo.storeUrl;
    this.ecommerceUrl = trackInfo.ecommerceUrl;
    this.requireUserCareer = trackInfo.requireUserCareer ? true : false;
    this.allowedPercentageWithoutCareerInfo = this.requireUserCareer ?
      trackInfo.allowedPercentageWithoutCareerInfo : null;
    this.createInEcommerce = trackInfo.createInEcommerce;
    this.profileTestId = trackInfo.profileTestId;
    this.profileTestName = trackInfo.profileTestName;
    this.validFor = trackInfo.validFor;
  }

  public setVideoInfo(trackInfo: Track) {
    this.videoUrl = trackInfo.videoUrl;
    this.videoDuration = trackInfo.videoDuration;
    this.videoId = trackInfo.videoId;
    this.videoTitle = trackInfo.videoTitle;
    this.mandatoryCourseVideo = trackInfo.mandatoryCourseVideo;
    this.courseVideoUrl = trackInfo.courseVideoUrl;
    this.courseVideoDuration = trackInfo.courseVideoDuration;
    this.courseVideoId = trackInfo.courseVideoId;
    this.courseVideoTitle = trackInfo.courseVideoTitle;
  }
}
