import { Component, Input, Output, EventEmitter } from '@angular/core';
import { EventPreview } from '../../../models/previews/event.interface';
import { AuthService } from 'src/app/shared/services/auth.service';

@Component({
  selector: 'app-settings-event-card',
  template: `
    <div class="event-card" >
      <img class="main-img" [src]="event.imageUrl" />

      <div class="draft-tag" [ngClass]="{ 'draft': event.isDraft }" >
        {{ event.isDraft ? 'RASCUNHO': 'PUBLICADO' }}
      </div>

      <div class="preview" >
        <div>
          <h3>
            {{ event.title }}<br>
            <small *ngIf="isAdmin" >
              Id: {{ event.id }}
            </small>
          </h3>
        </div>
      </div>

      <div class="edit">
        <img *ngIf="!isManagementPage"
          src="./assets/img/edit.png"
          (click)="editEvent.emit(event)"
        />
        <img *ngIf="isManagementPage"
          src="./assets/img/view.png"
          (click)="editEvent.emit(event)"
        />
        <img style="margin-top: 24px;"
          src="./assets/img/trash.png"
          (click)="deleteEvent.emit(event)"
        />
      </div>
    </div>`,
  styleUrls: ['./event-card.component.scss']
})
export class SettingsEventCardComponent {

  @Input() readonly event: EventPreview;
  @Input() readonly isManagementPage: boolean;
  @Output() editEvent = new EventEmitter<EventPreview>();
  @Output() deleteEvent = new EventEmitter<EventPreview>();

  public isAdmin: boolean = false;

  constructor(
    private _authService: AuthService
  ) {
    this.isAdmin = this._authService.isAdmin();
  }

}
