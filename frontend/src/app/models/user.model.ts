import { UserProgress } from './subject.model';
import { TrackEvent } from './track-event.model';
import { UserCareer } from '../settings/users/user-models/user-career';

export class User {
  public id?: string;
  public imageUrl?: string;
  public name: string;
  public registerNumber: string;
  public document: string;
  public username: string;
  public password: string;
  public responsible: string;
  public biography: string;
  public email: string;
  public phone: string;
  public businessGroup: number;
  public businessUnit: number;
  public country: number;
  public office: number;
  public jobTitle: number;
  public location: number;
  public rank: number;
  public sectorOne: number;
  public sectorTwo: number;
  public sectorThree: number;
  public sectorFour: number;
  public segment: number;
  public modulesInfo: Array<UserProgress>;
  public tracksInfo: Array<UserProgress>;
  public eventsInfo: Array<UserProgress>;
  public calendarEvents?: Array<TrackEvent>;
  public career?: UserCareer;
}
