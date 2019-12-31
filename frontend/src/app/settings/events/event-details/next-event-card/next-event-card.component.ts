import { Component, Input, Output, EventEmitter } from '@angular/core';
import { EventPreview } from 'src/app/models/previews/event.interface';
import { EventSchedule } from 'src/app/models/event-schedule.model';
import { ActivatedRoute } from '@angular/router';
import { NotificationClass } from 'src/app/shared/classes/notification';
import { MatSnackBar } from '@angular/material';
import { SettingsEventsService } from 'src/app/settings/_services/events.service';

@Component({
  selector: 'app-settings-next-event-card',
  template: `
    <div class="next-event-card" >
      <div class="content" >
        <div class="header" >
          {{ schedule.eventDate | date : 'dd/MM/yyyy' }}
          <p class="published" >
            <select [(ngModel)]="schedule.published" (change)="objChanged()">
              <option [ngValue]="true">Publicado</option>
              <option [ngValue]="false">Oculto</option>
            </select>
          </p>
        </div>
        <div style="display: flex;">
          <p class="subscriptions" >
            Inscrições<br>
            <span>
              {{ schedule.subscriptionStartDate | date : 'dd/MM' }} -
              {{ schedule.subscriptionEndDate | date : 'dd/MM' }}
              </span>
          </p>
          <p class="subscriptions" *ngIf="schedule.location" style="margin-left:20px;" >
            Localização<br>
            <span>
              {{ schedule.location.name }}
              </span>
          </p>
        </div>
        <div class="status" >
          <p><span>{{ schedule.usersTotal - schedule.approvedUsersTotal - schedule.rejectedUsersTotal }}</span> pendentes</p>
          <p><span>{{ schedule.approvedUsersTotal }}</span> aprovadas</p>
          <p><span>{{ schedule.rejectedUsersTotal }}</span> recusadas</p>
        </div>
      </div>
      <div class="actions" >
        <button class="accent" (click)="manageDate.emit()" >
          EDITAR DATA E HORA
        </button>
        <button class="primary" (click)="manageApplications.emit(schedule.id)" >
          GERENCIAR INSCRIÇÕES
        </button>
      </div>
    </div>`,
  styleUrls: ['./next-event-card.component.scss']
})
export class SettingsNextEventCardComponent extends NotificationClass {

  @Input() schedule: EventSchedule;
  @Output() manageDate = new EventEmitter<EventPreview>();
  @Output() manageApplications = new EventEmitter<string>();

  private _isChanging: boolean = false;

  constructor(
    protected _snackBar: MatSnackBar,
    private _eventsService: SettingsEventsService,
    private _activatedRoute: ActivatedRoute
  ) {
    super(_snackBar);
  }

  public objChanged() {
    if (!this._isChanging) {
      this._isChanging = true;
      this.changeEventScheduleStatus();
    }
  }

  public changeEventScheduleStatus(): void {
    this._eventsService.changeEventScheduleStatus(this._activatedRoute.snapshot.paramMap.get('eventId'), this.schedule.id).subscribe(() => {
      this.notify('Status alterado com sucesso');
      this._isChanging = false;
    }, () => {
      this.notify('Ocorreu um erro ao alterar status, por favor tente novamente mais tarde');
      this.schedule.published = !this.schedule.published;
      this._isChanging = false;
    });
  }

}
