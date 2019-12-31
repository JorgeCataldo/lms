import { Component, Input, OnInit } from '@angular/core';
import { EventApplication } from '../../../models/event-application.model';

@Component({
  selector: 'app-settings-applications-header',
  template: `
    <div class="header" >
      <div class="title" >
        <h3>{{eventTitle}}</h3>
        <p>{{eventDate | date : 'dd/MM/yyyy'}}</p>
      </div>
      <div class="application-period" >
        <p>
          <small>Período de Inscrição</small><br>
          {{scheduleStartDate | date : 'dd/MM'}} - {{scheduleEndDate | date : 'dd/MM'}}
        </p>
        <app-progress-bar
          [completedPercentage]="percentage"
          [height]="18"
        ></app-progress-bar>
      </div>
    </div>`,
  styleUrls: ['./applications-header.component.scss']
})
export class SettingsApplicationsHeaderComponent {

  @Input() readonly eventTitle: string;
  @Input() readonly eventDate: Date;
  @Input() readonly scheduleStartDate: Date;
  @Input() readonly scheduleEndDate: Date;
  @Input() readonly percentage: number = 0;
}
