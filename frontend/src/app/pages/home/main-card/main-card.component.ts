import { Component, Input } from '@angular/core';
import { Router } from '@angular/router';
import { EventPreview } from '../../../models/previews/event.interface';
import * as moment from 'moment/moment';
import { ModuleProgress } from 'src/app/models/previews/module-progress.interface';

@Component({
  selector: 'app-main-card',
  template: `
    <div class="module" (click)="goToEvent()"
      [style.backgroundImage]="'url('+ event.imageUrl +')'"
      [ngClass]="{ 'last': isLast }"
    >
      <p class="subscription" >
        <small>Inscrições Terminam em</small><br>
        <span>{{ getSubscriptionDueDays() }}</span>
      </p>

      <p class="title" >
        {{ event.title }}<br>
        <small>{{ event.instructor }}</small>
      </p>

      <div class="date" >
        <p class="course-date" >
          <small>Data do curso</small><br>
          {{ event.nextSchedule ? (event.nextSchedule.eventDate | date : 'dd/MM/yyyy') : '--' }}
        </p>
        <p *ngIf="event.requirements.length > 0">
          <span [ngClass]="{ 'completed': getRemainingRequirements() === event.requirements.length }" >
            {{ getRemainingRequirements() }} de {{ event.requirements.length }}
          </span> requisitos
        </p>
      </div>
      <div class="bg-shadow" ></div>
    </div>`,
  styleUrls: ['./main-card.component.scss']
})
export class MainCardComponent {

  @Input() event: EventPreview;
  @Input() isLast: boolean = false;

  constructor(private _router: Router) { }

  public getSubscriptionDueDays(): string {
    if (this.event.nextSchedule) {
      const momentDueDate = moment(this.event.nextSchedule.subscriptionEndDate).startOf('day');
      const momentToday = moment().startOf('day');
      const diff = moment.duration(momentDueDate.diff(momentToday)).asDays();
      if (diff > 0)
        return diff + ' dias';
      else
        return 'Inscr. Enc.';
    }
    return '--';
  }

  public getRemainingRequirements(): number {
    let completedRequirements: number = 0;
    this.event.requirements.forEach(req => {
      if (!req.optional) {
        const progress: ModuleProgress = this.event.moduleProgressInfo[req.moduleId];
        if (progress && req.level + 1 <= progress.level)
          completedRequirements++;
      }
    });
    return completedRequirements;
  }

  public goToEvent() {
    this._router.navigate(['evento/' + this.event.id + '/' + this.event.nextSchedule.id]);
  }

}
