import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { FormGroup, FormControl, Validators, FormArray } from '@angular/forms';
import { MatSnackBar } from '@angular/material';
import { NotificationClass } from '../../../../../shared/classes/notification';
import { UtilService } from '../../../../../shared/services/util.service';
import { Track } from '../../../../../models/track.model';
import { TrackEvent } from 'src/app/models/track-event.model';
import * as moment from 'moment/moment';
import { SettingsTracksService } from 'src/app/settings/_services/tracks.service';

@Component({
  selector: 'app-new-track-relevant-dates',
  templateUrl: './relevant-dates.component.html',
  styleUrls: ['../new-track-steps.scss', './relevant-dates.component.scss']
})
export class NewTrackRelevantDatesComponent extends NotificationClass implements OnInit {

  @Input() readonly track: Track;
  @Output() manageRelevantDates = new EventEmitter<Track>();

  public formGroup: FormGroup;

  constructor(
    protected _snackBar: MatSnackBar,
    private _tracksService: SettingsTracksService,
    private _utilService: UtilService
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    this.formGroup = this._createFormGroup();
    this._fillFormEvents( this.track );
  }

  public nextStep(): void {
    if (this.formGroup.valid) {
      const relevantDates = this.formGroup.getRawValue();

      relevantDates.calendarEvents.forEach(calEv =>
        calEv = this._adjustEventTime(calEv)
      );

      this.manageRelevantDates.emit( relevantDates.calendarEvents );

    } else {
      this.formGroup = this._utilService.markFormControlsAsTouch( this.formGroup );
      this.notify('Por favor, preencha todos os campos obrigatórios');
    }
  }

  public addCalendarEvent(): void {
    const calEvents = this.formGroup.get('calendarEvents') as FormArray;
    calEvents.push(
      this._createCalendarEventForm()
    );
  }

  public removeCalendarEvent(index: number): void {
    const calEvents = this.formGroup.get('calendarEvents') as FormArray;
    calEvents.removeAt(index);
  }

  private _adjustEventTime(calEv: TrackEvent): TrackEvent {
    calEv.eventDate = new Date(calEv.eventDate);

    if (calEv.duration) {
      if ((calEv.duration as string).length === 4)
        calEv.duration = calEv.duration + '00';

      calEv.duration = this._utilService.getDurationFromFormattedHour(
        calEv.duration.toString()
      );
    }

    if (calEv.startHour && calEv.startHour.trim() !== '') {
      calEv.startHour = calEv.startHour.split(':').join('');
      const startHours = calEv.startHour.substring(0, 2);
      calEv.eventDate.setHours(
        parseInt(startHours, 10)
      );

      const startMinutes = calEv.startHour.substring(2, 4);
      calEv.eventDate.setMinutes(
        parseInt(startMinutes, 10)
      );
    }

    return calEv;
  }

  private _createFormGroup(): FormGroup {
    return new FormGroup({
      'calendarEvents': new FormArray([])
    });
  }

  private _createCalendarEventForm(calEvent: TrackEvent = null): FormGroup {
    return new FormGroup({
      'title': new FormControl(calEvent ? calEvent.title : '', [ Validators.required ]),
      'eventDate': new FormControl(calEvent ? calEvent.eventDate : '', [ Validators.required ]),
      'startHour': new FormControl(calEvent ? moment(calEvent.eventDate).format('HH:mm') : ''),
      'duration': new FormControl(calEvent && calEvent.duration ?
        this._utilService.formatDurationToHour(calEvent.duration as number) : null
      )
    });
  }

  private _fillFormEvents(track: Track): void {
    if (track.calendarEvents) {
      const calEvents = this.formGroup.get('calendarEvents') as FormArray;
      track.calendarEvents.forEach(calEv => {
        calEvents.push(
          this._createCalendarEventForm(calEv)
        );
      });
    }
  }

  public setDocumentFile(files: FileList) {
    const file = files.item(0);
    const callback = this._sendToServer.bind(this);
    const reader = new FileReader();
    reader.onloadend = function (e) {
      callback(
        this.result as string
      );
    };
    reader.readAsDataURL(file);
  }

  private _sendToServer(result: string) {

    this.notify('Dependendo do tamanho da planilha o processo de importação pode demorar\
     um pouco. Assim que completar ele aparecerá na lista abaixo');

    this._tracksService.addCalendarEventsFromFile(this.track.id, result).subscribe((response) => {
      this.notify('Arquivo enviado com sucesso!');
    }, (err: any) => {
      this.notify('Ocorreu um erro ao enviar o arquivo, por favor tente novamente mais tarde');
    });
  }
}
