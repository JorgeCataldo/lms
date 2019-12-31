import { Component, Input, AfterViewInit } from '@angular/core';
import { TrackStudentOverview } from 'src/app/models/track-overview.interface';
import * as Chart from 'chart.js';

@Component({
  selector: 'app-track-overview-performance-radar',
  template: `
    <div class="performance" >
      <p class="title" >
        {{ title }}
      </p>
      <div class="content" >
        <div class="totals" *ngIf="showLegends" >
          <p class="points" *ngIf="hasTotalPoints" >
            <b>{{ track?.student?.points }}</b><br>
            <span>PONTUAÇÃO TOTAL</span>
          </p>
          <div class="count" >
            <p>{{ track?.modulesConfiguration.length }}</p>
            <p>
              MÓDULOS TOTAL NA TRILHA
            </p>
          </div>
          <div class="count" >
            <p>{{ track?.student?.unachievedGoals }}</p>
            <p>
              OBJETIVOS NÃO ALCANÇADOS
            </p>
          </div>
          <div class="count" >
            <p>{{ track?.student?.achievedGoals }}</p>
            <p>
              OBJETIVOS ALCANÇADOS
            </p>
          </div>
          <div class="legends" >
            <ng-container *ngFor="let data of dataset">
              <div class="legend" >
                <div class="square" [ngStyle]="{'background': data.backgroundColor}"></div>
                {{data.label}}
              </div>
            </ng-container>
          </div>
        </div>
        <canvas
          id="radarCanvas" width="95%" height="95%"
          [ngClass]="{ 'no-legends': !showLegends }"
        ></canvas>
      </div>
    </div>`,
  styleUrls: ['./performance-radar.component.scss']
})
export class TrackOverviewPerformanceRadarComponent implements AfterViewInit {

  @Input() readonly title: string;
  @Input() readonly track: TrackStudentOverview;
  @Input() readonly labels: Array<number>;
  @Input() readonly titleCallback: Function;
  @Input() readonly dataset: any;
  @Input() readonly tooltipCallback: Function;
  @Input() readonly showLegends: boolean = true;
  @Input() readonly hasTotalPoints: boolean = true;

  public chart: any;

  ngAfterViewInit() {
    this.chart = new Chart('radarCanvas', {
      type: 'radar',
      data: {
        labels: this.labels,
        datasets: this.dataset
      },
      options: {
        legend: { display: false },
        tooltips: {
          enabled: true,
          titleFontSize: 20,
          bodyFontSize: 24,
          callbacks: {
            title: this.titleCallback,
            label: this.tooltipCallback
          }
        },
        scale: {
          ticks: {
            display: false,
            max: 4
          },
          pointLabels: {
            fontSize: 26,
            fontColor: '#c4c4c4',
            fontStyle: 'bold'
          }
        },
        scales: {
          xAxes: [{ display: false }],
          yAxes: [{ display: false }],
        }
      }
    });
  }
}
