import { Component, OnInit, Output, EventEmitter, Input } from '@angular/core';
import { FormGroup, FormControl, Validators, FormArray } from '@angular/forms';
import { Event } from '../../../../../models/event.model';
import { EventSchedule } from '../../../../../models/event-schedule.model';
import { MatSnackBar } from '@angular/material';
import { NotificationClass } from '../../../../../shared/classes/notification';
import { UtilService } from '../../../../../shared/services/util.service';

import * as moment from 'moment/moment';
import { SettingsEventsDraftsService } from 'src/app/settings/_services/events-drafts.service';
import { UserRelationalItem } from 'src/app/settings/users/user-models/user-relational-item';
import { SettingsUsersService } from 'src/app/settings/_services/users.service';

@Component({
  selector: 'app-new-event-date',
  templateUrl: './date.component.html',
  styleUrls: ['../new-event-steps.scss', './date.component.scss']
})
export class NewEventDateComponent extends NotificationClass implements OnInit {

  @Input() readonly event: Event;
  @Output() setEventDates = new EventEmitter();

  public formGroup: FormGroup;
  public locations: UserRelationalItem[];
  public selectedLocations: string[];

  constructor(
    protected _snackBar: MatSnackBar,
    private _draftsService: SettingsEventsDraftsService,
    private _utilService: UtilService,
    private _usersService: SettingsUsersService,
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    this.selectedLocations = [];
    this.formGroup = this._createFormGroup( this.event );
    this._getAllLocations();
  }

  public nextStep(): void {
    const eventDates = this.formGroup.getRawValue();
    eventDates.schedules.forEach((schedule, index) => {
      this.saveSchedule(index);
    });
    this.setEventDates.emit( eventDates.schedules );
  }

  public addSchedule(): void {
    const materials = this.formGroup.get('schedules') as FormArray;
    materials.push(
      this._createScheduleForm()
    );
  }

  private _createFormGroup(event: Event): FormGroup {

    return new FormGroup({
      'schedules': this._setSchedulesFormArray( event )
    });
  }

  private _setSchedulesFormArray(event: Event): FormArray {

    if (event && event.schedules) {

      for (let index = 0; index < event.schedules.length; index++) {
        if (event.schedules[index].location) {
          this.selectedLocations.push(event.schedules[index].location.id);
        } else {
          this.selectedLocations.push('');
        }
      }

      return new FormArray(
        event.schedules.map((sc) => this._createScheduleForm(sc))
      );
    } else {
      return new FormArray([
        this._createScheduleForm()
      ]);
    }
  }

  private _createScheduleForm(schedule: EventSchedule = null): FormGroup {

    return new FormGroup({
      'id': new FormControl(schedule ? schedule.id : null),
      'eventId': new FormControl(this.event.id),
      'subscriptionStartDate': new FormControl(
        schedule ? schedule.subscriptionStartDate : '', [ Validators.required ]
      ),
      'subscriptionEndDate': new FormControl(
        schedule ? schedule.subscriptionEndDate : '', [ Validators.required ]
      ),
      'discussionStartDate': new FormControl(
        schedule ? schedule.subscriptionStartDate : '', [ Validators.required ]
      ),
      'discussionEndDate': new FormControl(
        schedule ? schedule.subscriptionEndDate : '', [ Validators.required ]
      ),
      'forumStartDate': new FormControl(
        schedule ? schedule.forumStartDate : ''
      ),
      'forumEndDate': new FormControl(
        schedule ? schedule.forumEndDate : ''
      ),
      'published': new FormControl(
        schedule ? schedule.published : '', [ Validators.required ]
      ),
      'eventDate': new FormControl(
        schedule ? schedule.eventDate : '', [ Validators.required ]
      ),
      'startHour': new FormControl(
        schedule && schedule.eventDate ?
          moment(schedule.eventDate).format('HH:mm') : '', [ Validators.required ]
      ),
      'duration': new FormControl(
        schedule && schedule.duration ?
          this._utilService.formatDurationToHour(schedule.duration) : '00:00:00',
          [ Validators.required ]
      ),
      'webinarUrl': new FormControl(
        schedule ? schedule.webinarUrl : ''
      ),
      'location': new FormControl(
        schedule && schedule.location ? schedule.location.id : ''
      ),
      'applicationLimit': new FormControl(
        schedule ? schedule.applicationLimit : ''
      )
    });
  }

  public saveSchedule(index: number): void {
    const formArray = this.formGroup.get('schedules') as FormArray;
    const schedule: EventSchedule = (formArray.controls[index] as FormGroup).getRawValue();
    schedule.eventId = this.event.id;
    schedule.duration = this._utilService.getDurationFromFormattedHour(
      schedule.duration as any
    );

    schedule.eventDate = new Date(schedule.eventDate);
    schedule.startHour = schedule.startHour.split(':').join('');

    const startHours = schedule.startHour.substring(0, 2);
    schedule.eventDate.setHours(parseInt(startHours, 10));

    const startMinutes = schedule.startHour.substring(2, 4);
    schedule.eventDate.setMinutes(parseInt(startMinutes, 10));

    this._draftsService.manageEventDraftSchedule(schedule).subscribe(
      (response) => {
        formArray.controls[index].get('id').setValue(response.data.id);
      },
      (error) => {
        this.notify(
          this.getErrorNotification(error)
        );
      }
    );
  }

  private _getAllLocations(): void {
    this._usersService.getAllLocations().subscribe((response) => {
      this.locations = response.data;
    }, (error) => this.notify( this.getErrorNotification(error) ));

  }

  public changeLocation(locationId: string, scheduleIndex: number) {
    const formArray = this.formGroup.get('schedules') as FormArray;
    const schedule: EventSchedule = (formArray.controls[scheduleIndex] as FormGroup).getRawValue();
    schedule.location = this.locations.find(x => x.id === locationId);
  }
}
