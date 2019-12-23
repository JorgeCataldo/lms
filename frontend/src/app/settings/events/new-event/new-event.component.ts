import { Component, ViewChild, OnInit, OnDestroy } from '@angular/core';
import { MatStepper, MatDialog, MatSnackBar } from '@angular/material';
import { Router } from '@angular/router';
import { Event } from '../../../models/event.model';
import { Requirement } from '../../modules/new-module/models/new-requirement.model';
import { NewEventEventInfoComponent } from './steps/1_event-info/event-info.component';
import { CreatedEventDialogComponent } from './steps/7_created-event/created-event.dialog';
import { NewEventDateComponent } from './steps/2_date/date.component';
import { NewEventVideoComponent } from './steps/3_video/video.component';
import { NewEventSupportMaterialsComponent } from './steps/4_support-materials/support-materials.component';
import { NewEventRequirementsComponent } from './steps/5_requirements/requirements.component';
import { NewEventQuestionsComponent } from './steps/6_questions/questions.component';
import { SupportMaterial } from '../../../models/support-material.interface';
import { NotificationClass } from '../../../shared/classes/notification';
import { SettingsEventsService } from '../../_services/events.service';
import { EventSchedule } from '../../../models/event-schedule.model';
import { Level } from '../../../models/shared/level.interface';
import { SharedService } from '../../../shared/services/shared.service';
import { SettingsEventsDraftsService } from '../../_services/events-drafts.service';
import { ConfirmDialogComponent } from 'src/app/shared/dialogs/confirm/confirm.dialog';

@Component({
  selector: 'app-settings-new-event',
  templateUrl: './new-event.component.html',
  styleUrls: ['./new-event.component.scss']
})
export class SettingsNewEventComponent extends NotificationClass implements OnInit, OnDestroy {

  @ViewChild('stepper') stepper: MatStepper;
  @ViewChild('eventInfo') eventInfo: NewEventEventInfoComponent;
  @ViewChild('eventDate') eventDate: NewEventDateComponent;
  @ViewChild('eventVideo') eventVideo: NewEventVideoComponent;
  @ViewChild('eventMaterials') eventMaterials: NewEventSupportMaterialsComponent;
  @ViewChild('eventRequirements') eventRequirements: NewEventRequirementsComponent;
  @ViewChild('eventQuestions') eventQuestions: NewEventQuestionsComponent;

  public levels: Array<Level> = [];
  public newEvent = new Event();
  public stepIndex: number = 0;
  public loading: boolean = false;
  public allowEditing: boolean = false;
  public showDraftOptions: boolean = false;
  private _shouldFinish: boolean = true;

  constructor(
    protected _snackBar: MatSnackBar,
    private _router: Router,
    private _dialog: MatDialog,
    private _eventsService: SettingsEventsService,
    private _draftsService: SettingsEventsDraftsService,
    private _sharedService: SharedService
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    this._loadLevels();
    const eventStr = localStorage.getItem('editingEvent');
    if (eventStr && eventStr.trim() !== '') {
      this.newEvent = new Event( JSON.parse(eventStr) );

      this.allowEditing = true;

      const indexStr = localStorage.getItem('editingEventInitialIndex');
      if (indexStr && eventStr.trim() !== '') {
        const index = parseInt(indexStr, 10);
        this.stepIndex = index;
        this.stepper.selectedIndex = index;
      }
    }
  }

  public saveContent(): void {
    this._shouldFinish = this.stepIndex === 5;
    this.nextStep();
  }

  public nextStep(offset: number = 0) {
    switch (this.stepper.selectedIndex + offset) {
      case 0:
        this.eventInfo.nextStep(); break;
      case 1:
        this.eventDate.nextStep(); break;
      case 2:
        this.eventVideo.nextStep(); break;
      case 3:
        this.eventMaterials.nextStep(); break;
      case 4:
        this.eventRequirements.nextStep(); break;
      case 5:
        this.eventQuestions.nextStep(); break;
      default:
        break;
    }
  }

  public previousStep() {
    this.stepIndex--;
    this.stepper.previous();
  }

  public stepChanged(event, shouldFinish: boolean = true) {
    setTimeout(() => {
      if (event.previouslySelectedIndex < event.selectedIndex) {
        this._shouldFinish = shouldFinish;
        this.nextStep(-1);
      }
      if (this.stepIndex !== event.selectedIndex)
        this.stepIndex = event.selectedIndex;
    });
  }

  public setEventInfo(eventInfo: Event) {
    this.newEvent.setEventInfo(eventInfo);
    this.newEvent.id ?
      this._updateEventInfo(this.newEvent) :
      this._createNewEvent(eventInfo);
  }

  public setEventDates(schedules: Array<EventSchedule>) {
    this.newEvent.schedules = schedules;

    this._shouldFinish ?
      this._updateFooter() :
      this._shouldFinish = true;
  }

  public setEventVideo(eventVideo: Event) {
    this.newEvent.setVideoInfo(eventVideo);
    this._updateEventInfo( this.newEvent );
  }

  public addEventSupportMaterials(materials: Array<SupportMaterial>) {
    this.newEvent.supportMaterials = materials;
    this._updateSupportMaterials( this.newEvent.id, this.newEvent.supportMaterials );
  }

  public setRequirements(requirements: Array<Array<Requirement>>) {
    this.newEvent.requiredModules = requirements[0];
    this.newEvent.optionalModules = requirements[1];
    const requirmentsList = [ ...requirements[0], ...requirements[1] ];
    requirmentsList.forEach((req) => { delete req.module; delete req.editing; });
    this._updateRequirements(this.newEvent.id, requirmentsList);
  }

  public addEventQuestions(questions) {
    this.newEvent.prepQuizQuestionList = questions;
    this._updateEventInfo( this.newEvent, true );
  }

  public publishDraftChanges(): void {
    const dialogRef = this._dialog.open(ConfirmDialogComponent, {
      width: '400px',
      data: { message: 'Tem certeza que deseja publicar as alterações? O evento será substituído pela versão em rascunho.' }
    });

    dialogRef.afterClosed().subscribe((result: boolean) => {
      if (result) {
        this._draftsService.publishEventDraft(
          this.newEvent.id
        ).subscribe(() => {
          this.notify('Evento publicado com sucesso!');
          this._router.navigate([ 'configuracoes/eventos' ]);

        }, (error) => this.notify( this.getErrorNotification(error) ));
      }
    });
  }

  public rejectDraftChanges(): void {
    const dialogRef = this._dialog.open(ConfirmDialogComponent, {
      width: '400px',
      data: { message: 'Tem certeza que deseja rejeitar as alterações em rascunho? Todas as alterações serão perdidas.' }
    });

    dialogRef.afterClosed().subscribe((result: boolean) => {
      if (result) {
        this._draftsService.rejectEventDraft(
          this.newEvent.id
        ).subscribe(() => {
          this.notify('Alterações rejeitadas com sucesso!');
          this._router.navigate([ 'configuracoes/eventos' ]);

        }, (error) => this.notify( this.getErrorNotification(error) ));
      }
    });
  }

  private _createNewEvent(event: Event) {
    this.loading = true;
    this._draftsService.addNewEventDraft(event).subscribe((response) => {
      this.newEvent.id = response.data.id;
      this.newEvent.ecommerceId = response.data.ecommerceId;

      this._updateFooter();
      this.loading = false;

    }, (response) => this._errorHandlingFunc(response) );
  }

  private _updateEventInfo(event: Event, finished: boolean = false) {
    this.loading = true;
    this._draftsService.updateEventDraft(event).subscribe((response) => {
      this.newEvent.id = response.data.id;
      this.newEvent.ecommerceId = response.data.ecommerceId;
      this.eventInfo.setEcommerceId( this.newEvent.ecommerceId );

      if (this._shouldFinish) {
        if (finished) {
          this._dialog.open(CreatedEventDialogComponent);
          this._router.navigate([ 'configuracoes/eventos' ]);

        } else {
          this.newEvent.id = response.data.id;
          this._updateFooter();
        }
      } else {
        this._shouldFinish = true;
      }

      this.loading = false;
    }, (response) => this._errorHandlingFunc(response) );
  }

  private _updateSupportMaterials(eventId: string, materials: Array<SupportMaterial>) {
    this.loading = true;
    this._draftsService.manageEventDraftSupportMaterials(eventId, materials).subscribe(() => {
      this._shouldFinish ?
        this._updateFooter() :
        this._shouldFinish = true;
      this.loading = false;

    }, (response) => this._errorHandlingFunc(response) );
  }

  private _updateRequirements(eventId: string, requirements: Array<Requirement>) {
    this.loading = true;
    this._draftsService.manageEventDraftRequirements(eventId, requirements).subscribe(() => {
      this._shouldFinish ?
        this._updateFooter() :
        this._shouldFinish = true;
      this.loading = false;

    }, (response) => this._errorHandlingFunc(response) );
  }

  private _updateFooter() {
    this.stepIndex++;
    this.stepper.next();
  }

  private _errorHandlingFunc(response) {
    this.loading = false;
    this.notify(
      this.getErrorNotification(response)
    );
  }

  private _loadLevels(): void {
    this._sharedService.getLevels().subscribe((response) => {
      this.levels = response.data;

    }, () => this.notify('Ocorreu um erro, por favor tente novamente mais tarde') );
  }

  ngOnDestroy() {
    localStorage.removeItem('editingEventInitialIndex');
  }

}
